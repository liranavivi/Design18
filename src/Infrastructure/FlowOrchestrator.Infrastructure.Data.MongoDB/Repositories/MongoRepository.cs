using System.Linq.Expressions;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// Base implementation of the repository pattern for MongoDB.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public class MongoRepository<T> : IRepository<T> where T : class
    {
        protected readonly IMongoCollection<T> _collection;
        protected readonly ILogger<MongoRepository<T>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="logger">The logger.</param>
        public MongoRepository(MongoDbContext context, string collectionName, ILogger<MongoRepository<T>> logger)
        {
            _collection = context.GetCollection<T>(collectionName);
            _logger = logger;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all entities from collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entities with filter in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity with ID {Id} from collection {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<T?> FindOneAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding one entity with filter in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity to collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> UpdateAsync(string id, T entity)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                var result = await _collection.ReplaceOneAsync(filter, entity);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity with ID {Id} in collection {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> RemoveAsync(string id)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                var result = await _collection.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing entity with ID {Id} from collection {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(string id)
        {
            // DeleteAsync is an alias for RemoveAsync for backward compatibility
            return await RemoveAsync(id);
        }

        /// <inheritdoc />
        public virtual async Task<long> RemoveManyAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                var result = await _collection.DeleteManyAsync(filter);
                return result.DeletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing entities with filter from collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _collection.Find(filter).AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if entity exists in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<long> CountAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _collection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }
    }
}
