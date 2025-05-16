using System.Diagnostics.Metrics;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models
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
        /// Gets or sets the metric tags.
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
        /// An up-down counter that can increase or decrease.
        /// </summary>
        UpDownCounter
    }
}
