using System.Text.Json;
using Microsoft.Data.SqlClient;
using TaskBoard.Domain.Events;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Infrastructure.Events;

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IDomainEventOutboxRepository _outboxRepository;

    public DomainEventPublisher(IDomainEventOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public async Task PublishAsync(DomainEvent domainEvent, SqlTransaction transaction)
    {
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

        // Determine entityId from the event type
        var entityId = GetEntityId(domainEvent);
        var entityType = domainEvent.GetType().Name;

        await _outboxRepository.InsertAsync(
            domainEvent.EventName,
            entityId,
            entityType,
            payload,
            transaction);
    }

    private static Guid GetEntityId(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            TaskCreatedEvent e => e.TaskId,
            TaskUpdatedEvent e => e.TaskId,
            TaskAssignedEvent e => e.TaskId,
            TaskDeletedEvent e => e.TaskId,
            _ => domainEvent.EventId
        };
    }
}
