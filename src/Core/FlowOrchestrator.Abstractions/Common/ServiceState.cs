namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the possible states of a service in the FlowOrchestrator system.
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// Service has not been initialized.
        /// </summary>
        UNINITIALIZED,

        /// <summary>
        /// Service is in the process of initializing.
        /// </summary>
        INITIALIZING,

        /// <summary>
        /// Service has been initialized and is ready to process requests.
        /// </summary>
        READY,

        /// <summary>
        /// Service is currently processing a request.
        /// </summary>
        PROCESSING,

        /// <summary>
        /// Service has encountered an error and is in a faulted state.
        /// </summary>
        ERROR,

        /// <summary>
        /// Service is in the process of terminating.
        /// </summary>
        TERMINATING,

        /// <summary>
        /// Service has been terminated.
        /// </summary>
        TERMINATED
    }
}
