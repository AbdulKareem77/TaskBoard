using Dapper;
using Microsoft.Data.SqlClient;

namespace TaskBoard.Infrastructure.Repositories;

public record DomainEventOutboxItem(
    Guid Id,
    string EventName,
    Guid EntityId,
    string EntityType,
    string Payload,
    string Status,
    int Attempts,
    DateTime DateCreated);

public interface IDomainEventOutboxRepository
{
    Task InsertAsync(string eventName, Guid entityId, string entityType, string payload, SqlTransaction transaction);
    Task<IEnumerable<DomainEventOutboxItem>> GetPendingAsync(int maxAttempts);
    Task MarkProcessingAsync(Guid id);
    Task MarkProcessedAsync(Guid id);
    Task MarkFailedOrRetryAsync(Guid id, int currentAttempts, int maxAttempts);
}

public class DomainEventOutboxRepository : SqlRepositoryBase, IDomainEventOutboxRepository
{
    public DomainEventOutboxRepository(string connectionString) : base(connectionString) { }

    public async Task InsertAsync(string eventName, Guid entityId, string entityType, string payload, SqlTransaction transaction)
    {
        var sql = LoadSql("DomainEvents_InsertOutbox.sql");
        var conn = transaction.Connection!;
        await conn.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            EventName = eventName,
            EntityId = entityId,
            EntityType = entityType,
            Payload = payload
        }, transaction);
    }

    public async Task<IEnumerable<DomainEventOutboxItem>> GetPendingAsync(int maxAttempts)
    {
        const string sql = @"
            SELECT TOP 50 Id, EventName, EntityId, EntityType, Payload, Status, Attempts, DateCreated
            FROM dbo.DomainEventOutbox
            WHERE Status = 'Pending' AND Attempts < @MaxAttempts
            ORDER BY DateCreated ASC";

        using var conn = CreateConnection();
        return await conn.QueryAsync<DomainEventOutboxItem>(sql, new { MaxAttempts = maxAttempts });
    }

    public async Task MarkProcessingAsync(Guid id)
    {
        const string sql = "UPDATE dbo.DomainEventOutbox SET Status = 'Processing', Attempts = Attempts + 1 WHERE Id = @Id";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task MarkProcessedAsync(Guid id)
    {
        const string sql = "UPDATE dbo.DomainEventOutbox SET Status = 'Pending', DateProcessed = SYSUTCDATETIME() WHERE Id = @Id";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task MarkFailedOrRetryAsync(Guid id, int currentAttempts, int maxAttempts)
    {
        var newStatus = currentAttempts >= maxAttempts ? "Failed" : "Pending";
        const string sql = "UPDATE dbo.DomainEventOutbox SET Status = @Status WHERE Id = @Id";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Status = newStatus, Id = id });
    }
}
