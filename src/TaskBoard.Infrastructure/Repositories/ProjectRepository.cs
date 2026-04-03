using Dapper;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Repositories;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId, bool isAdmin);
    Task<Project?> GetByIdAsync(Guid projectId);
    Task<Guid> InsertAsync(Project project);
    Task<IEnumerable<ProjectMember>> GetMembersAsync(Guid projectId);
    Task UpsertMemberAsync(Guid projectId, Guid userId, string role);
    Task RemoveMemberAsync(Guid projectId, Guid userId);
    Task<bool> IsMemberAsync(Guid projectId, Guid userId);
    Task<string?> GetMemberRoleAsync(Guid projectId, Guid userId);
}

public class ProjectRepository : SqlRepositoryBase, IProjectRepository
{
    public ProjectRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId, bool isAdmin)
    {
        var sql = LoadSql("Projects_GetByUserId.sql");
        using var conn = CreateConnection();
        var projects = await conn.QueryAsync<Project>(sql, new { UserId = userId, IsAdmin = isAdmin ? 1 : 0 });
        return projects;
    }

    public async Task<Project?> GetByIdAsync(Guid projectId)
    {
        const string sql = @"
            SELECT p.Id, p.Name, p.Description, p.OwnerId, p.IsArchived, p.DateCreated, p.DateUpdated,
                   CONCAT(u.FirstName, ' ', u.LastName) AS OwnerName,
                   (SELECT COUNT(*) FROM dbo.ProjectMembers pm2 WHERE pm2.ProjectId = p.Id) AS MemberCount,
                   (SELECT COUNT(*) FROM dbo.TaskItems t2 WHERE t2.ProjectId = p.Id) AS TaskCount
            FROM dbo.Projects p
            JOIN dbo.Users u ON u.Id = p.OwnerId
            WHERE p.Id = @ProjectId";

        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Project>(sql, new { ProjectId = projectId });
    }

    public async Task<Guid> InsertAsync(Project project)
    {
        var sql = LoadSql("Projects_Insert.sql");
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId
        });
    }

    public async Task<IEnumerable<ProjectMember>> GetMembersAsync(Guid projectId)
    {
        var sql = LoadSql("ProjectMembers_GetByProjectId.sql");
        using var conn = CreateConnection();
        return await conn.QueryAsync<ProjectMember>(sql, new { ProjectId = projectId });
    }

    public async Task UpsertMemberAsync(Guid projectId, Guid userId, string role)
    {
        var sql = LoadSql("ProjectMembers_Upsert.sql");
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { ProjectId = projectId, UserId = userId, Role = role });
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid userId)
    {
        const string sql = "DELETE FROM dbo.ProjectMembers WHERE ProjectId = @ProjectId AND UserId = @UserId";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { ProjectId = projectId, UserId = userId });
    }

    public async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.ProjectMembers WHERE ProjectId = @ProjectId AND UserId = @UserId";
        using var conn = CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(sql, new { ProjectId = projectId, UserId = userId });
        return count > 0;
    }

    public async Task<string?> GetMemberRoleAsync(Guid projectId, Guid userId)
    {
        const string sql = "SELECT Role FROM dbo.ProjectMembers WHERE ProjectId = @ProjectId AND UserId = @UserId";
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<string>(sql, new { ProjectId = projectId, UserId = userId });
    }
}
