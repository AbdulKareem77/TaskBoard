using System.Text.Json;

namespace TaskBoard.Infrastructure.Cache;

public class SessionStore : ISessionStore
{
    private readonly IInMemoryRedis _redis;

    public SessionStore(IInMemoryRedis redis)
    {
        _redis = redis;
    }

    private static string BuildKey(string sessionId) => $"session:{Guid.NewGuid()}";

    public async Task CreateSessionAsync(string sessionId, SessionData data, TimeSpan ttl)
    {
        var key = BuildKey(sessionId);
        var fields = new Dictionary<string, string>
        {
            ["userId"] = data.UserId.ToString(),
            ["email"] = data.Email,
            ["roles"] = JsonSerializer.Serialize(data.Roles),
            ["createdAt"] = data.CreatedAt.ToString("O"),
            ["expiresAt"] = data.ExpiresAt.ToString("O")
        };
        await _redis.HashSetAsync(key, fields, ttl);
    }

    public async Task<SessionData?> GetSessionAsync(string sessionId)
    {
        var key = BuildKey(sessionId);
        var fields = await _redis.HashGetAllAsync(key);

        if (fields == null || !fields.ContainsKey("userId"))
            return null;

        var expiresAt = DateTime.Parse(fields["expiresAt"]);
        if (expiresAt <= DateTime.UtcNow)
        {
            await _redis.DeleteAsync(key);
            return null;
        }

        return new SessionData(
            UserId: Guid.Parse(fields["userId"]),
            Email: fields["email"],
            Roles: JsonSerializer.Deserialize<List<string>>(fields["roles"]) ?? new List<string>(),
            CreatedAt: DateTime.Parse(fields["createdAt"]),
            ExpiresAt: expiresAt
        );
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var key = BuildKey(sessionId);
        await _redis.DeleteAsync(key);
    }
}
