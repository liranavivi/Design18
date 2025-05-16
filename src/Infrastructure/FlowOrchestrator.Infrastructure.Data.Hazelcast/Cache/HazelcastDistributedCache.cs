using System.Text.Json;
using Hazelcast.DistributedObjects;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Cache
{
    /// <summary>
    /// Hazelcast implementation of the distributed cache.
    /// </summary>
    public class HazelcastDistributedCache : IDistributedCache
    {
        private readonly HazelcastContext _context;
        private readonly ILogger<HazelcastDistributedCache> _logger;
        private readonly string _cacheName;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastDistributedCache"/> class.
        /// </summary>
        /// <param name="context">The Hazelcast context.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cacheName">The cache name.</param>
        public HazelcastDistributedCache(HazelcastContext context, ILogger<HazelcastDistributedCache> logger, string cacheName = "default-cache")
        {
            _context = context;
            _logger = logger;
            _cacheName = cacheName;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Gets the Hazelcast map.
        /// </summary>
        /// <returns>The Hazelcast map.</returns>
        protected async Task<IHMap<string, string>> GetCacheMapAsync()
        {
            var client = await _context.GetClientAsync();
            return await client.GetMapAsync<string, string>(_cacheName);
        }

        /// <summary>
        /// Serializes a value to JSON.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The serialized value.</returns>
        protected string SerializeValue<T>(T value)
        {
            return JsonSerializer.Serialize(value, _jsonOptions);
        }

        /// <summary>
        /// Deserializes a value from JSON.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="json">The JSON to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        protected T? DeserializeValue<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync<T>(string key)
        {
            var map = await GetCacheMapAsync();
            var json = await map.GetAsync(key);

            if (string.IsNullOrEmpty(json))
                return default;

            return DeserializeValue<T>(json);
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, int? expirationSeconds = null)
        {
            var map = await GetCacheMapAsync();
            var json = SerializeValue(value);

            if (expirationSeconds.HasValue)
            {
                await map.PutAsync(key, json, TimeSpan.FromSeconds(expirationSeconds.Value));
            }
            else
            {
                await map.PutAsync(key, json);
            }

            _logger.LogDebug("Set cache value for key {Key} with expiration {Expiration}", key, expirationSeconds);
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(string key)
        {
            var map = await GetCacheMapAsync();
            var result = await map.RemoveAsync(key);

            var success = result != null;
            if (success)
            {
                _logger.LogDebug("Removed cache value for key {Key}", key);
            }

            return success;
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key)
        {
            var map = await GetCacheMapAsync();
            return await map.ContainsKeyAsync(key);
        }

        /// <inheritdoc />
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int? expirationSeconds = null)
        {
            var value = await GetAsync<T>(key);

            if (value == null || EqualityComparer<T>.Default.Equals(value, default))
            {
                value = await factory();
                await SetAsync(key, value, expirationSeconds);
            }

            return value;
        }

        /// <inheritdoc />
        public async Task ClearAsync()
        {
            var map = await GetCacheMapAsync();
            await map.ClearAsync();
            _logger.LogDebug("Cleared all cache values");
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetAllKeysAsync()
        {
            var map = await GetCacheMapAsync();
            var keys = await map.GetKeysAsync();
            return keys;
        }

        /// <inheritdoc />
        public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys)
        {
            var map = await GetCacheMapAsync();
            var keySet = keys.ToHashSet();
            var entries = await map.GetAllAsync(keySet);

            var result = new Dictionary<string, T?>();
            foreach (var key in keySet)
            {
                if (entries.TryGetValue(key, out var json))
                {
                    result[key] = DeserializeValue<T>(json);
                }
                else
                {
                    result[key] = default;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task SetManyAsync<T>(IDictionary<string, T> keyValues, int? expirationSeconds = null)
        {
            var map = await GetCacheMapAsync();
            var entries = new Dictionary<string, string>();

            foreach (var kv in keyValues)
            {
                entries[kv.Key] = SerializeValue(kv.Value);
            }

            if (expirationSeconds.HasValue)
            {
                foreach (var kv in entries)
                {
                    await map.PutAsync(kv.Key, kv.Value, TimeSpan.FromSeconds(expirationSeconds.Value));
                }
            }
            else
            {
                foreach (var kv in entries)
                {
                    await map.PutAsync(kv.Key, kv.Value);
                }
            }

            _logger.LogDebug("Set {Count} cache values with expiration {Expiration}", keyValues.Count, expirationSeconds);
        }

        /// <inheritdoc />
        public async Task RemoveManyAsync(IEnumerable<string> keys)
        {
            var map = await GetCacheMapAsync();
            foreach (var key in keys)
            {
                await map.RemoveAsync(key);
            }

            _logger.LogDebug("Removed {Count} cache values", keys.Count());
        }
    }
}
