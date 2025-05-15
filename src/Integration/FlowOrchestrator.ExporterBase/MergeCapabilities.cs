using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Integration.Exporters
{
    /// <summary>
    /// Represents the merge capabilities of an exporter service.
    /// </summary>
    public class MergeCapabilities
    {
        /// <summary>
        /// Gets or sets the supported merge strategies.
        /// </summary>
        public List<MergeStrategy> SupportedMergeStrategies { get; set; } = new List<MergeStrategy>();

        /// <summary>
        /// Gets or sets a value indicating whether the exporter supports partial merges.
        /// </summary>
        public bool SupportsPartialMerge { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exporter supports conflict resolution.
        /// </summary>
        public bool SupportsConflictResolution { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of branches that can be merged.
        /// </summary>
        public int? MaxBranchCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum data size that can be merged (in bytes).
        /// </summary>
        public long? MaxMergeDataSize { get; set; }

        /// <summary>
        /// Gets or sets additional capabilities.
        /// </summary>
        public Dictionary<string, object> AdditionalCapabilities { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeCapabilities"/> class.
        /// </summary>
        public MergeCapabilities()
        {
            SupportedMergeStrategies.Add(MergeStrategy.CONCATENATE);
        }

        /// <summary>
        /// Determines whether the specified merge strategy is supported.
        /// </summary>
        /// <param name="strategy">The merge strategy.</param>
        /// <returns>True if the merge strategy is supported, otherwise false.</returns>
        public bool SupportsStrategy(MergeStrategy strategy)
        {
            return SupportedMergeStrategies.Contains(strategy);
        }

        /// <summary>
        /// Determines whether the specified branch count is supported.
        /// </summary>
        /// <param name="branchCount">The branch count.</param>
        /// <returns>True if the branch count is supported, otherwise false.</returns>
        public bool SupportsBranchCount(int branchCount)
        {
            return !MaxBranchCount.HasValue || branchCount <= MaxBranchCount.Value;
        }

        /// <summary>
        /// Determines whether the specified data size is supported.
        /// </summary>
        /// <param name="dataSize">The data size in bytes.</param>
        /// <returns>True if the data size is supported, otherwise false.</returns>
        public bool SupportsDataSize(long dataSize)
        {
            return !MaxMergeDataSize.HasValue || dataSize <= MaxMergeDataSize.Value;
        }
    }
}
