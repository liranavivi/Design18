namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Models
{
    /// <summary>
    /// Represents the status of a scheduler.
    /// </summary>
    public enum SchedulerStatus
    {
        /// <summary>
        /// The scheduler is not initialized.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// The scheduler is initialized but not started.
        /// </summary>
        Initialized,

        /// <summary>
        /// The scheduler is running.
        /// </summary>
        Running,

        /// <summary>
        /// The scheduler is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The scheduler is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// The scheduler is in an error state.
        /// </summary>
        Error
    }
}
