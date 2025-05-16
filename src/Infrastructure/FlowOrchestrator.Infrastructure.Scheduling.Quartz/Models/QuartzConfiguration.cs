namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Models
{
    /// <summary>
    /// Configuration for the Quartz scheduler.
    /// </summary>
    public class QuartzConfiguration
    {
        /// <summary>
        /// Gets or sets the scheduler name.
        /// </summary>
        public string SchedulerName { get; set; } = "FlowOrchestratorScheduler";

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
        /// Gets or sets the job store type.
        /// </summary>
        public string JobStoreType { get; set; } = "Quartz.Simpl.RAMJobStore, Quartz";

        /// <summary>
        /// Gets or sets a value indicating whether to skip update check.
        /// </summary>
        public bool SkipUpdateCheck { get; set; } = true;

        /// <summary>
        /// Gets or sets the serializer type.
        /// </summary>
        public string SerializerType { get; set; } = "json";

        /// <summary>
        /// Gets or sets a value indicating whether to auto start the scheduler.
        /// </summary>
        public bool AutoStart { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to wait for jobs to complete on shutdown.
        /// </summary>
        public bool WaitForJobsToCompleteOnShutdown { get; set; } = true;

        /// <summary>
        /// Gets or sets the shutdown timeout in seconds.
        /// </summary>
        public int ShutdownTimeoutSeconds { get; set; } = 10;
    }
}
