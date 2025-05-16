namespace FlowOrchestrator.Observability.Statistics.Models
{
    /// <summary>
    /// Represents a metric definition.
    /// </summary>
    public class MetricDefinition
    {
        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric unit.
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric type.
        /// </summary>
        public MetricType Type { get; set; } = MetricType.Counter;

        /// <summary>
        /// Gets or sets the default aggregation policy.
        /// </summary>
        public AggregationPolicy DefaultAggregation { get; set; } = AggregationPolicy.Sum;

        /// <summary>
        /// Gets or sets the default retention policy.
        /// </summary>
        public RetentionPolicy DefaultRetention { get; set; } = RetentionPolicy.Standard;

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the histogram boundaries for this metric (if applicable).
        /// </summary>
        public double[]? HistogramBoundaries { get; set; }
    }

    /// <summary>
    /// Represents the type of a metric.
    /// </summary>
    public enum MetricType
    {
        /// <summary>
        /// A counter metric that only increases.
        /// </summary>
        Counter,

        /// <summary>
        /// A gauge metric that can increase or decrease.
        /// </summary>
        Gauge,

        /// <summary>
        /// A histogram metric that tracks value distributions.
        /// </summary>
        Histogram,

        /// <summary>
        /// A timer metric that tracks durations.
        /// </summary>
        Timer,

        /// <summary>
        /// A meter metric that tracks rates.
        /// </summary>
        Meter
    }

    /// <summary>
    /// Represents an aggregation policy for metrics.
    /// </summary>
    public enum AggregationPolicy
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
        /// Last value aggregation.
        /// </summary>
        LastValue,

        /// <summary>
        /// Minimum value aggregation.
        /// </summary>
        Min,

        /// <summary>
        /// Maximum value aggregation.
        /// </summary>
        Max,

        /// <summary>
        /// Histogram aggregation.
        /// </summary>
        Histogram
    }

    /// <summary>
    /// Represents a retention policy for metrics.
    /// </summary>
    public enum RetentionPolicy
    {
        /// <summary>
        /// Short-term retention (1 day).
        /// </summary>
        ShortTerm,

        /// <summary>
        /// Standard retention (30 days).
        /// </summary>
        Standard,

        /// <summary>
        /// Long-term retention (90 days).
        /// </summary>
        LongTerm,

        /// <summary>
        /// Archive retention (1 year).
        /// </summary>
        Archive,

        /// <summary>
        /// Custom retention period.
        /// </summary>
        Custom
    }
}
