using System.Collections.Concurrent;

namespace TaskBoard.Infrastructure.Cache;

public class InMemoryRedis : IInMemoryRedis, IDisposable
{
    private record CacheEntry(string Value, DateTime? ExpiresAt);
    private record HashEntry(Dictionary<string, string> Fields, DateTime? ExpiresAt);

    private readonly ConcurrentDictionary<string, CacheEntry> _store = new();
    private readonly ConcurrentDictionary<string, HashEntry> _hashStore = new();
    private readonly ConcurrentDictionary<string, BlockingCollection<string>> _queues = new();
    private readonly ConcurrentDictionary<string, List<Action<string>>> _subscriptions = new();
    private readonly Timer _expiryTimer;
    private readonly object _subscriptionLock = new();
    private bool _disposed;

    public InMemoryRedis()
    {
        _expiryTimer = new Timer(SweepExpiredKeys, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void SweepExpiredKeys(object? state)
    {
        var now = DateTime.UtcNow;

        foreach (var kvp in _store)
        {
            if (kvp.Value.ExpiresAt.HasValue && kvp.Value.ExpiresAt.Value <= now)
            {
                _store.TryRemove(kvp.Key, out _);
            }
        }

        foreach (var kvp in _hashStore)
        {
            if (kvp.Value.ExpiresAt.HasValue && kvp.Value.ExpiresAt.Value <= now)
            {
                _hashStore.TryRemove(kvp.Key, out _);
            }
        }
    }

    public Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        var expiresAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : (DateTime?)null;
        _store[key] = new CacheEntry(value, expiresAt);
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value <= DateTime.UtcNow)
            {
                _store.TryRemove(key, out _);
                return Task.FromResult<string?>(null);
            }
            return Task.FromResult<string?>(entry.Value);
        }
        return Task.FromResult<string?>(null);
    }

    public Task<bool> DeleteAsync(string key)
    {
        var removed = _store.TryRemove(key, out _);
        if (!removed)
        {
            removed = _hashStore.TryRemove(key, out _);
        }
        return Task.FromResult(removed);
    }

    public Task<bool> KeyExistsAsync(string key)
    {
        var existsInStore = _store.TryGetValue(key, out var entry) &&
                            (!entry.ExpiresAt.HasValue || entry.ExpiresAt.Value > DateTime.UtcNow);
        var existsInHash = !existsInStore && _hashStore.TryGetValue(key, out var hashEntry) &&
                           (!hashEntry.ExpiresAt.HasValue || hashEntry.ExpiresAt.Value > DateTime.UtcNow);
        return Task.FromResult(existsInStore || existsInHash);
    }

    public Task QueuePushAsync(string queueName, string message)
    {
        var queue = _queues.GetOrAdd(queueName, _ => new BlockingCollection<string>(new ConcurrentQueue<string>()));
        queue.Add(message);
        return Task.CompletedTask;
    }

    public async Task<string?> QueuePopAsync(string queueName, TimeSpan timeout)
    {
        var queue = _queues.GetOrAdd(queueName, _ => new BlockingCollection<string>(new ConcurrentQueue<string>()));
        var deadline = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < deadline)
        {
            if (queue.TryTake(out var item))
                return item;

            await Task.Delay(50);
        }

        return null;
    }

    public Task HashSetAsync(string key, Dictionary<string, string> fields, TimeSpan? expiry = null)
    {
        var expiresAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : (DateTime?)null;

        _hashStore.AddOrUpdate(key,
            _ => new HashEntry(new Dictionary<string, string>(fields), expiresAt),
            (_, existing) =>
            {
                var merged = new Dictionary<string, string>(existing.Fields);
                foreach (var kvp in fields)
                {
                    merged[kvp.Key] = kvp.Value;
                }
                return new HashEntry(merged, expiresAt ?? existing.ExpiresAt);
            });

        return Task.CompletedTask;
    }

    public Task<Dictionary<string, string>?> HashGetAllAsync(string key)
    {
        if (_hashStore.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value <= DateTime.UtcNow)
            {
                _hashStore.TryRemove(key, out _);
                return Task.FromResult<Dictionary<string, string>?>(null);
            }
            return Task.FromResult<Dictionary<string, string>?>(new Dictionary<string, string>(entry.Fields));
        }
        return Task.FromResult<Dictionary<string, string>?>(null);
    }

    public IDisposable Subscribe(string channel, Action<string> handler)
    {
        lock (_subscriptionLock)
        {
            if (!_subscriptions.TryGetValue(channel, out var handlers))
            {
                handlers = new List<Action<string>>();
                _subscriptions[channel] = handlers;
            }
            handlers.Add(handler);
        }
        return new Subscription(this, channel, handler);
    }

    public Task PublishAsync(string channel, string message)
    {
        List<Action<string>> handlers;
        lock (_subscriptionLock)
        {
            if (!_subscriptions.TryGetValue(channel, out var h))
                return Task.CompletedTask;
            handlers = new List<Action<string>>(h);
        }

        foreach (var handler in handlers)
        {
            try { handler(message); }
            catch { /* ignore handler errors */ }
        }
        return Task.CompletedTask;
    }

    internal void Unsubscribe(string channel, Action<string> handler)
    {
        lock (_subscriptionLock)
        {
            if (_subscriptions.TryGetValue(channel, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _expiryTimer.Dispose();
            foreach (var q in _queues.Values)
            {
                q.CompleteAdding();
                q.Dispose();
            }
        }
    }

    private class Subscription : IDisposable
    {
        private readonly InMemoryRedis _redis;
        private readonly string _channel;
        private readonly Action<string> _handler;
        private bool _disposed;

        public Subscription(InMemoryRedis redis, string channel, Action<string> handler)
        {
            _redis = redis;
            _channel = channel;
            _handler = handler;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _redis.Unsubscribe(_channel, _handler);
            }
        }
    }
}
