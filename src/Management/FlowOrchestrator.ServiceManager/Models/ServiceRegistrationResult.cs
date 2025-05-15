namespace FlowOrchestrator.Management.Services
{
    /// <summary>
    /// Represents the result of a service registration operation.
    /// </summary>
    public class ServiceRegistrationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the registration was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message if the registration failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error details if the registration failed.
        /// </summary>
        public Dictionary<string, object>? ErrorDetails { get; set; }
    }
}
