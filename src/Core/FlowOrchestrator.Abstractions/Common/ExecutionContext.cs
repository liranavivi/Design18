using System.Collections.Concurrent;

namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the context for a flow execution.
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// Gets or sets the unique identifier for the execution.
        /// </summary>
        public string ExecutionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the identifier of the flow being executed.
        /// </summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the flow being executed.
        /// </summary>
        public string FlowVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the execution started.
        /// </summary>
        public DateTime StartTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the execution ended.
        /// </summary>
        public DateTime? EndTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the execution status.
        /// </summary>
        public ExecutionStatus Status { get; set; } = ExecutionStatus.RUNNING;

        /// <summary>
        /// Gets or sets the branch identifier for this execution context.
        /// </summary>
        public string? BranchId { get; set; }

        /// <summary>
        /// Gets or sets the parent branch identifier for this execution context.
        /// </summary>
        public string? ParentBranchId { get; set; }

        /// <summary>
        /// Gets or sets the execution parameters.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the execution metadata.
        /// </summary>
        public ConcurrentDictionary<string, object> Metadata { get; set; } = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Gets or sets the execution error information.
        /// </summary>
        public ExecutionError? Error { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token for the execution.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// Creates a new branch execution context from this context.
        /// </summary>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>A new execution context for the branch.</returns>
        public ExecutionContext CreateBranchContext(string branchId)
        {
            return new ExecutionContext
            {
                ExecutionId = ExecutionId,
                FlowId = FlowId,
                FlowVersion = FlowVersion,
                StartTimestamp = DateTime.UtcNow,
                Status = ExecutionStatus.RUNNING,
                BranchId = branchId,
                ParentBranchId = BranchId,
                Parameters = new Dictionary<string, object>(Parameters),
                CancellationToken = CancellationToken
            };
        }
    }

    /// <summary>
    /// Represents the status of a flow execution.
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// Execution is running.
        /// </summary>
        RUNNING,

        /// <summary>
        /// Execution has completed successfully.
        /// </summary>
        COMPLETED,

        /// <summary>
        /// Execution has failed.
        /// </summary>
        FAILED,

        /// <summary>
        /// Execution has been cancelled.
        /// </summary>
        CANCELLED,

        /// <summary>
        /// Execution has timed out.
        /// </summary>
        TIMEOUT
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
