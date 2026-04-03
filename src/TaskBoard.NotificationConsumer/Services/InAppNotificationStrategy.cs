using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.NotificationConsumer.Services;

public class InAppNotificationStrategy : INotificationDeliveryStrategy
{
    private readonly INotificationRepository _notificationRepository;

    public InAppNotificationStrategy(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task DeliverAsync(Guid userId, string title, string message, Guid? referenceId, string notificationType)
    {
        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = notificationType,
            Title = title,
            Message = message,
            ReferenceId = referenceId,
            IsRead = false,
            DateCreated = DateTime.UtcNow
        };

        await _notificationRepository.InsertAsync(notification);
    }
}
