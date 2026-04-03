namespace TaskBoard.NotificationConsumer.Services;

public interface INotificationDeliveryStrategy
{
    Task DeliverAsync(Guid userId, string title, string message, Guid? referenceId, string notificationType);
}
