using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Management.Versioning.Models;

namespace FlowOrchestrator.Management.Versioning.Services
{
    /// <summary>
    /// Defines the interface for the version manager service.
    /// </summary>
    public interface IVersionManager
    {
        /// <summary>
        /// Gets version information for a specific component and version.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <returns>The version information, or null if not found.</returns>
        Task<VersionInfo?> GetVersionInfoAsync(ComponentType componentType, string componentId, string version);

        /// <summary>
        /// Determines whether two component versions are compatible.
        /// </summary>
        /// <param name="sourceType">The source component type.</param>
        /// <param name="sourceId">The source component identifier.</param>
        /// <param name="sourceVersion">The source component version.</param>
        /// <param name="targetType">The target component type.</param>
        /// <param name="targetId">The target component identifier.</param>
        /// <param name="targetVersion">The target component version.</param>
        /// <returns>True if the components are compatible; otherwise, false.</returns>
        Task<bool> IsVersionCompatibleAsync(
            ComponentType sourceType, string sourceId, string sourceVersion,
            ComponentType targetType, string targetId, string targetVersion);

        /// <summary>
        /// Gets the compatibility matrix for a specific component and version.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <returns>The compatibility matrix, or null if not found.</returns>
        Task<CompatibilityMatrix?> GetCompatibilityMatrixAsync(ComponentType componentType, string componentId, string version);

        /// <summary>
        /// Gets the version history for a specific component.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <returns>The version history.</returns>
        Task<IEnumerable<VersionInfo>> GetVersionHistoryAsync(ComponentType componentType, string componentId);

        /// <summary>
        /// Updates the status of a specific component version.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="status">The new status.</param>
        /// <returns>True if the status was updated successfully; otherwise, false.</returns>
        Task<bool> UpdateVersionStatusAsync(ComponentType componentType, string componentId, string version, VersionStatus status);

        /// <summary>
        /// Registers a new version of a component.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <param name="compatibilityMatrix">The compatibility matrix.</param>
        /// <returns>The registration result.</returns>
        Task<RegistrationResult> RegisterVersionAsync(
            ComponentType componentType, string componentId, string version,
            VersionInfo versionInfo, CompatibilityMatrix compatibilityMatrix);

        /// <summary>
        /// Deletes a specific component version.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <returns>True if the version was deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteVersionAsync(ComponentType componentType, string componentId, string version);

        /// <summary>
        /// Gets all registered components of a specific type.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns>The list of component identifiers.</returns>
        Task<IEnumerable<string>> GetComponentsAsync(ComponentType componentType);

        /// <summary>
        /// Gets all registered component types.
        /// </summary>
        /// <returns>The list of component types.</returns>
        Task<IEnumerable<ComponentType>> GetComponentTypesAsync();
    }
}
