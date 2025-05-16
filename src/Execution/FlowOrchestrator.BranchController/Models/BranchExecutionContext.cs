using FlowOrchestrator.Abstractions.Common;
using System.Collections.Concurrent;

namespace FlowOrchestrator.BranchController.Models
{
    /// <summary>
    /// Represents the status of a branch execution.
    /// </summary>
    public enum BranchStatus
    {
        /// <summary>
        /// The branch is new and has not started execution.
        /// </summary>
        NEW,

        /// <summary>
        /// The branch is currently executing.
        /// </summary>
        IN_PROGRESS,

        /// <summary>
        /// The branch has completed execution successfully.
        /// </summary>
        COMPLETED,

        /// <summary>
        /// The branch has failed execution.
        /// </summary>
        FAILED,

        /// <summary>
        /// The branch has been cancelled.
        /// </summary>
        CANCELLED
    }

    /// <summary>
    /// Represents the context for a branch execution.
    /// </summary>
    public class BranchExecutionContext
    {
        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch identifier.
        /// </summary>
        public string BranchId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent branch identifier.
        /// </summary>
        public string? ParentBranchId { get; set; }

        /// <summary>
        /// Gets or sets the branch status.
        /// </summary>
        public BranchStatus Status { get; set; } = BranchStatus.NEW;

        /// <summary>
        /// Gets or sets the timestamp when the branch started.
        /// </summary>
        public DateTime StartTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the branch ended.
        /// </summary>
        public DateTime? EndTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the completed steps in the branch.
        /// </summary>
        public List<string> CompletedSteps { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the pending steps in the branch.
        /// </summary>
        public List<string> PendingSteps { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the branch dependencies.
        /// </summary>
        public List<string> Dependencies { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the merge points.
        /// </summary>
        public List<string> MergePoints { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the branch configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the branch parameters.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the branch metadata.
        /// </summary>
        public ConcurrentDictionary<string, object> Metadata { get; set; } = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Gets or sets the branch error information.
        /// </summary>
        public ExecutionError? Error { get; set; }

        /// <summary>
        /// Gets or sets the branch priority.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets the branch timeout in milliseconds.
        /// </summary>
        public int TimeoutMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the branch statistics.
        /// </summary>
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the memory addresses associated with this branch.
        /// </summary>
        public List<string> MemoryAddresses { get; set; } = new List<string>();

        /// <summary>
        /// Creates a new branch execution context from this context.
        /// </summary>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>A new branch execution context.</returns>
        public BranchExecutionContext CreateChildBranch(string branchId)
        {
            return new BranchExecutionContext
            {
                ExecutionId = ExecutionId,
                BranchId = branchId,
                ParentBranchId = BranchId,
                Status = BranchStatus.NEW,
                StartTimestamp = DateTime.UtcNow,
                Parameters = new Dictionary<string, object>(Parameters),
                Configuration = new Dictionary<string, object>(Configuration),
                Priority = Priority
            };
        }
    }
}
