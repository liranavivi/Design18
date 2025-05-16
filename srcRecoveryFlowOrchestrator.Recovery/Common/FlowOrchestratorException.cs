namespace FlowOrchestrator.Common.Exceptions
{
    /// <summary>
    /// Base exception class for all exceptions in the FlowOrchestrator system.
    /// </summary>
    public class FlowOrchestratorException : Exception
    {
        /// <summary>
        /// Gets the error classification.
        /// </summary>
        public ErrorClassification Classification { get; }

        /// <summary>
        /// Gets additional error details.
        /// </summary>
        public Dictionary<string, object> Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowOrchestratorException"/> class.
        /// </summary>
        public FlowOrchestratorException() 
            : this("An error occurred in the FlowOrchestrator system.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowOrchestratorException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FlowOrchestratorException(string message) 
            : this(message, ErrorClassification.GENERAL_ERROR)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowOrchestratorException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public FlowOrchestratorException(string message, Exception innerException) 
            : this(message, ErrorClassification.GENERAL_ERROR, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowOrchestratorException"/> class with a specified error message and classification.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="classification">The error classification.</param>
        public FlowOrchestratorException(string message, ErrorClassification classification) 
            : this(message, classification, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowOrchestratorException"/> class with a specified error message, classification, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="classification">The error classification.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public FlowOrchestratorException(string message, ErrorClassification classification, Exception? innerException) 
            : this(message, classification, innerException, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowOrchestratorException"/> class with a specified error message, classification, a reference to the inner exception that is the cause of this exception, and additional error details.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="classification">The error classification.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="details">Additional error details.</param>
        public FlowOrchestratorException(string message, ErrorClassification classification, Exception? innerException, Dictionary<string, object>? details) 
            : base(message, innerException)
        {
            Classification = classification;
            Details = details ?? new Dictionary<string, object>();
        }
    }
}
