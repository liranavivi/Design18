namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the context of an execution.
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch identifier.
        /// </summary>
        public string BranchId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the step identifier.
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the execution started.
        /// </summary>
        public DateTime StartTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the execution parameters.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the execution state.
        /// </summary>
        public Dictionary<string, object> State { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents error information for a flow execution.
    /// </summary>
    public class ExecutionError
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        public Dictionary<string, object> ErrorDetails { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the service identifier that reported the error.
        /// </summary>
        public string? ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the component identifier that reported the error.
        /// </summary>
        public string? ComponentId { get; set; }
    }
}
