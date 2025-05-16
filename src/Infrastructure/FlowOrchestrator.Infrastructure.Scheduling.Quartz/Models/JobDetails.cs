namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Models
{
    /// <summary>
    /// Represents the details of a job.
    /// </summary>
    public class JobDetails
    {
        /// <summary>
        /// Gets or sets the job key.
        /// </summary>
        public string JobKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the job group.
        /// </summary>
        public string JobGroup { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the job description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the job status.
        /// </summary>
        public JobStatus Status { get; set; } = JobStatus.NotFound;

        /// <summary>
        /// Gets or sets the next fire time.
        /// </summary>
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// Gets or sets the previous fire time.
        /// </summary>
        public DateTime? PreviousFireTime { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the job data map.
        /// </summary>
        public Dictionary<string, string> JobDataMap { get; set; } = new Dictionary<string, string>();
    }
}
