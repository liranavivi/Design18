using System.Collections.Generic;

namespace FlowOrchestrator.Observability.Analytics.Models
{
    /// <summary>
    /// Configuration options for the Analytics Engine.
    /// </summary>
    public class AnalyticsEngineOptions
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = "analytics-engine-01";

        /// <summary>
        /// Gets or sets the service name.
        /// </summary>
        public string ServiceName { get; set; } = "FlowOrchestrator.AnalyticsEngine";

        /// <summary>
        /// Gets or sets the URL of the Statistics Service.
        /// </summary>
        public string StatisticsServiceUrl { get; set; } = "https://localhost:7001";

        /// <summary>
        /// Gets or sets the analysis settings.
        /// </summary>
        public AnalysisSettings AnalysisSettings { get; set; } = new AnalysisSettings();

        /// <summary>
        /// Gets or sets the recommendation settings.
        /// </summary>
        public RecommendationSettings RecommendationSettings { get; set; } = new RecommendationSettings();
    }

    /// <summary>
    /// Settings for analytics analysis.
    /// </summary>
    public class AnalysisSettings
    {
        /// <summary>
        /// Gets or sets the default time range in hours for analysis.
        /// </summary>
        public int DefaultTimeRangeHours { get; set; } = 24;

        /// <summary>
        /// Gets or sets the minimum number of data points required for trend analysis.
        /// </summary>
        public int TrendAnalysisMinimumDataPoints { get; set; } = 10;

        /// <summary>
        /// Gets or sets the sensitivity for anomaly detection (higher values = more sensitive).
        /// </summary>
        public double AnomalyDetectionSensitivity { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the performance thresholds.
        /// </summary>
        public PerformanceThresholds PerformanceThresholds { get; set; } = new PerformanceThresholds();
    }

    /// <summary>
    /// Performance threshold settings.
    /// </summary>
    public class PerformanceThresholds
    {
        /// <summary>
        /// Gets or sets the threshold for flow execution time in milliseconds.
        /// </summary>
        public int FlowExecutionTimeMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the threshold for memory usage percentage.
        /// </summary>
        public int MemoryUsagePercent { get; set; } = 80;

        /// <summary>
        /// Gets or sets the threshold for CPU usage percentage.
        /// </summary>
        public int CpuUsagePercent { get; set; } = 70;

        /// <summary>
        /// Gets or sets the threshold for disk usage percentage.
        /// </summary>
        public int DiskUsagePercent { get; set; } = 85;
    }

    /// <summary>
    /// Settings for optimization recommendations.
    /// </summary>
    public class RecommendationSettings
    {
        /// <summary>
        /// Gets or sets the list of enabled recommendation types.
        /// </summary>
        public List<string> EnabledRecommendations { get; set; } = new List<string>
        {
            "ResourceAllocation",
            "FlowStructure",
            "BranchParallelism",
            "ComponentSelection",
            "MemoryManagement"
        };

        /// <summary>
        /// Gets or sets the minimum confidence score required for recommendations (0.0 to 1.0).
        /// </summary>
        public double MinimumConfidenceScore { get; set; } = 0.7;

        /// <summary>
        /// Gets or sets the maximum number of recommendations per category.
        /// </summary>
        public int MaxRecommendationsPerCategory { get; set; } = 5;
    }
}
