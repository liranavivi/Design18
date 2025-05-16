using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.BranchController.Messaging.Commands
{
    /// <summary>
    /// Command to create a new branch.
    /// </summary>
    public class CreateBranchCommand
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
        /// Gets or sets the branch parameters.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the branch configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the branch priority.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets the branch timeout in milliseconds.
        /// </summary>
        public int TimeoutMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? Context { get; set; }
    }

    /// <summary>
    /// Command to update the status of a branch.
    /// </summary>
    public class UpdateBranchStatusCommand
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
        /// Gets or sets the branch status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? Context { get; set; }
    }

    /// <summary>
    /// Command to add a completed step to a branch.
    /// </summary>
    public class AddCompletedStepCommand
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
        /// Gets or sets the step identifier.
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? Context { get; set; }
    }

    /// <summary>
    /// Command to add a pending step to a branch.
    /// </summary>
    public class AddPendingStepCommand
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
        /// Gets or sets the step identifier.
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? Context { get; set; }
    }
}
