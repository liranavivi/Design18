namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the status of a service.
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// The service is unknown or not registered.
        /// </summary>
        UNKNOWN,

        /// <summary>
        /// The service is registered but not initialized.
        /// </summary>
        REGISTERED,

        /// <summary>
        /// The service is initialized but not running.
        /// </summary>
        INITIALIZED,

        /// <summary>
        /// The service is running.
        /// </summary>
        RUNNING,

        /// <summary>
        /// The service is paused.
        /// </summary>
        PAUSED,

        /// <summary>
        /// The service is stopped.
        /// </summary>
        STOPPED,

        /// <summary>
        /// The service is terminated.
        /// </summary>
        TERMINATED,

        /// <summary>
        /// The service is in an error state.
        /// </summary>
        ERROR
    }
}
