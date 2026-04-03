using Microsoft.Extensions.Logging;

namespace TaskBoard.NotificationConsumer.Services;

public class EmailNotificationStrategy : INotificationDeliveryStrategy
{
    private readonly ILogger<EmailNotificationStrategy> _logger;

    public EmailNotificationStrategy(ILogger<EmailNotificationStrategy> logger)
    {
        _logger = logger;
    }

    public Task DeliverAsync(Guid userId, string title, string message, Guid? referenceId, string notificationType)
    {
        _logger.LogInformation(
            "Would send email to userId={UserId}: [{Type}] {Title} — {Message}",
            userId, notificationType, title, message);

        return Task.CompletedTask;
    }
}
