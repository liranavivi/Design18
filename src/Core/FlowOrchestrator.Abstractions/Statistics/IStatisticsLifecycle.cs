namespace FlowOrchestrator.Abstractions.Statistics
{
    /// <summary>
    /// Defines the interface for statistics lifecycle management in the FlowOrchestrator system.
    /// </summary>
    public interface IStatisticsLifecycle
    {
        /// <summary>
        /// Gets the lifecycle identifier.
        /// </summary>
        string LifecycleId { get; }

        /// <summary>
        /// Gets the lifecycle type.
        /// </summary>
        string LifecycleType { get; }

        /// <summary>
        /// Initializes the statistics lifecycle.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Starts the statistics collection.
        /// </summary>
        void StartCollection();

        /// <summary>
        /// Stops the statistics collection.
        /// </summary>
        void StopCollection();

        /// <summary>
        /// Pauses the statistics collection.
        /// </summary>
        void PauseCollection();

        /// <summary>
        /// Resumes the statistics collection.
        /// </summary>
        void ResumeCollection();

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        void ResetStatistics();

        /// <summary>
        /// Archives the statistics.
        /// </summary>
        /// <param name="archiveId">The archive identifier.</param>
        void ArchiveStatistics(string archiveId);

        /// <summary>
        /// Retrieves archived statistics.
        /// </summary>
        /// <param name="archiveId">The archive identifier.</param>
        /// <returns>The archived statistics.</returns>
        Dictionary<string, object> RetrieveArchivedStatistics(string archiveId);

        /// <summary>
        /// Gets the collection status.
        /// </summary>
        /// <returns>The collection status.</returns>
        StatisticsCollectionStatus GetCollectionStatus();

        /// <summary>
        /// Gets the collection configuration.
        /// </summary>
        /// <returns>The collection configuration.</returns>
        StatisticsCollectionConfiguration GetCollectionConfiguration();

        /// <summary>
        /// Sets the collection configuration.
        /// </summary>
        /// <param name="configuration">The collection configuration.</param>
        void SetCollectionConfiguration(StatisticsCollectionConfiguration configuration);
    }

    /// <summary>
    /// Represents the status of statistics collection.
    /// </summary>
    public enum StatisticsCollectionStatus
    {
        /// <summary>
        /// Collection is not initialized.
        /// </summary>
        UNINITIALIZED,

        /// <summary>
        /// Collection is initialized but not started.
        /// </summary>
        INITIALIZED,

        /// <summary>
        /// Collection is active.
        /// </summary>
        ACTIVE,

        /// <summary>
        /// Collection is paused.
        /// </summary>
        PAUSED,

        /// <summary>
        /// Collection is stopped.
        /// </summary>
        STOPPED,

        /// <summary>
        /// Collection is in an error state.
        /// </summary>
        ERROR
    }

    /// <summary>
    /// Represents the configuration for statistics collection.
    /// </summary>
    public class StatisticsCollectionConfiguration
    {
        /// <summary>
        /// Gets or sets the collection interval in milliseconds.
        /// </summary>
        public int CollectionIntervalMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the retention period in days.
        /// </summary>
        public int RetentionPeriodDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets a value indicating whether to collect detailed statistics.
        /// </summary>
        public bool CollectDetailedStatistics { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to archive statistics.
        /// </summary>
        public bool ArchiveStatistics { get; set; } = true;

        /// <summary>
        /// Gets or sets the archival interval in days.
        /// </summary>
        public int ArchivalIntervalDays { get; set; } = 7;

        /// <summary>
        /// Gets or sets the statistics to collect.
        /// </summary>
        public List<string> StatisticsToCollect { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the statistics to exclude.
        /// </summary>
        public List<string> StatisticsToExclude { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the sources to collect from.
        /// </summary>
        public List<string> SourcesToCollectFrom { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the sources to exclude.
        /// </summary>
        public List<string> SourcesToExclude { get; set; } = new List<string>();
    }
}
