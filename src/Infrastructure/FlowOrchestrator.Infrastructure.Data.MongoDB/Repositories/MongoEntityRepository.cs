using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB implementation of the entity repository.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public class MongoEntityRepository<T> : MongoRepository<T>, IEntityRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoEntityRepository{T}"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="logger">The logger.</param>
        public MongoEntityRepository(MongoDbContext context, string collectionName, ILogger<MongoEntityRepository<T>> logger)
            : base(context, collectionName, logger)
        {
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<T>> GetVersionHistoryAsync(string entityId)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", entityId);
                return await _collection.Find(filter).SortByDescending(e => e.CreatedTimestamp).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving version history for entity with ID {EntityId} from collection {CollectionName}",
                    entityId, _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<T?> GetVersionAsync(string entityId, string version)
        {
            try
            {
                var filter = Builders<T>.Filter.And(
                    Builders<T>.Filter.Eq("_id", entityId),
                    Builders<T>.Filter.Eq(e => e.Version, version));

                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving version {Version} for entity with ID {EntityId} from collection {CollectionName}",
                    version, entityId, _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> UpdateVersionStatusAsync(string entityId, string version, VersionStatus status)
        {
            try
            {
                var filter = Builders<T>.Filter.And(
                    Builders<T>.Filter.Eq("_id", entityId),
                    Builders<T>.Filter.Eq(e => e.Version, version));

                var update = Builders<T>.Update
                    .Set(e => e.VersionStatus, status)
                    .Set(e => e.LastModifiedTimestamp, DateTime.UtcNow);

                var result = await _collection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating version status for entity with ID {EntityId} and version {Version} in collection {CollectionName}",
                    entityId, version, _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }
    }
}
