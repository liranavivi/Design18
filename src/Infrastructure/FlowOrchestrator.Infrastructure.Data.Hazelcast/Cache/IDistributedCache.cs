namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Cache
{
    /// <summary>
    /// Defines the interface for distributed cache operations.
    /// </summary>
    public interface IDistributedCache
    {
        /// <summary>
        /// Gets a value from the cache.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached value, or default if not found.</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Sets a value in the cache.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="expirationSeconds">The expiration time in seconds, or null for no expiration.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetAsync<T>(string key, T value, int? expirationSeconds = null);

        /// <summary>
        /// Removes a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>true if the value was removed; otherwise, false.</returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// Checks if a key exists in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>true if the key exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Gets or sets a value in the cache.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="factory">The factory function to create the value if not found.</param>
        /// <param name="expirationSeconds">The expiration time in seconds, or null for no expiration.</param>
        /// <returns>The cached value.</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int? expirationSeconds = null);

        /// <summary>
        /// Clears all values from the cache.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearAsync();

        /// <summary>
        /// Gets all keys in the cache.
        /// </summary>
        /// <returns>A collection of all keys in the cache.</returns>
        Task<IEnumerable<string>> GetAllKeysAsync();

        /// <summary>
        /// Gets multiple values from the cache.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="keys">The cache keys.</param>
        /// <returns>A dictionary of keys and values.</returns>
        Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys);

        /// <summary>
        /// Sets multiple values in the cache.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="keyValues">The dictionary of keys and values.</param>
        /// <param name="expirationSeconds">The expiration time in seconds, or null for no expiration.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetManyAsync<T>(IDictionary<string, T> keyValues, int? expirationSeconds = null);

        /// <summary>
        /// Removes multiple values from the cache.
        /// </summary>
        /// <param name="keys">The cache keys.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveManyAsync(IEnumerable<string> keys);
    }
}
