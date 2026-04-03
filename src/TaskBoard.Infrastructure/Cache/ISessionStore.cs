namespace TaskBoard.Infrastructure.Cache;

public record SessionData(Guid UserId, string Email, List<string> Roles, DateTime CreatedAt, DateTime ExpiresAt);

public interface ISessionStore
{
    Task CreateSessionAsync(string sessionId, SessionData data, TimeSpan ttl);
    Task<SessionData?> GetSessionAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);
}
