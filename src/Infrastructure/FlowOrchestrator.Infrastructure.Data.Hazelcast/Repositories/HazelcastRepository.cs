using System.Linq.Expressions;
using System.Text.Json;
using FlowOrchestrator.Infrastructure.Data.Hazelcast.Extensions;
using Hazelcast.DistributedObjects;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Repositories
{
    /// <summary>
    /// Base implementation of the repository pattern for Hazelcast.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public class HazelcastRepository<T> : IHazelcastRepository<T> where T : class
    {
        private readonly HazelcastContext _context;
        private readonly ILogger<HazelcastRepository<T>> _logger;
        private readonly string _mapName;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastRepository{T}"/> class.
        /// </summary>
        /// <param name="context">The Hazelcast context.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="mapName">The map name.</param>
        public HazelcastRepository(HazelcastContext context, ILogger<HazelcastRepository<T>> logger, string mapName)
        {
            _context = context;
            _logger = logger;
            _mapName = mapName;
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
        protected async Task<IHMap<string, string>> GetMapAsync()
        {
            var client = await _context.GetClientAsync();
            return await client.GetMapAsync<string, string>(_mapName);
        }

        /// <summary>
        /// Serializes an entity to JSON.
        /// </summary>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns>The serialized entity.</returns>
        protected string SerializeEntity(T entity)
        {
            return JsonSerializer.Serialize(entity, _jsonOptions);
        }

        /// <summary>
        /// Deserializes an entity from JSON.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <returns>The deserialized entity.</returns>
        protected T? DeserializeEntity(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier.</returns>
        protected virtual string GetEntityId(T entity)
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Entity type {typeof(T).Name} does not have an Id property.");

            var id = idProperty.GetValue(entity)?.ToString();
            if (string.IsNullOrEmpty(id))
                throw new InvalidOperationException($"Entity of type {typeof(T).Name} has a null or empty Id.");

            return id;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var map = await GetMapAsync();
            var keys = await map.GetKeysAsync();
            var entries = await map.GetAllAsync(keys.ToCollection());

            return entries.Values
                .Select(DeserializeEntity)
                .Where(e => e != null)
                .Cast<T>()
                .ToList();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        {
            var predicate = filter.Compile();
            var map = await GetMapAsync();
            var keys = await map.GetKeysAsync();
            var entries = await map.GetAllAsync(keys.ToCollection());

            return entries.Values
                .Select(DeserializeEntity)
                .Where(e => e != null)
                .Cast<T>()
                .Where(predicate)
                .ToList();
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(string id)
        {
            var map = await GetMapAsync();
            var json = await map.GetAsync(id);
            return DeserializeEntity(json);
        }

        /// <inheritdoc />
        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> filter)
        {
            var predicate = filter.Compile();
            var map = await GetMapAsync();
            var keys = await map.GetKeysAsync();
            var entries = await map.GetAllAsync(keys.ToCollection());

            return entries.Values
                .Select(DeserializeEntity)
                .Where(e => e != null)
                .Cast<T>()
                .FirstOrDefault(predicate);
        }

        /// <inheritdoc />
        public virtual async Task<T> AddAsync(T entity)
        {
            var id = GetEntityId(entity);
            var map = await GetMapAsync();
            var json = SerializeEntity(entity);

            await map.PutAsync(id, json);
            _logger.LogDebug("Added entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);

            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<T> UpdateAsync(string id, T entity)
        {
            var map = await GetMapAsync();
            var json = SerializeEntity(entity);

            await map.PutAsync(id, json);
            _logger.LogDebug("Updated entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);

            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(string id)
        {
            var map = await GetMapAsync();
            var result = await map.RemoveAsync(id);

            var success = result != null;
            if (success)
            {
                _logger.LogDebug("Deleted entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
            }

            return success;
        }

        /// <inheritdoc />
        public async Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter)
        {
            var predicate = filter.Compile();
            var map = await GetMapAsync();
            var keys = await map.GetKeysAsync();
            var entries = await map.GetAllAsync(keys.ToCollection());

            var entitiesToDelete = entries
                .Select(kv => new { Key = kv.Key, Value = DeserializeEntity(kv.Value) })
                .Where(e => e.Value != null)
                .Where(e => predicate(e.Value!))
                .ToList();

            foreach (var entity in entitiesToDelete)
            {
                await map.RemoveAsync(entity.Key);
            }

            var count = entitiesToDelete.Count;
            _logger.LogDebug("Deleted {Count} entities of type {EntityType}", count, typeof(T).Name);

            return count;
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            var predicate = filter.Compile();
            var map = await GetMapAsync();
            var keys = await map.GetKeysAsync();
            var entries = await map.GetAllAsync(keys.ToCollection());

            return entries.Values
                .Select(DeserializeEntity)
                .Where(e => e != null)
                .Cast<T>()
                .Any(predicate);
        }

        /// <inheritdoc />
        public async Task<long> CountAsync(Expression<Func<T, bool>> filter)
        {
            var predicate = filter.Compile();
            var map = await GetMapAsync();
            var keys = await map.GetKeysAsync();
            var entries = await map.GetAllAsync(keys.ToCollection());

            return entries.Values
                .Select(DeserializeEntity)
                .Where(e => e != null)
                .Cast<T>()
                .Count(predicate);
        }
    }
}
