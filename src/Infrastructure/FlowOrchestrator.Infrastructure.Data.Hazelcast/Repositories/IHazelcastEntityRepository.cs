using FlowOrchestrator.Abstractions.Entities;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Repositories
{
    /// <summary>
    /// Defines the repository interface for entity operations in Hazelcast.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public interface IHazelcastEntityRepository<T> : IHazelcastRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Gets all versions of an entity.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>A collection of all versions of the entity.</returns>
        Task<IEnumerable<T>> GetVersionHistoryAsync(string entityId);

        /// <summary>
        /// Gets a specific version of an entity.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="version">The entity version.</param>
        /// <returns>The entity with the specified version, or null if not found.</returns>
        Task<T?> GetVersionAsync(string entityId, string version);

        /// <summary>
        /// Gets the latest version of an entity.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>The latest version of the entity, or null if not found.</returns>
        Task<T?> GetLatestVersionAsync(string entityId);

        /// <summary>
        /// Creates a new version of an entity.
        /// </summary>
        /// <param name="entity">The entity to create a new version for.</param>
        /// <param name="versionDescription">The version description.</param>
        /// <returns>The new entity version.</returns>
        Task<T> CreateNewVersionAsync(T entity, string versionDescription);
    }
}
