using FlowOrchestrator.Management.Scheduling.Messaging.Commands;

namespace FlowOrchestrator.Management.Scheduling.Messaging.Results
{
    /// <summary>
    /// Represents the result of scheduling a flow.
    /// </summary>
    public class FlowSchedulingResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the command that triggered the scheduling.
        /// </summary>
        public ScheduleFlowCommand Command { get; set; } = new ScheduleFlowCommand();

        /// <summary>
        /// Gets or sets a value indicating whether the scheduling was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the job key.
        /// </summary>
        public string JobKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trigger key.
        /// </summary>
        public string TriggerKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the next execution time.
        /// </summary>
        public DateTime? NextExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error details, if any.
        /// </summary>
        public Dictionary<string, object> ErrorDetails { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the timestamp when the scheduling was completed.
        /// </summary>
        public DateTime CompletedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowSchedulingResult"/> class.
        /// </summary>
        public FlowSchedulingResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowSchedulingResult"/> class.
        /// </summary>
        /// <param name="command">The command that triggered the scheduling.</param>
        /// <param name="success">Whether the scheduling was successful.</param>
        /// <param name="jobKey">The job key.</param>
        /// <param name="triggerKey">The trigger key.</param>
        /// <param name="nextExecutionTime">The next execution time.</param>
        public FlowSchedulingResult(
            ScheduleFlowCommand command,
            bool success,
            string jobKey,
            string triggerKey,
            DateTime? nextExecutionTime)
        {
            Command = command;
            Success = success;
            JobKey = jobKey;
            TriggerKey = triggerKey;
            NextExecutionTime = nextExecutionTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowSchedulingResult"/> class for an error.
        /// </summary>
        /// <param name="command">The command that triggered the scheduling.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorDetails">The error details.</param>
        public FlowSchedulingResult(
            ScheduleFlowCommand command,
            string errorMessage,
            Dictionary<string, object> errorDetails)
        {
            Command = command;
            Success = false;
            ErrorMessage = errorMessage;
            ErrorDetails = errorDetails;
        }
    }
}
