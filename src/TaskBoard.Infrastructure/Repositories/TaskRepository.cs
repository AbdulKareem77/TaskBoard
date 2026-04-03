using Dapper;
using Microsoft.Data.SqlClient;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Repositories;

public record PagedItems<T>(IEnumerable<T> Items, int TotalCount);

public interface ITaskRepository
{
    Task<PagedItems<TaskItem>> GetByProjectIdAsync(Guid projectId, int page, int pageSize, string? status);
    Task<TaskItem?> GetByIdAsync(Guid taskId);
    Task<Guid> InsertAsync(TaskItem task, SqlTransaction transaction);
    Task<int> UpdateAsync(TaskItem task);
    Task DeleteAsync(Guid taskId);
    Task<IEnumerable<TaskItem>> GetByProjectIdsAsync(IEnumerable<Guid> projectIds);
    Task<PagedItems<TaskItem>> SearchAsync(string searchTerm, Guid userId, Guid? projectId, int page, int pageSize);
    Task UpsertAssignmentAsync(Guid taskId, Guid userId, SqlTransaction? transaction = null);
    Task RemoveAssignmentAsync(Guid taskId, Guid userId, SqlTransaction? transaction = null);
}

public class TaskRepository : SqlRepositoryBase, ITaskRepository
{
    public TaskRepository(string connectionString) : base(connectionString) { }

    public async Task<PagedItems<TaskItem>> GetByProjectIdAsync(Guid projectId, int page, int pageSize, string? status)
    {
        var sql = LoadSql("Tasks_GetByProjectId.sql");
        using var conn = CreateConnection();

        var rows = await conn.QueryAsync<dynamic>(sql, new
        {
            ProjectId = projectId,
            Status = status,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });

        var taskDict = new Dictionary<Guid, TaskItem>();
        int totalCount = 0;

        foreach (var row in rows)
        {
            Guid id = (Guid)row.Id;
            if (!taskDict.TryGetValue(id, out var task))
            {
                task = new TaskItem
                {
                    Id = (Guid)row.Id,
                    ProjectId = (Guid)row.ProjectId,
                    Title = (string)row.Title,
                    Description = row.Description as string,
                    Status = (string)row.Status,
                    Priority = row.Priority as string,
                    DueDate = row.DueDate as DateTime?,
                    CreatedByUserId = (Guid)row.CreatedByUserId,
                    RowVersion = (int)row.RowVersion,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    Assignees = new List<TaskAssignment>()
                };
                taskDict[id] = task;
                totalCount = (int)(row.TotalCount ?? 0);
            }

            Guid? assigneeUserId = row.AssigneeUserId as Guid?;
            if (assigneeUserId.HasValue && !task.Assignees.Any(a => a.UserId == assigneeUserId.Value))
            {
                task.Assignees.Add(new TaskAssignment
                {
                    TaskItemId = id,
                    UserId = assigneeUserId.Value,
                    FullName = row.AssigneeFullName as string ?? string.Empty
                });
            }
        }

        return new PagedItems<TaskItem>(taskDict.Values, totalCount);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid taskId)
    {
        var sql = LoadSql("Tasks_GetById.sql");
        using var conn = CreateConnection();

        var rows = await conn.QueryAsync<dynamic>(sql, new { TaskId = taskId });

        TaskItem? result = null;

        foreach (var row in rows)
        {
            if (result == null)
            {
                result = new TaskItem
                {
                    Id = (Guid)row.Id,
                    ProjectId = (Guid)row.ProjectId,
                    Title = (string)row.Title,
                    Description = row.Description as string,
                    Status = (string)row.Status,
                    Priority = row.Priority as string,
                    DueDate = row.DueDate as DateTime?,
                    CreatedByUserId = (Guid)row.CreatedByUserId,
                    CreatedByFullName = row.CreatedByFullName as string,
                    RowVersion = (int)row.RowVersion,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    Assignees = new List<TaskAssignment>(),
                    History = new List<TaskHistory>()
                };
            }

            Guid? assigneeUserId = row.AssigneeUserId as Guid?;
            if (assigneeUserId.HasValue && !result.Assignees.Any(a => a.UserId == assigneeUserId.Value))
            {
                result.Assignees.Add(new TaskAssignment
                {
                    TaskItemId = (Guid)row.Id,
                    UserId = assigneeUserId.Value,
                    FullName = row.AssigneeFullName as string ?? string.Empty
                });
            }

            Guid? historyId = row.HistoryId as Guid?;
            if (historyId.HasValue && !result.History.Any(h => h.Id == historyId.Value))
            {
                result.History.Add(new TaskHistory
                {
                    Id = historyId.Value,
                    TaskItemId = (Guid)row.Id,
                    Action = row.HistoryAction as string ?? string.Empty,
                    OldValue = row.OldValue as string,
                    NewValue = row.NewValue as string,
                    DateCreated = row.HistoryDate as DateTime? ?? DateTime.MinValue,
                    UserName = row.HistoryUserName as string ?? string.Empty
                });
            }
        }

        return result;
    }

