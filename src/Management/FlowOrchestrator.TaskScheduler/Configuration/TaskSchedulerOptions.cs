namespace FlowOrchestrator.Management.Scheduling.Configuration
{
    /// <summary>
    /// Options for the task scheduler.
    /// </summary>
    public class TaskSchedulerOptions
    {
        /// <summary>
        /// Gets or sets the scheduler name.
        /// </summary>
        public string SchedulerName { get; set; } = "FlowOrchestratorTaskScheduler";

        /// <summary>
        /// Gets or sets the instance ID.
        /// </summary>
        public string InstanceId { get; set; } = "AUTO";

        /// <summary>
        /// Gets or sets the thread count.
        /// </summary>
        public int ThreadCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether to make scheduler threads daemon threads.
        /// </summary>
        public bool MakeSchedulerThreadDaemon { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to auto start the scheduler.
        /// </summary>
        public bool AutoStart { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to wait for jobs to complete on shutdown.
        /// </summary>
        public bool WaitForJobsToCompleteOnShutdown { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of concurrent executions.
        /// </summary>
        public int MaxConcurrentExecutions { get; set; } = 10;

        /// <summary>
        /// Gets or sets the default timeout in seconds.
        /// </summary>
        public int DefaultTimeoutSeconds { get; set; } = 3600;

        /// <summary>
        /// Gets or sets the default retry count.
        /// </summary>
        public int DefaultRetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the default retry delay in seconds.
        /// </summary>
        public int DefaultRetryDelaySeconds { get; set; } = 300;
    }
}
