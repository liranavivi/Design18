using FlowOrchestrator.Common.Exceptions;

namespace FlowOrchestrator.Recovery.Models
{
    /// <summary>
    /// Represents the context of an error that occurred in the system.
    /// </summary>
    public class ErrorContext
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
        /// Gets or sets the error classification.
        /// </summary>
        public ErrorClassification Classification { get; set; }

        /// <summary>
        /// Gets or sets the error severity.
        /// </summary>
        public ErrorSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the service identifier that generated the error.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string? ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the branch identifier.
        /// </summary>
        public string? BranchId { get; set; }

        /// <summary>
        /// Gets or sets the step identifier.
        /// </summary>
        public string? StepId { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the number of retry attempts.
        /// </summary>
        public int RetryAttempts { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retry attempts.
        /// </summary>
        public int MaxRetryAttempts { get; set; }

        /// <summary>
        /// Gets or sets the exception that caused the error.
        /// </summary>
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Represents the severity of an error.
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Critical severity.
        /// </summary>
        CRITICAL,

        /// <summary>
        /// Major severity.
        /// </summary>
        MAJOR,

        /// <summary>
        /// Minor severity.
        /// </summary>
        MINOR,

        /// <summary>
        /// Warning severity.
        /// </summary>
        WARNING,

        /// <summary>
        /// Informational severity.
        /// </summary>
        INFO
    }
}
