namespace FlowOrchestrator.Abstractions.Statistics
{
    /// <summary>
    /// Defines the interface for statistics consumers in the FlowOrchestrator system.
    /// </summary>
    public interface IStatisticsConsumer
    {
        /// <summary>
        /// Gets the consumer identifier.
        /// </summary>
        string ConsumerId { get; }

        /// <summary>
        /// Gets the consumer type.
        /// </summary>
        string ConsumerType { get; }

        /// <summary>
        /// Consumes statistics.
        /// </summary>
        /// <param name="statistics">The statistics to consume.</param>
        /// <param name="source">The source of the statistics.</param>
        void ConsumeStatistics(Dictionary<string, object> statistics, StatisticsSource source);

        /// <summary>
        /// Consumes a specific statistic.
        /// </summary>
        /// <param name="key">The statistic key.</param>
        /// <param name="value">The statistic value.</param>
        /// <param name="source">The source of the statistic.</param>
        void ConsumeStatistic(string key, object value, StatisticsSource source);

        /// <summary>
        /// Gets the supported statistic keys.
        /// </summary>
        /// <returns>The supported statistic keys.</returns>
        List<string> GetSupportedStatisticKeys();

        /// <summary>
        /// Gets the supported source types.
        /// </summary>
        /// <returns>The supported source types.</returns>
        List<string> GetSupportedSourceTypes();
    }

    /// <summary>
    /// Represents the source of statistics.
    /// </summary>
    public class StatisticsSource
    {
        /// <summary>
        /// Gets or sets the source identifier.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source type.
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the statistics were collected.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the source metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsSource"/> class.
        /// </summary>
        public StatisticsSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsSource"/> class with the specified source identifier and type.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <param name="sourceType">The source type.</param>
        public StatisticsSource(string sourceId, string sourceType)
        {
            SourceId = sourceId;
            SourceType = sourceType;
        }
    }
}
