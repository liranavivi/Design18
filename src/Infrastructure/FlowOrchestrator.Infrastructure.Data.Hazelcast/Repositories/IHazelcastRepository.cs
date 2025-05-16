using System.Linq.Expressions;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Repositories
{
    /// <summary>
    /// Defines the generic repository interface for Hazelcast data access operations.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public interface IHazelcastRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Gets entities that match the specified filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <returns>A collection of entities that match the filter.</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Gets a single entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The entity with the specified identifier, or null if not found.</returns>
        Task<T?> GetByIdAsync(string id);

        /// <summary>
        /// Gets a single entity that matches the specified filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <returns>The entity that matches the filter, or null if not found.</returns>
        Task<T?> FindOneAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The updated entity.</returns>
        Task<T> UpdateAsync(string id, T entity);

        /// <summary>
        /// Deletes an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>true if the entity was deleted; otherwise, false.</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Deletes entities that match the specified filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <returns>The number of entities deleted.</returns>
        Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Checks if any entity matches the specified filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <returns>true if any entity matches the filter; otherwise, false.</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Gets the count of entities that match the specified filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <returns>The count of entities that match the filter.</returns>
        Task<long> CountAsync(Expression<Func<T, bool>> filter);
    }
}
