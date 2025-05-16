using System;
using System.Collections.Generic;

namespace FlowOrchestrator.Observability.Analytics.Models
{
    /// <summary>
    /// Represents a usage pattern analysis result.
    /// </summary>
    public class UsagePatternAnalysisResult
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
        /// Gets or sets the flow usage patterns.
        /// </summary>
        public List<FlowUsagePattern> FlowUsagePatterns { get; set; } = new List<FlowUsagePattern>();

        /// <summary>
        /// Gets or sets the component usage patterns.
        /// </summary>
        public List<ComponentUsagePattern> ComponentUsagePatterns { get; set; } = new List<ComponentUsagePattern>();

        /// <summary>
        /// Gets or sets the resource usage patterns.
        /// </summary>
        public List<ResourceUsagePattern> ResourceUsagePatterns { get; set; } = new List<ResourceUsagePattern>();

        /// <summary>
        /// Gets or sets the temporal patterns.
        /// </summary>
        public List<TemporalPattern> TemporalPatterns { get; set; } = new List<TemporalPattern>();
    }

    /// <summary>
    /// Represents a flow usage pattern.
    /// </summary>
    public class FlowUsagePattern
    {
        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow name.
        /// </summary>
        public string FlowName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution count.
        /// </summary>
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Gets or sets the average execution time in milliseconds.
        /// </summary>
        public double AverageExecutionTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the success rate (0.0 to 1.0).
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets or sets the average data volume in bytes.
        /// </summary>
        public long AverageDataVolumeBytes { get; set; }

        /// <summary>
        /// Gets or sets the execution frequency pattern.
        /// </summary>
        public string ExecutionFrequencyPattern { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the common error patterns.
        /// </summary>
        public List<ErrorPattern> CommonErrorPatterns { get; set; } = new List<ErrorPattern>();
    }

    /// <summary>
    /// Represents a component usage pattern.
    /// </summary>
    public class ComponentUsagePattern
    {
        /// <summary>
        /// Gets or sets the component identifier.
        /// </summary>
        public string ComponentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        public string ComponentType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the component name.
        /// </summary>
        public string ComponentName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the usage count.
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// Gets or sets the average processing time in milliseconds.
        /// </summary>
        public double AverageProcessingTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the success rate (0.0 to 1.0).
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets or sets the version distribution.
        /// </summary>
        public Dictionary<string, int> VersionDistribution { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the common error patterns.
        /// </summary>
        public List<ErrorPattern> CommonErrorPatterns { get; set; } = new List<ErrorPattern>();
    }

    /// <summary>
    /// Represents a resource usage pattern.
    /// </summary>
    public class ResourceUsagePattern
    {
        /// <summary>
        /// Gets or sets the resource type.
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        public string ResourceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the average usage percentage.
        /// </summary>
        public double AverageUsagePercentage { get; set; }

        /// <summary>
        /// Gets or sets the peak usage percentage.
        /// </summary>
        public double PeakUsagePercentage { get; set; }

        /// <summary>
        /// Gets or sets the usage pattern description.
        /// </summary>
        public string UsagePatternDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the usage trend.
        /// </summary>
        public TrendDirection UsageTrend { get; set; }

        /// <summary>
        /// Gets or sets the correlation with flow executions.
        /// </summary>
        public double CorrelationWithFlowExecutions { get; set; }
    }

    /// <summary>
    /// Represents a temporal pattern.
    /// </summary>
    public class TemporalPattern
    {
        /// <summary>
        /// Gets or sets the pattern type.
        /// </summary>
        public TemporalPatternType PatternType { get; set; }

        /// <summary>
        /// Gets or sets the pattern description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the confidence score (0.0 to 1.0).
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// Gets or sets the temporal distribution.
        /// </summary>
        public Dictionary<string, double> Distribution { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// Represents an error pattern.
    /// </summary>
    public class ErrorPattern
    {
        /// <summary>
        /// Gets or sets the error type.
        /// </summary>
        public string ErrorType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message pattern.
        /// </summary>
        public string ErrorMessagePattern { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the occurrence count.
        /// </summary>
        public int OccurrenceCount { get; set; }

        /// <summary>
        /// Gets or sets the frequency percentage.
        /// </summary>
        public double FrequencyPercentage { get; set; }

        /// <summary>
        /// Gets or sets the common context attributes.
        /// </summary>
        public Dictionary<string, string> CommonContextAttributes { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Enum for temporal pattern type.
    /// </summary>
    public enum TemporalPatternType
    {
        /// <summary>
        /// Daily pattern.
        /// </summary>
        Daily,

        /// <summary>
        /// Weekly pattern.
        /// </summary>
        Weekly,

        /// <summary>
        /// Monthly pattern.
        /// </summary>
        Monthly,

        /// <summary>
        /// Seasonal pattern.
        /// </summary>
        Seasonal,

        /// <summary>
        /// Periodic pattern.
        /// </summary>
        Periodic,

        /// <summary>
        /// Irregular pattern.
        /// </summary>
        Irregular
    }
}
