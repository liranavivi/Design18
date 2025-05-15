namespace FlowOrchestrator.Management.Services
{
    /// <summary>
    /// Represents the result of a service unregistration operation.
    /// </summary>
    public class ServiceUnregistrationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the unregistration was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message if the unregistration failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error details if the unregistration failed.
        /// </summary>
        public Dictionary<string, object>? ErrorDetails { get; set; }
    }
}
