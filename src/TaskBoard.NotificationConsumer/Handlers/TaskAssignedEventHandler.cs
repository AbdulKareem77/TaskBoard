using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskBoard.Domain.Events;
using TaskBoard.NotificationConsumer.Services;

namespace TaskBoard.NotificationConsumer.Handlers;

public class TaskAssignedEventHandler
{
    private readonly INotificationDeliveryStrategy _deliveryStrategy;
    private readonly ChangeDetectionService _changeDetection;
    private readonly ILogger<TaskAssignedEventHandler> _logger;

    public TaskAssignedEventHandler(
        INotificationDeliveryStrategy deliveryStrategy,
        ChangeDetectionService changeDetection,
        ILogger<TaskAssignedEventHandler> logger)
    {
        _deliveryStrategy = deliveryStrategy;
        _changeDetection = changeDetection;
        _logger = logger;
    }

    public async Task HandleAsync(string payload)
    {
        TaskAssignedEvent? evt;
        try
        {
            evt = JsonSerializer.Deserialize<TaskAssignedEvent>(payload);
            if (evt == null)
            {
                _logger.LogWarning("Failed to deserialize TaskAssignedEvent payload.");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing TaskAssignedEvent.");
            return;
        }

        // Compute hash for deduplication
        var hash = _changeDetection.ComputeHash(
            evt.TaskId.ToString(),
            evt.AssigneeId.ToString(),
            evt.OccurredAt.ToString("O"));

        if (!await _changeDetection.HasChangedAsync(evt.TaskId, "TaskAssigned", hash))
        {
            _logger.LogInformation("Duplicate TaskAssignedEvent for task {TaskId}, skipping.", evt.TaskId);
            return;
        }

        // Deliver notification to assignee
        await _deliveryStrategy.DeliverAsync(
            userId: evt.AssigneeId,
            title: "Task Assigned to You",
            message: $"You have been assigned to task '{evt.TaskTitle}'.",
            referenceId: evt.TaskId,
            notificationType: "TaskAssigned");

        await _changeDetection.UpdateHashAsync(evt.TaskId, "TaskAssigned", hash);
        _logger.LogInformation("TaskAssigned notification delivered to user {AssigneeId}.", evt.AssigneeId);
    }
}
