using Microsoft.Data.SqlClient;
using TaskBoard.Domain.Events;

namespace TaskBoard.Infrastructure.Events;

public interface IDomainEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, SqlTransaction transaction);
}
