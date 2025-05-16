namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Models
{
    /// <summary>
    /// Represents the status of a job.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// The job is not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// The job is scheduled.
        /// </summary>
        Scheduled,

        /// <summary>
        /// The job is running.
        /// </summary>
        Running,

        /// <summary>
        /// The job is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The job is completed.
        /// </summary>
        Completed,

        /// <summary>
        /// The job is in an error state.
        /// </summary>
        Error
    }
}
