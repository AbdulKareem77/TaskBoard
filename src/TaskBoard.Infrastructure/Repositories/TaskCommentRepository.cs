using Dapper;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Repositories;

public interface ITaskCommentRepository
{
    Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskItemId);
    Task<TaskComment?> GetByIdAsync(Guid commentId);
    Task InsertAsync(TaskComment comment);
}

public class TaskCommentRepository : SqlRepositoryBase, ITaskCommentRepository
{
    public TaskCommentRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskItemId)
    {
        var sql = LoadSql("TaskComments_GetByTaskId.sql");
        using var conn = CreateConnection();
        return await conn.QueryAsync<TaskComment>(sql, new { TaskItemId = taskItemId });
    }

    public async Task<TaskComment?> GetByIdAsync(Guid commentId)
    {
        const string sql = @"
            SELECT c.Id, c.TaskItemId, c.UserId, c.Content, c.DateCreated,
                   CONCAT(u.FirstName, ' ', u.LastName) AS AuthorFullName
            FROM dbo.TaskComments c
            JOIN dbo.Users u ON u.Id = c.UserId
            WHERE c.Id = @CommentId";

        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<TaskComment>(sql, new { CommentId = commentId });
    }

    public async Task InsertAsync(TaskComment comment)
    {
        var sql = LoadSql("TaskComments_Insert.sql");
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            Id = comment.Id,
            TaskItemId = comment.TaskItemId,
            UserId = comment.UserId,
            Content = comment.Content
        });
    }
}
