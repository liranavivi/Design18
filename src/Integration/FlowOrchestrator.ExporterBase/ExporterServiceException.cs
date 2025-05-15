namespace FlowOrchestrator.Integration.Exporters
{
    /// <summary>
    /// Exception thrown by exporter services.
    /// </summary>
    public class ExporterServiceException : Exception
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Gets the error details.
        /// </summary>
        public Dictionary<string, object> ErrorDetails { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceException"/> class.
        /// </summary>
        public ExporterServiceException()
            : base("An error occurred in the exporter service.")
        {
            ErrorCode = "EXPORTER_ERROR";
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceException"/> class with the specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ExporterServiceException(string message)
            : base(message)
        {
            ErrorCode = "EXPORTER_ERROR";
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceException"/> class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ExporterServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = "EXPORTER_ERROR";
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceException"/> class with the specified error code and message.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The error message.</param>
        public ExporterServiceException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceException"/> class with the specified error code, message, and inner exception.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ExporterServiceException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceException"/> class with the specified error code, message, and error details.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="errorDetails">The error details.</param>
        public ExporterServiceException(string errorCode, string message, Dictionary<string, object> errorDetails)
            : base(message)
        {
            ErrorCode = errorCode;
            ErrorDetails = errorDetails ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceException"/> class with the specified error code, message, error details, and inner exception.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="errorDetails">The error details.</param>
        /// <param name="innerException">The inner exception.</param>
        public ExporterServiceException(string errorCode, string message, Dictionary<string, object> errorDetails, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorDetails = errorDetails ?? new Dictionary<string, object>();
        }
    }
}
