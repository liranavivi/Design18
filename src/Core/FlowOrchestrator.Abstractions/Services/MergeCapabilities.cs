namespace FlowOrchestrator.Abstractions.Services
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
        /// Gets or sets a value indicating whether the exporter supports custom merge strategies.
        /// </summary>
        public bool SupportsCustomMergeStrategies { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of branches that can be merged.
        /// </summary>
        public int? MaxBranchCount { get; set; }

        /// <summary>
        /// Gets or sets additional capabilities specific to the exporter.
        /// </summary>
        public Dictionary<string, object> AdditionalCapabilities { get; set; } = new Dictionary<string, object>();
    }
}
