namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Represents the status of a service.
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// The service is active.
        /// </summary>
        ACTIVE,

        /// <summary>
        /// The service is inactive.
        /// </summary>
        INACTIVE,

        /// <summary>
        /// The service is suspended.
        /// </summary>
        SUSPENDED,

        /// <summary>
        /// The service is deprecated.
        /// </summary>
        DEPRECATED,

        /// <summary>
        /// The service is retired.
        /// </summary>
        RETIRED
    }
}
