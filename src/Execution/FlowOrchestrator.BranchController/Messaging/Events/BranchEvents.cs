using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.BranchController.Messaging.Events
{
    /// <summary>
    /// Event indicating that a branch has been created.
    /// </summary>
    public class BranchCreatedEvent
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
        /// Gets or sets the timestamp when the branch was created.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the branch priority.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets the branch timeout in milliseconds.
        /// </summary>
        public int TimeoutMs { get; set; } = 0;
    }

    /// <summary>
    /// Event indicating that a branch status has changed.
    /// </summary>
    public class BranchStatusChangedEvent
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
        /// Gets or sets the old branch status.
        /// </summary>
        public string OldStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the new branch status.
        /// </summary>
        public string NewStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the status changed.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event indicating that a branch has completed.
    /// </summary>
    public class BranchCompletedEvent
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
        /// Gets or sets the timestamp when the branch completed.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the branch statistics.
        /// </summary>
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the final status of the branch.
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event indicating that a branch has failed.
    /// </summary>
    public class BranchFailedEvent
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
        /// Gets or sets the timestamp when the branch failed.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        public ExecutionError Error { get; set; } = new ExecutionError();
    }
}
