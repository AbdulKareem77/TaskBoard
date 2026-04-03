using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskBoard.Infrastructure.Repositories;
using TaskBoard.NotificationConsumer.Handlers;

namespace TaskBoard.NotificationConsumer.Workers;

public class DomainEventWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);
    private const int MaxAttempts = 3;

    public DomainEventWorker(IServiceProvider serviceProvider, ILogger<DomainEventWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // yield immediately so host startup is not blocked

        _logger.LogInformation("DomainEventWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DomainEventWorker polling loop.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("DomainEventWorker stopped.");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IDomainEventOutboxRepository>();

        var pendingEvents = (await outboxRepo.GetPendingAsync(MaxAttempts)).ToList();
        if (!pendingEvents.Any())
            return;

        _logger.LogInformation("Processing {Count} pending domain events.", pendingEvents.Count);

        foreach (var evt in pendingEvents)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                await outboxRepo.MarkProcessingAsync(evt.Id);
                await RouteEventAsync(scope.ServiceProvider, evt.EventName, evt.Payload);
                await outboxRepo.MarkProcessedAsync(evt.Id);
                _logger.LogInformation("Processed event {EventName} (Id={Id}).", evt.EventName, evt.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event {EventName} (Id={Id}), attempt {Attempt}.",
                    evt.EventName, evt.Id, evt.Attempts + 1);

                await outboxRepo.MarkFailedOrRetryAsync(evt.Id, evt.Attempts, MaxAttempts);
            }
        }
    }

    private async Task RouteEventAsync(IServiceProvider services, string eventName, string payload)
    {
        switch (eventName)
        {
            case "TaskCreated":
                var taskCreatedHandler = services.GetRequiredService<TaskCreatedEventHandler>();
                await taskCreatedHandler.HandleAsync(payload);
                break;

            case "TaskUpdated":
                var taskUpdatedHandler = services.GetRequiredService<TaskUpdatedEventHandler>();
                await taskUpdatedHandler.HandleAsync(payload);
                break;

            case "TaskAssigned":
                var taskAssignedHandler = services.GetRequiredService<TaskAssignedEventHandler>();
                await taskAssignedHandler.HandleAsync(payload);
                break;

            case "TaskDeleted":
                // No notification needed for deletion in current spec
                _logger.LogInformation("TaskDeleted event received, no notification required.");
                break;

            default:
                _logger.LogWarning("Unknown event type: {EventName}. Skipping.", eventName);
                break;
        }
    }
}
