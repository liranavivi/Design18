using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Infrastructure.Data.Hazelcast.Extensions;
using Hazelcast.DistributedObjects;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Repositories
{
    /// <summary>
    /// Base implementation of the entity repository for Hazelcast.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public class HazelcastEntityRepository<T> : HazelcastRepository<T>, IHazelcastEntityRepository<T> where T : class, IEntity
    {
        private readonly string _versionMapName;
        private readonly HazelcastContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastEntityRepository{T}"/> class.
        /// </summary>
        /// <param name="context">The Hazelcast context.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="mapName">The map name.</param>
        public HazelcastEntityRepository(HazelcastContext context, ILogger<HazelcastEntityRepository<T>> logger, string mapName)
            : base(context, logger, mapName)
        {
            _context = context;
            _versionMapName = $"{mapName}_versions";
        }

        /// <summary>
        /// Gets the version map.
        /// </summary>
        /// <returns>The version map.</returns>
        protected async Task<IHMap<string, string>> GetVersionMapAsync()
        {
            var client = await _context.GetClientAsync();
            return await client.GetMapAsync<string, string>(_versionMapName);
        }

        /// <summary>
        /// Gets the version key.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="version">The version.</param>
        /// <returns>The version key.</returns>
        protected string GetVersionKey(string entityId, string version)
        {
            return $"{entityId}:{version}";
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetVersionHistoryAsync(string entityId)
        {
            var versionMap = await GetVersionMapAsync();
            var keys = await versionMap.GetKeysAsync();
            var entries = await versionMap.GetAllAsync(keys.ToCollection());

            return entries.Where(kv => kv.Key.StartsWith($"{entityId}:"))
                .Select(kv => DeserializeEntity(kv.Value))
                .Where(e => e != null)
                .Cast<T>()
                .OrderByDescending(e => e.Version)
                .ToList();
        }

        /// <inheritdoc />
        public async Task<T?> GetVersionAsync(string entityId, string version)
        {
            var versionMap = await GetVersionMapAsync();
            var versionKey = GetVersionKey(entityId, version);
            var json = await versionMap.GetAsync(versionKey);

            return DeserializeEntity(json);
        }

        /// <inheritdoc />
        public async Task<T?> GetLatestVersionAsync(string entityId)
        {
            var versions = await GetVersionHistoryAsync(entityId);
            return versions.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<T> CreateNewVersionAsync(T entity, string versionDescription)
        {
            // Create a new version of the entity
            var newVersion = (T)entity.DeepCopy();

            // Set version properties using reflection
            typeof(T).GetProperty("Version")?.SetValue(newVersion, Guid.NewGuid().ToString());
            typeof(T).GetProperty("VersionDescription")?.SetValue(newVersion, versionDescription);

            // Try to set creation date property (might be CreatedAt, CreatedDate, or CreateDate)
            var dateProperty = typeof(T).GetProperty("CreatedAt") ??
                              typeof(T).GetProperty("CreatedDate") ??
                              typeof(T).GetProperty("CreateDate");
            dateProperty?.SetValue(newVersion, DateTime.UtcNow);

            // Save the new version to both maps
            await base.AddAsync(newVersion);

            var versionMap = await GetVersionMapAsync();

            // Get Id and Version using reflection
            var id = typeof(T).GetProperty("Id")?.GetValue(newVersion)?.ToString() ??
                    throw new InvalidOperationException($"Entity of type {typeof(T).Name} has a null or empty Id.");
            var version = typeof(T).GetProperty("Version")?.GetValue(newVersion)?.ToString() ??
                         throw new InvalidOperationException($"Entity of type {typeof(T).Name} has a null or empty Version.");

            var versionKey = GetVersionKey(id, version);
            var json = SerializeEntity(newVersion);
            await versionMap.PutAsync(versionKey, json);

            return newVersion;
        }

        /// <inheritdoc />
        public override async Task<T> AddAsync(T entity)
        {
            // Ensure the entity has a version
            if (string.IsNullOrEmpty(entity.Version))
            {
                entity.Version = Guid.NewGuid().ToString();
            }

            // Save to both maps
            await base.AddAsync(entity);

            var versionMap = await GetVersionMapAsync();

            // Get Id and Version using reflection
            var id = typeof(T).GetProperty("Id")?.GetValue(entity)?.ToString() ??
                    throw new InvalidOperationException($"Entity of type {typeof(T).Name} has a null or empty Id.");
            var version = typeof(T).GetProperty("Version")?.GetValue(entity)?.ToString() ??
                         throw new InvalidOperationException($"Entity of type {typeof(T).Name} has a null or empty Version.");

            var versionKey = GetVersionKey(id, version);
            var json = SerializeEntity(entity);
            await versionMap.PutAsync(versionKey, json);

            return entity;
        }

        /// <inheritdoc />
        public override async Task<T> UpdateAsync(string id, T entity)
        {
            // Create a new version
            return await CreateNewVersionAsync(entity, "Updated entity");
        }

        /// <inheritdoc />
        public override async Task<bool> DeleteAsync(string id)
        {
            // Delete from the main map
            var result = await base.DeleteAsync(id);

            // Delete all versions
            var versionMap = await GetVersionMapAsync();
            var keys = await versionMap.GetKeysAsync();
            var entries = await versionMap.GetAllAsync(keys.ToCollection());

            foreach (var key in entries.Keys.Where(k => k.StartsWith($"{id}:")))
            {
                await versionMap.RemoveAsync(key);
            }

            return result;
        }
    }
}
