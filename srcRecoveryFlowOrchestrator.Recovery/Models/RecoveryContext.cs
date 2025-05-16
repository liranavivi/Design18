using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Recovery.Models
{
    /// <summary>
    /// Represents the context for a recovery operation.
    /// </summary>
    public class RecoveryContext
    {
        /// <summary>
        /// Gets or sets the error context.
        /// </summary>
        public ErrorContext ErrorContext { get; set; } = new ErrorContext();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? ExecutionContext { get; set; }

        /// <summary>
        /// Gets or sets the recovery identifier.
        /// </summary>
        public string RecoveryId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the timestamp when the recovery operation started.
        /// </summary>
        public DateTime StartTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the recovery strategy name.
        /// </summary>
        public string StrategyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the recovery parameters.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the recovery state.
        /// </summary>
        public Dictionary<string, object> State { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the recovery history.
        /// </summary>
        public List<RecoveryAttempt> History { get; set; } = new List<RecoveryAttempt>();
    }

    /// <summary>
    /// Represents a recovery attempt.
    /// </summary>
    public class RecoveryAttempt
    {
        /// <summary>
        /// Gets or sets the attempt number.
        /// </summary>
        public int AttemptNumber { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the attempt was made.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the strategy name.
        /// </summary>
        public string StrategyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the attempt was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the result message.
        /// </summary>
        public string ResultMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error context if the attempt failed.
        /// </summary>
        public ErrorContext? ErrorContext { get; set; }
    }
}
