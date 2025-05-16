using FlowOrchestrator.Infrastructure.Data.Hazelcast.Cache;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FlowOrchestrator.MemoryManager.Cache
{
    /// <summary>
    /// A mock implementation of the IDistributedCache interface for testing purposes.
    /// </summary>
    public class HazelcastDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly Dictionary<string, DateTime> _expirations = new Dictionary<string, DateTime>();
        private readonly ILogger<HazelcastDistributedCache> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastDistributedCache"/> class.
        /// </summary>
        public HazelcastDistributedCache()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<HazelcastDistributedCache>();
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key)
        {
            CleanupExpiredItems();
            return Task.FromResult(_cache.ContainsKey(key));
        }

        /// <inheritdoc/>
        public Task<T?> GetAsync<T>(string key)
        {
            CleanupExpiredItems();

            if (_cache.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return Task.FromResult<T?>(typedValue);
                }
                else
                {
                    try
                    {
                        // Try to convert the value to the requested type
                        var json = JsonSerializer.Serialize(value);
                        var result = JsonSerializer.Deserialize<T>(json);
                        return Task.FromResult(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error converting cached value to type {Type}", typeof(T).Name);
                        return Task.FromResult<T?>(default);
                    }
                }
            }

            return Task.FromResult<T?>(default);
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key)
        {
            CleanupExpiredItems();

            var removed = _cache.Remove(key);
            _expirations.Remove(key);
            return Task.FromResult(removed);
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, int? timeToLiveSeconds = null)
        {
            try
            {
                _cache[key] = value!;

                if (timeToLiveSeconds.HasValue)
                {
                    _expirations[key] = DateTime.UtcNow.AddSeconds(timeToLiveSeconds.Value);
                }
                else if (_expirations.ContainsKey(key))
                {
                    _expirations.Remove(key);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key {Key}", key);
                return Task.FromException(ex);
            }
        }

        /// <inheritdoc/>
        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int? timeToLiveSeconds = null)
        {
            CleanupExpiredItems();

            if (_cache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return Task.FromResult(typedValue);
            }

            return GetOrSetAsyncInternal(key, factory, timeToLiveSeconds);
        }

        private async Task<T> GetOrSetAsyncInternal<T>(string key, Func<Task<T>> factory, int? timeToLiveSeconds)
        {
            var value = await factory();
            await SetAsync(key, value, timeToLiveSeconds);
            return value;
        }

        /// <inheritdoc/>
        public Task ClearAsync()
        {
            _cache.Clear();
            _expirations.Clear();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> GetAllKeysAsync()
        {
            CleanupExpiredItems();
            return Task.FromResult<IEnumerable<string>>(_cache.Keys.ToList());
        }

        /// <inheritdoc/>
        public Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys)
        {
            CleanupExpiredItems();

            var result = new Dictionary<string, T?>();
            foreach (var key in keys)
            {
                if (_cache.TryGetValue(key, out var value))
                {
                    if (value is T typedValue)
                    {
                        result[key] = typedValue;
                    }
                    else
                    {
                        try
                        {
                            var json = JsonSerializer.Serialize(value);
                            result[key] = JsonSerializer.Deserialize<T>(json);
                        }
                        catch
                        {
                            result[key] = default;
                        }
                    }
                }
                else
                {
                    result[key] = default;
                }
            }

            return Task.FromResult<IDictionary<string, T?>>(result);
        }

        /// <inheritdoc/>
        public Task SetManyAsync<T>(IDictionary<string, T> values, int? timeToLiveSeconds = null)
        {
            foreach (var kvp in values)
            {
                _cache[kvp.Key] = kvp.Value!;

                if (timeToLiveSeconds.HasValue)
                {
                    _expirations[kvp.Key] = DateTime.UtcNow.AddSeconds(timeToLiveSeconds.Value);
                }
                else if (_expirations.ContainsKey(kvp.Key))
                {
                    _expirations.Remove(kvp.Key);
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task RemoveManyAsync(IEnumerable<string> keys)
        {
            CleanupExpiredItems();

            foreach (var key in keys)
            {
                _cache.Remove(key);
                _expirations.Remove(key);
            }

            return Task.CompletedTask;
        }

        private void CleanupExpiredItems()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _expirations.Where(kv => kv.Value <= now).Select(kv => kv.Key).ToList();

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
                _expirations.Remove(key);
            }
        }
    }
}
