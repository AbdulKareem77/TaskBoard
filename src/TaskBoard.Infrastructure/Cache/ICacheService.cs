namespace TaskBoard.Infrastructure.Cache;

public interface ICacheService
{
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl);
    Task InvalidateAsync(string key);
}
