namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Merge strategy for combining data from multiple branches.
    /// </summary>
    public enum MergeStrategy
    {
        /// <summary>
        /// Append data from branches.
        /// </summary>
        Append,

        /// <summary>
        /// Replace data from branches.
        /// </summary>
        Replace,

        /// <summary>
        /// Merge data from branches.
        /// </summary>
        Merge,

        /// <summary>
        /// Custom merge strategy.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Merge capabilities for a service.
    /// </summary>
    public class MergeCapabilities
    {
        /// <summary>
        /// Gets or sets the supported merge strategies.
        /// </summary>
        public List<MergeStrategy> SupportedStrategies { get; set; } = new List<MergeStrategy>();

        /// <summary>
        /// Gets or sets the default merge strategy.
        /// </summary>
        public MergeStrategy DefaultStrategy { get; set; } = MergeStrategy.Append;

        /// <summary>
        /// Gets or sets a value indicating whether the service supports custom merge strategies.
        /// </summary>
        public bool SupportsCustomStrategies { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum number of branches that can be merged.
        /// </summary>
        public int MaxBranches { get; set; } = 10;
    }
}