    public async Task<Guid> InsertAsync(TaskItem task, SqlTransaction transaction)
    {
        var sql = LoadSql("Tasks_Insert.sql");
        var conn = transaction.Connection!;
        await conn.ExecuteAsync(sql, new
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedByUserId = task.CreatedByUserId
        }, transaction);
        return task.Id;
    }

    public async Task<int> UpdateAsync(TaskItem task)
    {
        var sql = LoadSql("Tasks_Update.sql");
        using var conn = CreateConnection();
        var rowsAffected = await conn.ExecuteScalarAsync<int>(sql, new
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            ExpectedVersion = task.RowVersion
        });
        return rowsAffected;
    }

    public async Task DeleteAsync(Guid taskId)
    {
        const string sql = @"
            DELETE FROM dbo.TaskHistory WHERE TaskItemId = @TaskId;
            DELETE FROM dbo.TaskAssignments WHERE TaskItemId = @TaskId;
            DELETE FROM dbo.TaskComments WHERE TaskItemId = @TaskId;
            DELETE FROM dbo.TaskItems WHERE Id = @TaskId;";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { TaskId = taskId });
    }

    public async Task<IEnumerable<TaskItem>> GetByProjectIdsAsync(IEnumerable<Guid> projectIds)
    {
        var idList = projectIds.ToList();
        if (!idList.Any()) return Enumerable.Empty<TaskItem>();

        var sql = LoadSql("Tasks_GetByProjectIds.sql");
        using var conn = CreateConnection();

        var rows = await conn.QueryAsync<dynamic>(sql, new { ProjectIds = idList });

        var taskDict = new Dictionary<Guid, TaskItem>();

        foreach (var row in rows)
        {
            Guid id = (Guid)row.Id;
            if (!taskDict.TryGetValue(id, out var task))
            {
                task = new TaskItem
                {
                    Id = (Guid)row.Id,
                    ProjectId = (Guid)row.ProjectId,
                    Title = (string)row.Title,
                    Description = row.Description as string,
                    Status = (string)row.Status,
                    Priority = row.Priority as string,
                    DueDate = row.DueDate as DateTime?,
                    CreatedByUserId = (Guid)row.CreatedByUserId,
                    RowVersion = (int)row.RowVersion,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    ProjectName = row.ProjectName as string,
                    Assignees = new List<TaskAssignment>(),
                    History = new List<TaskHistory>()
                };
                taskDict[id] = task;
            }

            Guid? assigneeUserId = row.AssigneeUserId as Guid?;
            if (assigneeUserId.HasValue && !task.Assignees.Any(a => a.UserId == assigneeUserId.Value))
            {
                task.Assignees.Add(new TaskAssignment
                {
                    TaskItemId = id,
                    UserId = assigneeUserId.Value,
                    FullName = row.AssigneeFullName as string ?? string.Empty
                });
            }

            Guid? historyId = row.HistoryId as Guid?;
            if (historyId.HasValue && !task.History.Any(h => h.Id == historyId.Value))
            {
                task.History.Add(new TaskHistory
                {
                    Id = historyId.Value,
                    TaskItemId = id,
                    Action = row.Action as string ?? string.Empty,
                    OldValue = row.OldValue as string,
                    NewValue = row.NewValue as string,
                    DateCreated = row.HistoryDate as DateTime? ?? DateTime.MinValue,
                    UserName = row.HistoryUserName as string ?? string.Empty
                });
            }
        }

        return taskDict.Values;
    }

    public async Task<PagedItems<TaskItem>> SearchAsync(string searchTerm, Guid userId, Guid? projectId, int page, int pageSize)
    {
        const string sql = @"
            SELECT t.Id, t.ProjectId, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
                   t.CreatedByUserId, t.RowVersion, t.DateCreated, t.DateUpdated,
                   ta.UserId AS AssigneeUserId,
                   CONCAT(au.FirstName, ' ', au.LastName) AS AssigneeFullName,
                   COUNT(*) OVER() AS TotalCount
            FROM dbo.TaskItems t
            LEFT JOIN dbo.TaskAssignments ta ON ta.TaskItemId = t.Id
            LEFT JOIN dbo.Users au ON au.Id = ta.UserId
            JOIN dbo.ProjectMembers pm ON pm.ProjectId = t.ProjectId AND pm.UserId = @UserId
            WHERE (t.Title LIKE @SearchPattern OR t.Description LIKE @SearchPattern)
              AND (@ProjectId IS NULL OR t.ProjectId = @ProjectId)
            ORDER BY t.DateCreated DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var conn = CreateConnection();

        var rows = await conn.QueryAsync<dynamic>(sql, new
        {
            SearchPattern = $"%{searchTerm}%",
            UserId = userId,
            ProjectId = projectId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });

        var taskDict = new Dictionary<Guid, TaskItem>();
        int totalCount = 0;

        foreach (var row in rows)
        {
            Guid id = (Guid)row.Id;
            if (!taskDict.TryGetValue(id, out var task))
            {
                task = new TaskItem
                {
                    Id = (Guid)row.Id,
                    ProjectId = (Guid)row.ProjectId,
                    Title = (string)row.Title,
                    Description = row.Description as string,
                    Status = (string)row.Status,
                    Priority = row.Priority as string,
                    DueDate = row.DueDate as DateTime?,
                    CreatedByUserId = (Guid)row.CreatedByUserId,
                    RowVersion = (int)row.RowVersion,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    Assignees = new List<TaskAssignment>()
                };
                taskDict[id] = task;
                totalCount = (int)(row.TotalCount ?? 0);
            }

            Guid? assigneeUserId = row.AssigneeUserId as Guid?;
            if (assigneeUserId.HasValue && !task.Assignees.Any(a => a.UserId == assigneeUserId.Value))
            {
                task.Assignees.Add(new TaskAssignment
                {
                    TaskItemId = id,
                    UserId = assigneeUserId.Value,
                    FullName = row.AssigneeFullName as string ?? string.Empty
                });
            }
        }

        return new PagedItems<TaskItem>(taskDict.Values, totalCount);
    }

    public async Task UpsertAssignmentAsync(Guid taskId, Guid userId, SqlTransaction? transaction = null)
    {
        var sql = LoadSql("TaskAssignments_Upsert.sql");
        if (transaction != null)
        {
            var conn = transaction.Connection!;
            await conn.ExecuteAsync(sql, new { TaskItemId = taskId, UserId = userId }, transaction);
        }
        else
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { TaskItemId = taskId, UserId = userId });
        }
    }

    public async Task RemoveAssignmentAsync(Guid taskId, Guid userId, SqlTransaction? transaction = null)
    {
        const string sql = "DELETE FROM dbo.TaskAssignments WHERE TaskItemId = @TaskId AND UserId = @UserId";
        if (transaction != null)
        {
            var conn = transaction.Connection!;
            await conn.ExecuteAsync(sql, new { TaskId = taskId, UserId = userId }, transaction);
        }
        else
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { TaskId = taskId, UserId = userId });
        }
    }
}
