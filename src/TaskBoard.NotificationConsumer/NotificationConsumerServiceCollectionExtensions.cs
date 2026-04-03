using Microsoft.Extensions.DependencyInjection;
using TaskBoard.NotificationConsumer.Handlers;
using TaskBoard.NotificationConsumer.Services;
using TaskBoard.NotificationConsumer.Workers;

namespace TaskBoard.NotificationConsumer;

public static class NotificationConsumerServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationConsumer(this IServiceCollection services)
    {
        // Background workers
        services.AddHostedService<DomainEventWorker>();
        services.AddHostedService<ReportGeneratorWorker>();

        // Event handlers
        services.AddScoped<TaskAssignedEventHandler>();
        services.AddScoped<TaskCreatedEventHandler>();
        services.AddScoped<TaskUpdatedEventHandler>();

        // Notification strategies
        services.AddScoped<INotificationDeliveryStrategy, InAppNotificationStrategy>();
        services.AddScoped<InAppNotificationStrategy>();
        services.AddScoped<EmailNotificationStrategy>();
        services.AddScoped<NotificationStrategyFactory>();

        // Change detection
        services.AddScoped<ChangeDetectionService>();

        return services;
    }
}
