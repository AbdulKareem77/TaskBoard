namespace TaskBoard.Infrastructure.Cache;

public interface IInMemoryRedis
{
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> GetAsync(string key);
    Task<bool> DeleteAsync(string key);
    Task<bool> KeyExistsAsync(string key);
    Task QueuePushAsync(string queueName, string message);
    Task<string?> QueuePopAsync(string queueName, TimeSpan timeout);
    Task HashSetAsync(string key, Dictionary<string, string> fields, TimeSpan? expiry = null);
    Task<Dictionary<string, string>?> HashGetAllAsync(string key);
    IDisposable Subscribe(string channel, Action<string> handler);
    Task PublishAsync(string channel, string message);
}
