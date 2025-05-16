using System;
using System.Collections.Generic;

namespace FlowOrchestrator.Observability.Analytics.Models
{
    /// <summary>
    /// Represents a performance analysis result.
    /// </summary>
    public class PerformanceAnalysisResult
    {
        /// <summary>
        /// Gets or sets the analysis identifier.
        /// </summary>
        public string AnalysisId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the timestamp when the analysis was performed.
        /// </summary>
        public DateTime AnalysisTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the time range of the analysis.
        /// </summary>
        public TimeRange TimeRange { get; set; } = new TimeRange();

        /// <summary>
        /// Gets or sets the performance metrics.
        /// </summary>
        public List<PerformanceMetric> Metrics { get; set; } = new List<PerformanceMetric>();

        /// <summary>
        /// Gets or sets the performance trends.
        /// </summary>
        public List<PerformanceTrend> Trends { get; set; } = new List<PerformanceTrend>();

        /// <summary>
        /// Gets or sets the performance anomalies.
        /// </summary>
        public List<PerformanceAnomaly> Anomalies { get; set; } = new List<PerformanceAnomaly>();

        /// <summary>
        /// Gets or sets the performance correlations.
        /// </summary>
        public List<PerformanceCorrelation> Correlations { get; set; } = new List<PerformanceCorrelation>();
    }

    /// <summary>
    /// Represents a time range for analysis.
    /// </summary>
    public class TimeRange
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
    /// Represents a performance metric.
    /// </summary>
    public class PerformanceMetric
    {
        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric category.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the metric unit.
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the metric dimensions.
        /// </summary>
        public Dictionary<string, string> Dimensions { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the statistical properties.
        /// </summary>
        public MetricStatistics Statistics { get; set; } = new MetricStatistics();
    }

    /// <summary>
    /// Represents statistical properties of a metric.
    /// </summary>
    public class MetricStatistics
    {
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// Gets or sets the average value.
        /// </summary>
        public double Average { get; set; }

        /// <summary>
        /// Gets or sets the median value.
        /// </summary>
        public double Median { get; set; }

        /// <summary>
        /// Gets or sets the 95th percentile value.
        /// </summary>
        public double Percentile95 { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation.
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// Gets or sets the sample count.
        /// </summary>
        public int SampleCount { get; set; }
    }

    /// <summary>
    /// Represents a performance trend.
    /// </summary>
    public class PerformanceTrend
    {
        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trend direction.
        /// </summary>
        public TrendDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the trend magnitude.
        /// </summary>
        public double Magnitude { get; set; }

        /// <summary>
        /// Gets or sets the confidence score (0.0 to 1.0).
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// Gets or sets the data points used for trend analysis.
        /// </summary>
        public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
    }

    /// <summary>
    /// Represents a data point for trend analysis.
    /// </summary>
    public class DataPoint
    {
        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Represents a performance anomaly.
    /// </summary>
    public class PerformanceAnomaly
    {
        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the anomaly timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the expected value.
        /// </summary>
        public double ExpectedValue { get; set; }

        /// <summary>
        /// Gets or sets the actual value.
        /// </summary>
        public double ActualValue { get; set; }

        /// <summary>
        /// Gets or sets the deviation percentage.
        /// </summary>
        public double DeviationPercentage { get; set; }

        /// <summary>
        /// Gets or sets the severity level.
        /// </summary>
        public AnomalySeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the confidence score (0.0 to 1.0).
        /// </summary>
        public double ConfidenceScore { get; set; }
    }

    /// <summary>
    /// Represents a correlation between performance metrics.
    /// </summary>
    public class PerformanceCorrelation
    {
        /// <summary>
        /// Gets or sets the primary metric name.
        /// </summary>
        public string PrimaryMetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the secondary metric name.
        /// </summary>
        public string SecondaryMetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the correlation coefficient (-1.0 to 1.0).
        /// </summary>
        public double CorrelationCoefficient { get; set; }

        /// <summary>
        /// Gets or sets the correlation type.
        /// </summary>
        public CorrelationType Type { get; set; }

        /// <summary>
        /// Gets or sets the confidence score (0.0 to 1.0).
        /// </summary>
        public double ConfidenceScore { get; set; }
    }

    /// <summary>
    /// Enum for trend direction.
    /// </summary>
    public enum TrendDirection
    {
        /// <summary>
        /// Increasing trend.
        /// </summary>
        Increasing,

        /// <summary>
        /// Decreasing trend.
        /// </summary>
        Decreasing,

        /// <summary>
        /// Stable trend.
        /// </summary>
        Stable,

        /// <summary>
        /// Fluctuating trend.
        /// </summary>
        Fluctuating
    }

    /// <summary>
    /// Enum for anomaly severity.
    /// </summary>
    public enum AnomalySeverity
    {
        /// <summary>
        /// Low severity.
        /// </summary>
        Low,

        /// <summary>
        /// Medium severity.
        /// </summary>
        Medium,

        /// <summary>
        /// High severity.
        /// </summary>
        High,

        /// <summary>
        /// Critical severity.
        /// </summary>
        Critical
    }

    /// <summary>
    /// Enum for correlation type.
    /// </summary>
    public enum CorrelationType
    {
        /// <summary>
        /// Positive correlation.
        /// </summary>
        Positive,

        /// <summary>
        /// Negative correlation.
        /// </summary>
        Negative,

        /// <summary>
        /// No correlation.
        /// </summary>
        None
    }
}
