namespace FlowOrchestrator.Integration.Importers
{
    /// <summary>
    /// Represents errors that occur during importer service operations.
    /// </summary>
    public class ImporterServiceException : Exception
    {
        /// <summary>
        /// Gets the error code associated with the exception.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Gets additional details about the error.
        /// </summary>
        public Dictionary<string, object> ErrorDetails { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceException"/> class.
        /// </summary>
        public ImporterServiceException()
            : base("An error occurred in the importer service.")
        {
            ErrorCode = "UNKNOWN_ERROR";
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ImporterServiceException(string message)
            : base(message)
        {
            ErrorCode = "UNKNOWN_ERROR";
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImporterServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = "UNKNOWN_ERROR";
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceException"/> class with a specified error message and error code.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">The error code associated with the exception.</param>
        public ImporterServiceException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceException"/> class with a specified error message, error code, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">The error code associated with the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImporterServiceException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorDetails = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceException"/> class with a specified error message, error code, and error details.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">The error code associated with the exception.</param>
        /// <param name="errorDetails">Additional details about the error.</param>
        public ImporterServiceException(string message, string errorCode, Dictionary<string, object> errorDetails)
            : base(message)
        {
            ErrorCode = errorCode;
            ErrorDetails = errorDetails ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceException"/> class with a specified error message, error code, error details, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">The error code associated with the exception.</param>
        /// <param name="errorDetails">Additional details about the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImporterServiceException(string message, string errorCode, Dictionary<string, object> errorDetails, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorDetails = errorDetails ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Represents errors that occur during importer service configuration.
    /// </summary>
    public class ImporterConfigurationException : ImporterServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConfigurationException"/> class.
        /// </summary>
        public ImporterConfigurationException()
            : base("An error occurred in the importer service configuration.", "CONFIGURATION_ERROR")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConfigurationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ImporterConfigurationException(string message)
            : base(message, "CONFIGURATION_ERROR")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConfigurationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImporterConfigurationException(string message, Exception innerException)
            : base(message, "CONFIGURATION_ERROR", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConfigurationException"/> class with a specified error message and error details.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorDetails">Additional details about the error.</param>
        public ImporterConfigurationException(string message, Dictionary<string, object> errorDetails)
            : base(message, "CONFIGURATION_ERROR", errorDetails)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConfigurationException"/> class with a specified error message, error details, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorDetails">Additional details about the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImporterConfigurationException(string message, Dictionary<string, object> errorDetails, Exception innerException)
            : base(message, "CONFIGURATION_ERROR", errorDetails, innerException)
        {
        }
    }

    /// <summary>
    /// Represents errors that occur during importer service connection.
    /// </summary>
    public class ImporterConnectionException : ImporterServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConnectionException"/> class.
        /// </summary>
        public ImporterConnectionException()
            : base("An error occurred while connecting to the source.", "CONNECTION_ERROR")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConnectionException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ImporterConnectionException(string message)
            : base(message, "CONNECTION_ERROR")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConnectionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImporterConnectionException(string message, Exception innerException)
            : base(message, "CONNECTION_ERROR", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConnectionException"/> class with a specified error message and error details.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorDetails">Additional details about the error.</param>
        public ImporterConnectionException(string message, Dictionary<string, object> errorDetails)
            : base(message, "CONNECTION_ERROR", errorDetails)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterConnectionException"/> class with a specified error message, error details, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorDetails">Additional details about the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImporterConnectionException(string message, Dictionary<string, object> errorDetails, Exception innerException)
            : base(message, "CONNECTION_ERROR", errorDetails, innerException)
        {
        }
    }
}
