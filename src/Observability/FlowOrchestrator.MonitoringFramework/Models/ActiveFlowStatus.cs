namespace FlowOrchestrator.Observability.Monitoring.Models
{
    /// <summary>
    /// Status information for an active flow.
    /// </summary>
    public class ActiveFlowStatus
    {
        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow name.
        /// </summary>
        public string FlowName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow version.
        /// </summary>
        public string FlowVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution status.
        /// </summary>
        public string ExecutionStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start timestamp.
        /// </summary>
        public DateTime StartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the current step.
        /// </summary>
        public string CurrentStep { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current step start timestamp.
        /// </summary>
        public DateTime? CurrentStepStartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the execution duration.
        /// </summary>
        public TimeSpan ExecutionDuration { get; set; }

        /// <summary>
        /// Gets or sets the active branches.
        /// </summary>
        public List<ActiveBranchStatus> ActiveBranches { get; set; } = new List<ActiveBranchStatus>();

        /// <summary>
        /// Gets or sets the completed steps.
        /// </summary>
        public List<CompletedStepInfo> CompletedSteps { get; set; } = new List<CompletedStepInfo>();

        /// <summary>
        /// Gets or sets the pending steps.
        /// </summary>
        public List<string> PendingSteps { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        public ErrorInfo? Error { get; set; }
    }

    /// <summary>
    /// Status information for an active branch.
    /// </summary>
    public class ActiveBranchStatus
    {
        /// <summary>
        /// Gets or sets the branch identifier.
        /// </summary>
        public string BranchId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch name.
        /// </summary>
        public string BranchName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch status.
        /// </summary>
        public string BranchStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current step.
        /// </summary>
        public string CurrentStep { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start timestamp.
        /// </summary>
        public DateTime StartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the execution duration.
        /// </summary>
        public TimeSpan ExecutionDuration { get; set; }
    }

    /// <summary>
    /// Information about a completed step.
    /// </summary>
    public class CompletedStepInfo
    {
        /// <summary>
        /// Gets or sets the step name.
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start timestamp.
        /// </summary>
        public DateTime StartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the end timestamp.
        /// </summary>
        public DateTime EndTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the step status.
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Error information.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error type.
        /// </summary>
        public string ErrorType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the step where the error occurred.
        /// </summary>
        public string Step { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the recovery attempts.
        /// </summary>
        public int RecoveryAttempts { get; set; }
    }
}
