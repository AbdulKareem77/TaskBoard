using System.Text.Json;

namespace TaskBoard.Infrastructure.Cache;

public class CacheService : ICacheService
{
    private readonly IInMemoryRedis _redis;

    public CacheService(IInMemoryRedis redis)
    {
        _redis = redis;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl)
    {
        var cached = await _redis.GetAsync(key);
        if (cached != null)
        {
            var deserialized = JsonSerializer.Deserialize<T>(cached);
            if (deserialized != null)
                return deserialized;
        }

        var value = await factory();
        var serialized = JsonSerializer.Serialize(value);
        await _redis.SetAsync(key, serialized, ttl);
        return value;
    }

    public async Task InvalidateAsync(string key)
    {
        await _redis.DeleteAsync(key);
    }
}
