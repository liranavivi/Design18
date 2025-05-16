using System;
using System.Collections.Generic;

namespace FlowOrchestrator.Observability.Analytics.Models
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
        /// Gets or sets the metric category.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric type.
        /// </summary>
        public MetricType Type { get; set; } = MetricType.Counter;

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
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
    /// Represents a query for metrics.
    /// </summary>
    public class MetricQuery
    {
        /// <summary>
        /// Gets or sets the metric names to query.
        /// </summary>
        public List<string> MetricNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the time range for the query.
        /// </summary>
        public QueryTimeRange TimeRange { get; set; } = new QueryTimeRange();

        /// <summary>
        /// Gets or sets the filter criteria.
        /// </summary>
        public Dictionary<string, string> Filter { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the group by dimensions.
        /// </summary>
        public List<string> GroupBy { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the aggregation type.
        /// </summary>
        public string Aggregation { get; set; } = "none";

        /// <summary>
        /// Gets or sets the limit of results to return.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        public string? SortField { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        public string? SortOrder { get; set; }
    }

    /// <summary>
    /// Represents a time range for a query.
    /// </summary>
    public class QueryTimeRange
    {
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow.AddHours(-24);

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
    }

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
        /// Gets or sets the data returned by the query.
        /// </summary>
        public Dictionary<string, object>? Data { get; set; }

        /// <summary>
        /// Gets or sets the grouped data returned by the query.
        /// </summary>
        public Dictionary<string, Dictionary<string, object>>? GroupedData { get; set; }
    }

    /// <summary>
    /// Represents an alert definition.
    /// </summary>
    public class AlertDefinition
    {
        /// <summary>
        /// Gets or sets the alert identifier.
        /// </summary>
        public string AlertId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the alert name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the alert description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the threshold value.
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// Gets or sets the alert severity.
        /// </summary>
        public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

        /// <summary>
        /// Gets or sets the alert condition.
        /// </summary>
        public AlertCondition Condition { get; set; } = AlertCondition.GreaterThan;

        /// <summary>
        /// Gets or sets the alert status.
        /// </summary>
        public AlertStatus Status { get; set; } = AlertStatus.Enabled;
    }

    /// <summary>
    /// Represents the severity of an alert.
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// Information severity.
        /// </summary>
        Information,

        /// <summary>
        /// Warning severity.
        /// </summary>
        Warning,

        /// <summary>
        /// Error severity.
        /// </summary>
        Error,

        /// <summary>
        /// Critical severity.
        /// </summary>
        Critical
    }

    /// <summary>
    /// Represents the condition for an alert.
    /// </summary>
    public enum AlertCondition
    {
        /// <summary>
        /// Greater than condition.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Less than condition.
        /// </summary>
        LessThan,

        /// <summary>
        /// Equal to condition.
        /// </summary>
        EqualTo,

        /// <summary>
        /// Not equal to condition.
        /// </summary>
        NotEqualTo
    }

    /// <summary>
    /// Represents the status of an alert.
    /// </summary>
    public enum AlertStatus
    {
        /// <summary>
        /// Enabled status.
        /// </summary>
        Enabled,

        /// <summary>
        /// Disabled status.
        /// </summary>
        Disabled,

        /// <summary>
        /// Triggered status.
        /// </summary>
        Triggered,

        /// <summary>
        /// Acknowledged status.
        /// </summary>
        Acknowledged
    }

    /// <summary>
    /// Represents a source of statistics.
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
    }
}
