using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// Defines the repository interface for entity operations.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public interface IEntityRepository<T> : IRepository<T> where T : class, IEntity
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
        /// <param name="version">The version.</param>
        /// <returns>The entity with the specified version, or null if not found.</returns>
        Task<T?> GetVersionAsync(string entityId, string version);

        /// <summary>
        /// Updates the status of a specific version of an entity.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="status">The new status.</param>
        /// <returns>true if the status was updated; otherwise, false.</returns>
        Task<bool> UpdateVersionStatusAsync(string entityId, string version, VersionStatus status);
    }
}
