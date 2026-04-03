using Dapper;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids);
}

public class UserRepository : SqlRepositoryBase, IUserRepository
{
    public UserRepository(string connectionString) : base(connectionString) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var sql = LoadSql("Users_GetByEmail.sql");
        using var conn = CreateConnection();
        var userDict = new Dictionary<Guid, User>();

        await conn.QueryAsync<User, dynamic, User>(
            sql,
            (user, roleRow) =>
            {
                if (!userDict.TryGetValue(user.Id, out var existingUser))
                {
                    existingUser = user;
                    existingUser.Roles = new List<Role>();
                    userDict[user.Id] = existingUser;
                }

                if (roleRow != null)
                {
                    var roleId = (Guid?)roleRow.RoleId;
                    var roleName = (string?)roleRow.RoleName;
                    if (roleId.HasValue && roleName != null)
                    {
                        existingUser.Roles.Add(new Role { Id = roleId.Value, Name = roleName });
                    }
                }

                return existingUser;
            },
            new { Email = email },
            splitOn: "RoleId"
        );

        return userDict.Values.FirstOrDefault();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var sql = LoadSql("Users_GetByEmail.sql").Replace("WHERE u.Email = @Email", "WHERE u.Id = @Id");
        using var conn = CreateConnection();
        var userDict = new Dictionary<Guid, User>();

        await conn.QueryAsync<User, dynamic, User>(
            sql,
            (user, roleRow) =>
            {
                if (!userDict.TryGetValue(user.Id, out var existingUser))
                {
                    existingUser = user;
                    existingUser.Roles = new List<Role>();
                    userDict[user.Id] = existingUser;
                }

                if (roleRow != null)
                {
                    var roleId = (Guid?)roleRow.RoleId;
                    var roleName = (string?)roleRow.RoleName;
                    if (roleId.HasValue && roleName != null)
                    {
                        existingUser.Roles.Add(new Role { Id = roleId.Value, Name = roleName });
                    }
                }

                return existingUser;
            },
            new { Id = id },
            splitOn: "RoleId"
        );

        return userDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return Enumerable.Empty<User>();

        const string sql = @"
            SELECT u.Id, u.Email, u.PasswordHash, u.FirstName, u.LastName, u.IsActive, u.DateCreated, u.DateUpdated,
                   r.Id AS RoleId, r.Name AS RoleName
            FROM dbo.Users u
            LEFT JOIN dbo.UserRoles ur ON ur.UserId = u.Id
            LEFT JOIN dbo.Roles r ON r.Id = ur.RoleId
            WHERE u.Id IN @Ids AND u.IsActive = 1";

        using var conn = CreateConnection();
        var userDict = new Dictionary<Guid, User>();

        await conn.QueryAsync<User, dynamic, User>(
            sql,
            (user, roleRow) =>
            {
                if (!userDict.TryGetValue(user.Id, out var existingUser))
                {
                    existingUser = user;
                    existingUser.Roles = new List<Role>();
                    userDict[user.Id] = existingUser;
                }

                if (roleRow != null)
                {
                    var roleId = (Guid?)roleRow.RoleId;
                    var roleName = (string?)roleRow.RoleName;
                    if (roleId.HasValue && roleName != null)
                    {
                        existingUser.Roles.Add(new Role { Id = roleId.Value, Name = roleName });
                    }
                }

                return existingUser;
            },
            new { Ids = idList },
            splitOn: "RoleId"
        );

        return userDict.Values;
    }
}
