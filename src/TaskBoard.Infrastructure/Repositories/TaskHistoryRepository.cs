using Dapper;
using Microsoft.Data.SqlClient;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Repositories;

public interface ITaskHistoryRepository
{
    Task InsertAsync(TaskHistory history, SqlTransaction? transaction = null);
}

public class TaskHistoryRepository : SqlRepositoryBase, ITaskHistoryRepository
{
    public TaskHistoryRepository(string connectionString) : base(connectionString) { }

    public async Task InsertAsync(TaskHistory history, SqlTransaction? transaction = null)
    {
        var sql = LoadSql("TaskHistory_Insert.sql");

        if (transaction != null)
        {
            var conn = transaction.Connection!;
            await conn.ExecuteAsync(sql, new
            {
                Id = history.Id == Guid.Empty ? Guid.NewGuid() : history.Id,
                TaskItemId = history.TaskItemId,
                UserId = history.UserId,
                Action = history.Action,
                OldValue = history.OldValue,
                NewValue = history.NewValue
            }, transaction);
        }
        else
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new
            {
                Id = history.Id == Guid.Empty ? Guid.NewGuid() : history.Id,
                TaskItemId = history.TaskItemId,
                UserId = history.UserId,
                Action = history.Action,
                OldValue = history.OldValue,
                NewValue = history.NewValue
            });
        }
    }
}
