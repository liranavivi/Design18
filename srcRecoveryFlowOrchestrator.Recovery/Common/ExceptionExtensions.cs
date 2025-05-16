namespace FlowOrchestrator.Common.Exceptions
{
    /// <summary>
    /// Provides extension methods for exceptions.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Gets the error classification for an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The error classification.</returns>
        public static ErrorClassification GetErrorClassification(this Exception exception)
        {
            if (exception is FlowOrchestratorException flowOrchestratorException)
            {
                return flowOrchestratorException.Classification;
            }

            return ClassifyException(exception);
        }

        /// <summary>
        /// Classifies an exception based on its type.
        /// </summary>
        /// <param name="exception">The exception to classify.</param>
        /// <returns>The error classification.</returns>
        private static ErrorClassification ClassifyException(Exception exception)
        {
            var exceptionType = exception.GetType().Name;

            return exceptionType switch
            {
                "TimeoutException" => ErrorClassification.CONNECTION_TIMEOUT,
                "HttpRequestException" => ErrorClassification.CONNECTION_ERROR,
                "SocketException" => ErrorClassification.CONNECTION_ERROR,
                "WebException" => ErrorClassification.CONNECTION_ERROR,
                "UnauthorizedAccessException" => ErrorClassification.AUTHENTICATION_INSUFFICIENT_PERMISSIONS,
                "SecurityException" => ErrorClassification.AUTHENTICATION_ERROR,
                "FormatException" => ErrorClassification.DATA_INVALID_FORMAT,
                "JsonException" => ErrorClassification.DATA_INVALID_FORMAT,
                "XmlException" => ErrorClassification.DATA_INVALID_FORMAT,
                "InvalidDataException" => ErrorClassification.DATA_ERROR,
                "FileNotFoundException" => ErrorClassification.RESOURCE_NOT_FOUND,
                "DirectoryNotFoundException" => ErrorClassification.RESOURCE_NOT_FOUND,
                "IOException" => ErrorClassification.RESOURCE_ERROR,
                "OutOfMemoryException" => ErrorClassification.RESOURCE_QUOTA_EXCEEDED,
                "ArgumentException" => ErrorClassification.VALIDATION_ERROR,
                "ArgumentNullException" => ErrorClassification.VALIDATION_ERROR,
                "ArgumentOutOfRangeException" => ErrorClassification.VALIDATION_ERROR,
                "InvalidOperationException" => ErrorClassification.PROCESSING_ERROR,
                "NotSupportedException" => ErrorClassification.PROCESSING_ERROR,
                "NotImplementedException" => ErrorClassification.PROCESSING_ERROR,
                _ => ErrorClassification.GENERAL_ERROR
            };
        }
    }
}
