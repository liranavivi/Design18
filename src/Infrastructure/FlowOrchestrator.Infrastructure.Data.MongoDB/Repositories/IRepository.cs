using System.Linq.Expressions;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// Defines the generic repository interface for data access operations.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public interface IRepository<T> where T : class
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
        /// <param name="entity">The updated entity.</param>
        /// <returns>true if the entity was updated; otherwise, false.</returns>
        Task<bool> UpdateAsync(string id, T entity);

        /// <summary>
        /// Removes an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>true if the entity was removed; otherwise, false.</returns>
        Task<bool> RemoveAsync(string id);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>true if the entity was deleted; otherwise, false.</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Removes entities that match the specified filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <returns>The number of entities removed.</returns>
        Task<long> RemoveManyAsync(Expression<Func<T, bool>> filter);

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
