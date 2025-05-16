namespace FlowOrchestrator.Recovery.Models
{
    /// <summary>
    /// Represents the result of a recovery operation.
    /// </summary>
    public class RecoveryResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the recovery was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the recovery action.
        /// </summary>
        public RecoveryAction Action { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the recovery context.
        /// </summary>
        public RecoveryContext Context { get; set; } = new RecoveryContext();

        /// <summary>
        /// Gets or sets the next retry delay.
        /// </summary>
        public TimeSpan? NextRetryDelay { get; set; }

        /// <summary>
        /// Gets or sets the error context if the recovery failed.
        /// </summary>
        public ErrorContext? ErrorContext { get; set; }

        /// <summary>
        /// Gets or sets additional result data.
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a successful recovery result.
        /// </summary>
        /// <param name="action">The recovery action.</param>
        /// <param name="message">The message.</param>
        /// <param name="context">The recovery context.</param>
        /// <returns>A successful recovery result.</returns>
        public static RecoveryResult Successful(RecoveryAction action, string message, RecoveryContext context)
        {
            return new RecoveryResult
            {
                Success = true,
                Action = action,
                Message = message,
                Context = context
            };
        }

        /// <summary>
        /// Creates a failed recovery result.
        /// </summary>
        /// <param name="action">The recovery action.</param>
        /// <param name="message">The message.</param>
        /// <param name="context">The recovery context.</param>
        /// <param name="errorContext">The error context.</param>
        /// <returns>A failed recovery result.</returns>
        public static RecoveryResult Failed(RecoveryAction action, string message, RecoveryContext context, ErrorContext errorContext)
        {
            return new RecoveryResult
            {
                Success = false,
                Action = action,
                Message = message,
                Context = context,
                ErrorContext = errorContext
            };
        }
    }

    /// <summary>
    /// Represents the recovery action to take.
    /// </summary>
    public enum RecoveryAction
    {
        /// <summary>
        /// Retry the operation.
        /// </summary>
        RETRY,

        /// <summary>
        /// Fail the branch but continue with other branches.
        /// </summary>
        FAIL_BRANCH,

        /// <summary>
        /// Fail the entire execution.
        /// </summary>
        FAIL_EXECUTION,

        /// <summary>
        /// Fail fast without retrying.
        /// </summary>
        FAIL_FAST,

        /// <summary>
        /// Use a fallback operation.
        /// </summary>
        USE_FALLBACK,

        /// <summary>
        /// Apply compensation actions.
        /// </summary>
        COMPENSATE,

        /// <summary>
        /// Continue with degraded functionality.
        /// </summary>
        CONTINUE_DEGRADED
    }
}
