namespace FlowOrchestrator.Observability.Statistics.Models
{
    /// <summary>
    /// Configuration options for the statistics service.
    /// </summary>
    public class StatisticsOptions
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = "StatisticsService";

        /// <summary>
        /// Gets or sets the service type.
        /// </summary>
        public string ServiceType { get; set; } = "StatisticsService";

        /// <summary>
        /// Gets or sets the retention period in days.
        /// </summary>
        public int RetentionPeriodDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the archival interval in days.
        /// </summary>
        public int ArchivalIntervalDays { get; set; } = 7;

        /// <summary>
        /// Gets or sets the collection interval in milliseconds.
        /// </summary>
        public int CollectionIntervalMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether detailed statistics are enabled.
        /// </summary>
        public bool DetailedStatisticsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether historical data is enabled.
        /// </summary>
        public bool HistoricalDataEnabled { get; set; } = true;
    }
}
