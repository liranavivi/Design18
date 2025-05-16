namespace FlowOrchestrator.Observability.Statistics.Models
{
    /// <summary>
    /// Represents the result of a metric query.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// Gets or sets the query that produced this result.
        /// </summary>
        public MetricQuery Query { get; set; } = new MetricQuery();

        /// <summary>
        /// Gets or sets the metric records.
        /// </summary>
        public List<MetricRecord> Records { get; set; } = new List<MetricRecord>();

        /// <summary>
        /// Gets or sets the aggregated values.
        /// </summary>
        public Dictionary<string, object> AggregatedValues { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the time series data.
        /// </summary>
        public Dictionary<string, List<TimeSeriesPoint>> TimeSeries { get; set; } = new Dictionary<string, List<TimeSeriesPoint>>();

        /// <summary>
        /// Gets or sets a value indicating whether the query was successful.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets the error message if the query was not successful.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the execution time of the query in milliseconds.
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the total count of records that matched the query.
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Represents a point in a time series.
    /// </summary>
    public class TimeSeriesPoint
    {
        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSeriesPoint"/> class.
        /// </summary>
        public TimeSeriesPoint()
        {
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSeriesPoint"/> class with the specified timestamp and value.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="value">The value.</param>
        public TimeSeriesPoint(DateTime timestamp, object value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }
}
