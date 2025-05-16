namespace FlowOrchestrator.Common.Exceptions
{
    /// <summary>
    /// Defines error classifications for the FlowOrchestrator system.
    /// </summary>
    public enum ErrorClassification
    {
        /// <summary>
        /// General error.
        /// </summary>
        GENERAL_ERROR,

        /// <summary>
        /// Connection error.
        /// </summary>
        CONNECTION_ERROR,

        /// <summary>
        /// Connection timeout error.
        /// </summary>
        CONNECTION_TIMEOUT,

        /// <summary>
        /// Connection unreachable error.
        /// </summary>
        CONNECTION_UNREACHABLE,

        /// <summary>
        /// Connection handshake failure error.
        /// </summary>
        CONNECTION_HANDSHAKE_FAILURE,

        /// <summary>
        /// Authentication error.
        /// </summary>
        AUTHENTICATION_ERROR,

        /// <summary>
        /// Invalid credentials error.
        /// </summary>
        AUTHENTICATION_INVALID_CREDENTIALS,

        /// <summary>
        /// Expired token error.
        /// </summary>
        AUTHENTICATION_EXPIRED_TOKEN,

        /// <summary>
        /// Insufficient permissions error.
        /// </summary>
        AUTHENTICATION_INSUFFICIENT_PERMISSIONS,

        /// <summary>
        /// Data error.
        /// </summary>
        DATA_ERROR,

        /// <summary>
        /// Invalid data format error.
        /// </summary>
        DATA_INVALID_FORMAT,

        /// <summary>
        /// Schema violation error.
        /// </summary>
        DATA_SCHEMA_VIOLATION,

        /// <summary>
        /// Data corruption error.
        /// </summary>
        DATA_CORRUPTION,

        /// <summary>
        /// Resource error.
        /// </summary>
        RESOURCE_ERROR,

        /// <summary>
        /// Resource not found error.
        /// </summary>
        RESOURCE_NOT_FOUND,

        /// <summary>
        /// Resource unavailable error.
        /// </summary>
        RESOURCE_UNAVAILABLE,

        /// <summary>
        /// Resource quota exceeded error.
        /// </summary>
        RESOURCE_QUOTA_EXCEEDED,

        /// <summary>
        /// Processing error.
        /// </summary>
        PROCESSING_ERROR,

        /// <summary>
        /// Validation error.
        /// </summary>
        VALIDATION_ERROR,

        /// <summary>
        /// Configuration error.
        /// </summary>
        CONFIGURATION_ERROR,

        /// <summary>
        /// Version compatibility error.
        /// </summary>
        VERSION_COMPATIBILITY_ERROR
    }
}
