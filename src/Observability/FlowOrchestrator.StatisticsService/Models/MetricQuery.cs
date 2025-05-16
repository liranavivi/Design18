namespace FlowOrchestrator.Observability.Statistics.Models
{
    /// <summary>
    /// Represents a query for metrics.
    /// </summary>
    public class MetricQuery
    {
        /// <summary>
        /// Gets or sets the metric names to query.
        /// </summary>
        public List<string> MetricNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow.AddHours(-1);

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the filter attributes.
        /// </summary>
        public Dictionary<string, string> FilterAttributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the aggregation type.
        /// </summary>
        public AggregationType AggregationType { get; set; } = AggregationType.None;

        /// <summary>
        /// Gets or sets the time interval for aggregation.
        /// </summary>
        public TimeSpan? AggregationInterval { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of results to return.
        /// </summary>
        public int? MaxResults { get; set; }
    }

    /// <summary>
    /// Represents the type of aggregation to apply to metrics.
    /// </summary>
    public enum AggregationType
    {
        /// <summary>
        /// No aggregation.
        /// </summary>
        None,

        /// <summary>
        /// Sum aggregation.
        /// </summary>
        Sum,

        /// <summary>
        /// Average aggregation.
        /// </summary>
        Average,

        /// <summary>
        /// Minimum aggregation.
        /// </summary>
        Min,

        /// <summary>
        /// Maximum aggregation.
        /// </summary>
        Max,

        /// <summary>
        /// Count aggregation.
        /// </summary>
        Count,

        /// <summary>
        /// Percentile aggregation.
        /// </summary>
        Percentile
    }
}
