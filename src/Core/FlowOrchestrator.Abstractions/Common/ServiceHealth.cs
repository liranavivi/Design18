namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the health status of a service.
    /// </summary>
    public enum ServiceHealth
    {
        /// <summary>
        /// The service is healthy and operating normally.
        /// </summary>
        HEALTHY,

        /// <summary>
        /// The service is operating with warnings or degraded performance.
        /// </summary>
        WARNING,

        /// <summary>
        /// The service is unhealthy or not functioning properly.
        /// </summary>
        UNHEALTHY,

        /// <summary>
        /// The service health status is unknown.
        /// </summary>
        UNKNOWN
    }
}
