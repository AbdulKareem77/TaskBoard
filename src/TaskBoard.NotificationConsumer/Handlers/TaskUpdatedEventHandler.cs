using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskBoard.Domain.Events;
using TaskBoard.Infrastructure.Repositories;
using TaskBoard.NotificationConsumer.Services;

namespace TaskBoard.NotificationConsumer.Handlers;

public class TaskUpdatedEventHandler
{
    private readonly ITaskRepository _taskRepository;
    private readonly INotificationDeliveryStrategy _deliveryStrategy;
    private readonly ChangeDetectionService _changeDetection;
    private readonly ILogger<TaskUpdatedEventHandler> _logger;

    public TaskUpdatedEventHandler(
        ITaskRepository taskRepository,
        INotificationDeliveryStrategy deliveryStrategy,
        ChangeDetectionService changeDetection,
        ILogger<TaskUpdatedEventHandler> logger)
    {
        _taskRepository = taskRepository;
        _deliveryStrategy = deliveryStrategy;
        _changeDetection = changeDetection;
        _logger = logger;
    }

    public async Task HandleAsync(string payload)
    {
        TaskUpdatedEvent? evt;
        try
        {
            evt = JsonSerializer.Deserialize<TaskUpdatedEvent>(payload);
            if (evt == null)
            {
                _logger.LogWarning("Failed to deserialize TaskUpdatedEvent payload.");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing TaskUpdatedEvent.");
            return;
        }

        // Hash-based deduplication: include old/new status
        var hash = _changeDetection.ComputeHash(
            evt.TaskId.ToString(),
            evt.OldStatus,
            evt.NewStatus,
            evt.OccurredAt.ToString("O"));

        if (!await _changeDetection.HasChangedAsync(evt.TaskId, "TaskUpdated", hash))
        {
            _logger.LogInformation("Duplicate TaskUpdatedEvent for task {TaskId}, skipping.", evt.TaskId);
            return;
        }

        // Load task to get assignees
        var task = await _taskRepository.GetByIdAsync(evt.TaskId);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for TaskUpdatedEvent.", evt.TaskId);
            return;
        }

        var statusChanged = evt.OldStatus != evt.NewStatus;
        var notificationMessage = statusChanged
            ? $"Task status changed from '{evt.OldStatus}' to '{evt.NewStatus}'."
            : "Task details have been updated.";

        foreach (var assignee in task.Assignees)
        {
            if (assignee.UserId == evt.UpdatedByUserId) continue; // Don't notify the updater

            await _deliveryStrategy.DeliverAsync(
                userId: assignee.UserId,
                title: "Task Updated",
                message: notificationMessage,
                referenceId: evt.TaskId,
                notificationType: "TaskUpdated");
        }

        await _changeDetection.UpdateHashAsync(evt.TaskId, "TaskUpdated", hash);
        _logger.LogInformation("TaskUpdated notification sent for task {TaskId}.", evt.TaskId);
    }
}
