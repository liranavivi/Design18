using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Management.Versioning.Models;

namespace FlowOrchestrator.Management.Versioning.Repositories
{
    /// <summary>
    /// In-memory implementation of the version repository.
    /// </summary>
    public class InMemoryVersionRepository : IVersionRepository
    {
        private readonly Dictionary<string, VersionInfo> _versionInfos = new();
        private readonly Dictionary<string, CompatibilityMatrix> _compatibilityMatrices = new();
        private readonly object _lock = new();

        private static string GetVersionInfoKey(ComponentType componentType, string componentId, string version)
        {
            return $"{componentType}:{componentId}:{version}";
        }

        private static string GetCompatibilityMatrixKey(ComponentType componentType, string componentId, string version)
        {
            return $"{componentType}:{componentId}:{version}";
        }

        /// <inheritdoc />
        public Task<VersionInfo?> GetVersionInfoAsync(ComponentType componentType, string componentId, string version)
        {
            var key = GetVersionInfoKey(componentType, componentId, version);
            lock (_lock)
            {
                return Task.FromResult(_versionInfos.TryGetValue(key, out var versionInfo) ? versionInfo : null);
            }
        }

        /// <inheritdoc />
        public Task<CompatibilityMatrix?> GetCompatibilityMatrixAsync(ComponentType componentType, string componentId, string version)
        {
            var key = GetCompatibilityMatrixKey(componentType, componentId, version);
            lock (_lock)
            {
                return Task.FromResult(_compatibilityMatrices.TryGetValue(key, out var matrix) ? matrix : null);
            }
        }

        /// <inheritdoc />
        public Task<IEnumerable<VersionInfo>> GetVersionHistoryAsync(ComponentType componentType, string componentId)
        {
            lock (_lock)
            {
                var versions = _versionInfos.Values
                    .Where(v => v.ComponentType == componentType && v.ComponentId == componentId)
                    .OrderByDescending(v => v.CreatedTimestamp)
                    .ToList();

                return Task.FromResult<IEnumerable<VersionInfo>>(versions);
            }
        }

        /// <inheritdoc />
        public Task<bool> UpdateVersionStatusAsync(ComponentType componentType, string componentId, string version, VersionStatus status)
        {
            var key = GetVersionInfoKey(componentType, componentId, version);
            lock (_lock)
            {
                if (_versionInfos.TryGetValue(key, out var versionInfo))
                {
                    versionInfo.VersionStatus = status;
                    versionInfo.LastModifiedTimestamp = DateTime.UtcNow;
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc />
        public Task<bool> AddOrUpdateVersionInfoAsync(VersionInfo versionInfo)
        {
            var key = GetVersionInfoKey(versionInfo.ComponentType, versionInfo.ComponentId, versionInfo.Version);
            lock (_lock)
            {
                _versionInfos[key] = versionInfo;
                return Task.FromResult(true);
            }
        }

        /// <inheritdoc />
        public Task<bool> AddOrUpdateCompatibilityMatrixAsync(CompatibilityMatrix compatibilityMatrix)
        {
            var key = GetCompatibilityMatrixKey(compatibilityMatrix.ComponentType, compatibilityMatrix.ComponentId, compatibilityMatrix.Version);
            lock (_lock)
            {
                _compatibilityMatrices[key] = compatibilityMatrix;
                return Task.FromResult(true);
            }
        }

        /// <inheritdoc />
        public Task<bool> DeleteVersionAsync(ComponentType componentType, string componentId, string version)
        {
            var versionInfoKey = GetVersionInfoKey(componentType, componentId, version);
            var matrixKey = GetCompatibilityMatrixKey(componentType, componentId, version);
            lock (_lock)
            {
                var versionRemoved = _versionInfos.Remove(versionInfoKey);
                var matrixRemoved = _compatibilityMatrices.Remove(matrixKey);
                return Task.FromResult(versionRemoved || matrixRemoved);
            }
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetComponentsAsync(ComponentType componentType)
        {
            lock (_lock)
            {
                var components = _versionInfos.Values
                    .Where(v => v.ComponentType == componentType)
                    .Select(v => v.ComponentId)
                    .Distinct()
                    .ToList();

                return Task.FromResult<IEnumerable<string>>(components);
            }
        }

        /// <inheritdoc />
        public Task<IEnumerable<ComponentType>> GetComponentTypesAsync()
        {
            lock (_lock)
            {
                var types = _versionInfos.Values
                    .Select(v => v.ComponentType)
                    .Distinct()
                    .ToList();

                return Task.FromResult<IEnumerable<ComponentType>>(types);
            }
        }
    }
}
