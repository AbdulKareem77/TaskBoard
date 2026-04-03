using System.Text.Json;

namespace TaskBoard.Infrastructure.Cache;

public class QueueService : IQueueService
{
    private readonly IInMemoryRedis _redis;

    public QueueService(IInMemoryRedis redis)
    {
        _redis = redis;
    }

    public async Task EnqueueAsync(string queueName, object message)
    {
        var serialized = JsonSerializer.Serialize(message);
        await _redis.QueuePushAsync(queueName, serialized);
    }

    public async Task<T?> DequeueAsync<T>(string queueName, TimeSpan timeout)
    {
        var message = await _redis.QueuePopAsync(queueName, timeout);
        if (message == null)
            return default;

        return JsonSerializer.Deserialize<T>(message);
    }
}
