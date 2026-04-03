namespace TaskBoard.Infrastructure.Cache;

public interface IQueueService
{
    Task EnqueueAsync(string queueName, object message);
    Task<T?> DequeueAsync<T>(string queueName, TimeSpan timeout);
}
