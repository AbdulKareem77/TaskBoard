namespace TaskBoard.NotificationConsumer.Services;

public class NotificationStrategyFactory
{
    private readonly InAppNotificationStrategy _inApp;
    private readonly EmailNotificationStrategy _email;

    public NotificationStrategyFactory(
        InAppNotificationStrategy inApp,
        EmailNotificationStrategy email)
    {
        _inApp = inApp;
        _email = email;
    }

    public INotificationDeliveryStrategy GetStrategy(string? userPreference = null)
    {
        return userPreference switch
        {
            "Email" => _email,
            "Both" => new CompositeNotificationStrategy(_inApp, _email),
            _ => _inApp
        };
    }
}

internal class CompositeNotificationStrategy : INotificationDeliveryStrategy
{
    private readonly INotificationDeliveryStrategy[] _strategies;

    public CompositeNotificationStrategy(params INotificationDeliveryStrategy[] strategies)
    {
        _strategies = strategies;
    }

    public async Task DeliverAsync(Guid userId, string title, string message, Guid? referenceId, string notificationType)
    {
        foreach (var strategy in _strategies)
        {
            await strategy.DeliverAsync(userId, title, message, referenceId, notificationType);
        }
    }
}
