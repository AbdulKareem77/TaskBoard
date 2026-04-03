using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskBoard.Domain.Events;
using TaskBoard.Infrastructure.Repositories;
using TaskBoard.NotificationConsumer.Services;

namespace TaskBoard.NotificationConsumer.Handlers;

public class TaskCreatedEventHandler
{
    private readonly IProjectRepository _projectRepository;
    private readonly INotificationDeliveryStrategy _deliveryStrategy;
    private readonly ILogger<TaskCreatedEventHandler> _logger;

    public TaskCreatedEventHandler(
        IProjectRepository projectRepository,
        INotificationDeliveryStrategy deliveryStrategy,
        ILogger<TaskCreatedEventHandler> logger)
    {
        _projectRepository = projectRepository;
        _deliveryStrategy = deliveryStrategy;
        _logger = logger;
    }

    public async Task HandleAsync(string payload)
    {
        TaskCreatedEvent? evt;
        try
        {
            evt = JsonSerializer.Deserialize<TaskCreatedEvent>(payload);
            if (evt == null)
            {
                _logger.LogWarning("Failed to deserialize TaskCreatedEvent payload.");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing TaskCreatedEvent.");
            return;
        }

        // Get all project members except the creator
        var members = await _projectRepository.GetMembersAsync(evt.ProjectId);
        var recipients = members.Where(m => m.UserId != evt.CreatedByUserId).ToList();

        foreach (var member in recipients)
        {
            await _deliveryStrategy.DeliverAsync(
                userId: member.UserId,
                title: "New Task Created",
                message: $"A new task '{evt.Title}' has been created in the project.",
                referenceId: evt.TaskId,
                notificationType: "TaskCreated");
        }

        _logger.LogInformation("TaskCreated notification sent to {Count} members for task {TaskId}.",
            recipients.Count, evt.TaskId);
    }
}
