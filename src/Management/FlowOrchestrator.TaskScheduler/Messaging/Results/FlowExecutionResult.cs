using FlowOrchestrator.Management.Scheduling.Messaging.Commands;
using ExecutionStatus = FlowOrchestrator.Abstractions.Common.ExecutionStatus;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Management.Scheduling.Messaging.Results
{
    /// <summary>
    /// Represents the result of a flow execution.
    /// </summary>
    public class FlowExecutionResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the command that triggered the execution.
        /// </summary>
        public TriggerFlowCommand Command { get; set; } = new TriggerFlowCommand();

        /// <summary>
        /// Gets or sets a value indicating whether the execution was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the execution status.
        /// </summary>
        public ExecutionStatus Status { get; set; } = ExecutionStatus.RUNNING;

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error details, if any.
        /// </summary>
        public Dictionary<string, object> ErrorDetails { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public ExecutionContext Context { get; set; } = new ExecutionContext();

        /// <summary>
        /// Gets or sets the timestamp when the execution was completed.
        /// </summary>
        public DateTime CompletedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowExecutionResult"/> class.
        /// </summary>
        public FlowExecutionResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowExecutionResult"/> class.
        /// </summary>
        /// <param name="command">The command that triggered the execution.</param>
        /// <param name="success">Whether the execution was successful.</param>
        /// <param name="status">The execution status.</param>
        /// <param name="context">The execution context.</param>
        public FlowExecutionResult(
            TriggerFlowCommand command,
            bool success,
            ExecutionStatus status,
            ExecutionContext context)
        {
            Command = command;
            Success = success;
            Status = status;
            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowExecutionResult"/> class for an error.
        /// </summary>
        /// <param name="command">The command that triggered the execution.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorDetails">The error details.</param>
        /// <param name="context">The execution context.</param>
        public FlowExecutionResult(
            TriggerFlowCommand command,
            string errorMessage,
            Dictionary<string, object> errorDetails,
            ExecutionContext context)
        {
            Command = command;
            Success = false;
            Status = ExecutionStatus.FAILED;
            ErrorMessage = errorMessage;
            ErrorDetails = errorDetails;
            Context = context;
        }
    }
}
