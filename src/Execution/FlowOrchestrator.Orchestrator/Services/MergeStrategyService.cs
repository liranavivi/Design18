using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Orchestrator.Services
{
    /// <summary>
    /// Service for managing merge strategies.
    /// </summary>
    public class MergeStrategyService
    {
        private readonly ILogger<MergeStrategyService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeStrategyService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MergeStrategyService(ILogger<MergeStrategyService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Determines if a merge operation should be triggered.
        /// </summary>
        /// <param name="branchContexts">The branch execution contexts.</param>
        /// <param name="mergePolicy">The merge policy.</param>
        /// <returns>True if a merge should be triggered, false otherwise.</returns>
        public bool ShouldTriggerMerge(IEnumerable<BranchExecutionContext> branchContexts, MergePolicy mergePolicy)
        {
            _logger.LogInformation("Checking if merge should be triggered with policy {MergePolicy}", mergePolicy);

            switch (mergePolicy)
            {
                case MergePolicy.ALL_BRANCHES_COMPLETE:
                    return branchContexts.All(b => b.Status == BranchStatus.COMPLETED);

                case MergePolicy.ANY_BRANCH_COMPLETE:
                    return branchContexts.Any(b => b.Status == BranchStatus.COMPLETED);

                case MergePolicy.CRITICAL_BRANCHES_COMPLETE:
                    // Assuming branches with Priority > 0 are critical
                    var criticalBranches = branchContexts.Where(b => b.Priority > 0);
                    return criticalBranches.Any() && criticalBranches.All(b => b.Status == BranchStatus.COMPLETED);

                case MergePolicy.TIMEOUT_REACHED:
                    // This would typically be triggered by a timeout mechanism, not this check
                    return false;

                default:
                    _logger.LogWarning("Unknown merge policy: {MergePolicy}", mergePolicy);
                    return false;
            }
        }

        /// <summary>
        /// Determines the appropriate merge strategy based on exporter capabilities and branch data.
        /// </summary>
        /// <param name="exporterCapabilities">The exporter's merge capabilities.</param>
        /// <param name="branchContexts">The branch execution contexts.</param>
        /// <returns>The selected merge strategy.</returns>
        public MergeStrategy DetermineMergeStrategy(MergeCapabilities exporterCapabilities, IEnumerable<BranchExecutionContext> branchContexts)
        {
            _logger.LogInformation("Determining merge strategy based on exporter capabilities");

            // Default to the first supported strategy
            if (exporterCapabilities.SupportedStrategies.Contains(MergeStrategy.Append))
            {
                return MergeStrategy.Append;
            }
            else if (exporterCapabilities.SupportedStrategies.Contains(MergeStrategy.Replace))
            {
                return MergeStrategy.Replace;
            }
            else if (exporterCapabilities.SupportedStrategies.Contains(MergeStrategy.Merge))
            {
                return MergeStrategy.Merge;
            }
            else if (exporterCapabilities.SupportedStrategies.Contains(MergeStrategy.Custom))
            {
                return MergeStrategy.Custom;
            }
            else
            {
                _logger.LogWarning("No supported merge strategies found, defaulting to Replace");
                return MergeStrategy.Replace;
            }
        }
    }

    /// <summary>
    /// Defines the merge policies for determining when to trigger a merge operation.
    /// </summary>
    public enum MergePolicy
    {
        /// <summary>
        /// Trigger merge when all branches complete.
        /// </summary>
        ALL_BRANCHES_COMPLETE,

        /// <summary>
        /// Trigger merge when any branch completes.
        /// </summary>
        ANY_BRANCH_COMPLETE,

        /// <summary>
        /// Trigger merge when critical branches complete.
        /// </summary>
        CRITICAL_BRANCHES_COMPLETE,

        /// <summary>
        /// Trigger merge when a timeout is reached.
        /// </summary>
        TIMEOUT_REACHED
    }
}
