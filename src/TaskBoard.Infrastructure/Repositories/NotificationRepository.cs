using Dapper;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Repositories;

public interface INotificationRepository
{
    Task<Guid> InsertAsync(UserNotification notification);
    Task<(IEnumerable<UserNotification> Items, int TotalCount, int UnreadCount)> GetByUserIdAsync(Guid userId, bool unreadOnly, int page, int pageSize);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<string?> GetNotificationIndexHashAsync(Guid entityId, string entityType);
    Task UpsertNotificationIndexAsync(Guid entityId, string entityType, string hash);
}

public class NotificationRepository : SqlRepositoryBase, INotificationRepository
{
    public NotificationRepository(string connectionString) : base(connectionString) { }

    public async Task<Guid> InsertAsync(UserNotification notification)
    {
        var sql = LoadSql("Notifications_Insert.sql");
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            ReferenceId = notification.ReferenceId
        });
    }

    public async Task<(IEnumerable<UserNotification> Items, int TotalCount, int UnreadCount)> GetByUserIdAsync(
        Guid userId, bool unreadOnly, int page, int pageSize)
    {
        const string sql = @"
            SELECT n.Id, n.UserId, n.Type, n.Title, n.Message, n.ReferenceId, n.IsRead, n.DateCreated,
                   COUNT(*) OVER() AS TotalCount,
                   SUM(CASE WHEN n.IsRead = 0 THEN 1 ELSE 0 END) OVER() AS UnreadCount
            FROM dbo.UserNotifications n
            WHERE n.UserId = @UserId
              AND (@UnreadOnly = 0 OR n.IsRead = 0)
            ORDER BY n.DateCreated DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var conn = CreateConnection();
        int totalCount = 0;
        int unreadCount = 0;
        var items = new List<UserNotification>();

        var rows = await conn.QueryAsync<dynamic>(sql, new
        {
            UserId = userId,
            UnreadOnly = unreadOnly ? 1 : 0,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });

        foreach (var row in rows)
        {
            totalCount = (int)(row.TotalCount ?? 0);
            unreadCount = (int)(row.UnreadCount ?? 0);
            items.Add(new UserNotification
            {
                Id = row.Id,
                UserId = row.UserId,
                Type = row.Type,
                Title = row.Title,
                Message = row.Message,
                ReferenceId = row.ReferenceId,
                IsRead = row.IsRead,
                DateCreated = row.DateCreated
            });
        }

        return (items, totalCount, unreadCount);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        const string sql = "UPDATE dbo.UserNotifications SET IsRead = 1 WHERE Id = @Id AND UserId = @UserId";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = notificationId, UserId = userId });
    }

    public async Task<string?> GetNotificationIndexHashAsync(Guid entityId, string entityType)
    {
        const string sql = "SELECT CriticalFieldsHash FROM dbo.NotificationIndex WHERE EntityId = @EntityId AND EntityType = @EntityType";
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<string>(sql, new { EntityId = entityId, EntityType = entityType });
    }

    public async Task UpsertNotificationIndexAsync(Guid entityId, string entityType, string hash)
    {
        const string sql = @"
            MERGE dbo.NotificationIndex AS target
            USING (SELECT @EntityId AS EntityId, @EntityType AS EntityType) AS source
            ON target.EntityId = source.EntityId AND target.EntityType = source.EntityType
            WHEN MATCHED THEN
                UPDATE SET CriticalFieldsHash = @Hash, DateUpdated = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (EntityId, EntityType, CriticalFieldsHash, DateUpdated)
                VALUES (@EntityId, @EntityType, @Hash, SYSUTCDATETIME());";

        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { EntityId = entityId, EntityType = entityType, Hash = hash });
    }
}
