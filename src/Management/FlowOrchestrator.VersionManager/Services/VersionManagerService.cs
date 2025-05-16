using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Management.Versioning.Models;
using FlowOrchestrator.Management.Versioning.Repositories;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Management.Versioning.Services
{
    /// <summary>
    /// Implementation of the version manager service.
    /// </summary>
    public class VersionManagerService : IVersionManager
    {
        private readonly IVersionRepository _repository;
        private readonly ILogger<VersionManagerService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionManagerService"/> class.
        /// </summary>
        /// <param name="repository">The version repository.</param>
        /// <param name="logger">The logger.</param>
        public VersionManagerService(IVersionRepository repository, ILogger<VersionManagerService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<VersionInfo?> GetVersionInfoAsync(ComponentType componentType, string componentId, string version)
        {
            try
            {
                _logger.LogDebug("Getting version info for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);
                return await _repository.GetVersionInfoAsync(componentType, componentId, version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version info for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsVersionCompatibleAsync(
            ComponentType sourceType, string sourceId, string sourceVersion,
            ComponentType targetType, string targetId, string targetVersion)
        {
            try
            {
                _logger.LogDebug("Checking compatibility between {SourceType} {SourceId} {SourceVersion} and {TargetType} {TargetId} {TargetVersion}",
                    sourceType, sourceId, sourceVersion, targetType, targetId, targetVersion);

                // Get the compatibility matrix for the source component
                var sourceMatrix = await _repository.GetCompatibilityMatrixAsync(sourceType, sourceId, sourceVersion);
                if (sourceMatrix == null)
                {
                    _logger.LogWarning("Compatibility matrix not found for {SourceType} {SourceId} {SourceVersion}",
                        sourceType, sourceId, sourceVersion);
                    return false;
                }

                // Check if the target component is compatible with the source component
                var isCompatible = sourceMatrix.IsCompatibleWith(targetType, targetVersion);
                
                // If not compatible, also check the reverse direction
                if (!isCompatible)
                {
                    var targetMatrix = await _repository.GetCompatibilityMatrixAsync(targetType, targetId, targetVersion);
                    if (targetMatrix != null)
                    {
                        isCompatible = targetMatrix.IsCompatibleWith(sourceType, sourceVersion);
                    }
                }

                return isCompatible;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking compatibility between {SourceType} {SourceId} {SourceVersion} and {TargetType} {TargetId} {TargetVersion}",
                    sourceType, sourceId, sourceVersion, targetType, targetId, targetVersion);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<CompatibilityMatrix?> GetCompatibilityMatrixAsync(ComponentType componentType, string componentId, string version)
        {
            try
            {
                _logger.LogDebug("Getting compatibility matrix for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);
                return await _repository.GetCompatibilityMatrixAsync(componentType, componentId, version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compatibility matrix for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VersionInfo>> GetVersionHistoryAsync(ComponentType componentType, string componentId)
        {
            try
            {
                _logger.LogDebug("Getting version history for {ComponentType} {ComponentId}", componentType, componentId);
                return await _repository.GetVersionHistoryAsync(componentType, componentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for {ComponentType} {ComponentId}", componentType, componentId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateVersionStatusAsync(ComponentType componentType, string componentId, string version, VersionStatus status)
        {
            try
            {
                _logger.LogDebug("Updating version status for {ComponentType} {ComponentId} {Version} to {Status}", 
                    componentType, componentId, version, status);
                
                // Get the current version info
                var versionInfo = await _repository.GetVersionInfoAsync(componentType, componentId, version);
                if (versionInfo == null)
                {
                    _logger.LogWarning("Version info not found for {ComponentType} {ComponentId} {Version}", 
                        componentType, componentId, version);
                    return false;
                }

                // Validate the status transition
                if (!IsValidStatusTransition(versionInfo.VersionStatus, status))
                {
                    _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for {ComponentType} {ComponentId} {Version}",
                        versionInfo.VersionStatus, status, componentType, componentId, version);
                    return false;
                }

                // Update the status
                return await _repository.UpdateVersionStatusAsync(componentType, componentId, version, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating version status for {ComponentType} {ComponentId} {Version} to {Status}",
                    componentType, componentId, version, status);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RegistrationResult> RegisterVersionAsync(
            ComponentType componentType, string componentId, string version,
            VersionInfo versionInfo, CompatibilityMatrix compatibilityMatrix)
        {
            try
            {
                _logger.LogDebug("Registering version for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);

                // Validate input
                if (string.IsNullOrWhiteSpace(componentId))
                {
                    return RegistrationResult.CreateFailure(componentId, version, "Component ID cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(version))
                {
                    return RegistrationResult.CreateFailure(componentId, version, "Version cannot be empty");
                }

                if (!IsValidVersion(version))
                {
                    return RegistrationResult.CreateFailure(componentId, version, "Invalid version format. Expected format: MAJOR.MINOR.PATCH");
                }

                // Check if the version already exists
                var existingVersion = await _repository.GetVersionInfoAsync(componentType, componentId, version);
                if (existingVersion != null)
                {
                    return RegistrationResult.CreateFailure(componentId, version, "Version already exists");
                }

                // Ensure the version info and compatibility matrix have the correct component type, ID, and version
                versionInfo.ComponentType = componentType;
                versionInfo.ComponentId = componentId;
                versionInfo.Version = version;
                versionInfo.CreatedTimestamp = DateTime.UtcNow;
                versionInfo.LastModifiedTimestamp = DateTime.UtcNow;

                compatibilityMatrix.ComponentType = componentType;
                compatibilityMatrix.ComponentId = componentId;
                compatibilityMatrix.Version = version;

                // Save the version info and compatibility matrix
                var versionInfoSaved = await _repository.AddOrUpdateVersionInfoAsync(versionInfo);
                var matrixSaved = await _repository.AddOrUpdateCompatibilityMatrixAsync(compatibilityMatrix);

                if (versionInfoSaved && matrixSaved)
                {
                    return RegistrationResult.CreateSuccess(componentId, version);
                }
                else
                {
                    return RegistrationResult.CreateFailure(componentId, version, "Failed to save version information");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering version for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);
                return RegistrationResult.CreateFailure(componentId, version, $"Error: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteVersionAsync(ComponentType componentType, string componentId, string version)
        {
            try
            {
                _logger.LogDebug("Deleting version for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);
                
                // Get the current version info
                var versionInfo = await _repository.GetVersionInfoAsync(componentType, componentId, version);
                if (versionInfo == null)
                {
                    _logger.LogWarning("Version info not found for {ComponentType} {ComponentId} {Version}", 
                        componentType, componentId, version);
                    return false;
                }

                // Only allow deletion of DRAFT versions
                if (versionInfo.VersionStatus != VersionStatus.DRAFT)
                {
                    _logger.LogWarning("Cannot delete version with status {Status} for {ComponentType} {ComponentId} {Version}",
                        versionInfo.VersionStatus, componentType, componentId, version);
                    return false;
                }

                // Delete the version
                return await _repository.DeleteVersionAsync(componentType, componentId, version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting version for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetComponentsAsync(ComponentType componentType)
        {
            try
            {
                _logger.LogDebug("Getting components for {ComponentType}", componentType);
                return await _repository.GetComponentsAsync(componentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting components for {ComponentType}", componentType);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ComponentType>> GetComponentTypesAsync()
        {
            try
            {
                _logger.LogDebug("Getting component types");
                return await _repository.GetComponentTypesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting component types");
                throw;
            }
        }

        private static bool IsValidStatusTransition(VersionStatus currentStatus, VersionStatus newStatus)
        {
            // Define valid status transitions
            return (currentStatus, newStatus) switch
            {
                (VersionStatus.DRAFT, VersionStatus.ACTIVE) => true,
                (VersionStatus.ACTIVE, VersionStatus.DEPRECATED) => true,
                (VersionStatus.DEPRECATED, VersionStatus.ARCHIVED) => true,
                (VersionStatus.ACTIVE, VersionStatus.DISABLED) => true,
                (VersionStatus.DEPRECATED, VersionStatus.DISABLED) => true,
                (VersionStatus.DISABLED, VersionStatus.ACTIVE) => true,
                (VersionStatus.DISABLED, VersionStatus.DEPRECATED) => true,
                _ => false
            };
        }

        private static bool IsValidVersion(string version)
        {
            // Check if the version follows the MAJOR.MINOR.PATCH format
            var parts = version.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            return parts.All(part => int.TryParse(part, out _));
        }
    }
}
