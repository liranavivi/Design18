namespace FlowOrchestrator.Management.Services
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
        /// The service is in an error state.
        /// </summary>
        ERROR,

        /// <summary>
        /// The service is in maintenance mode.
        /// </summary>
        MAINTENANCE,

        /// <summary>
        /// The service is being initialized.
        /// </summary>
        INITIALIZING,

        /// <summary>
        /// The service is being terminated.
        /// </summary>
        TERMINATING
    }
}
