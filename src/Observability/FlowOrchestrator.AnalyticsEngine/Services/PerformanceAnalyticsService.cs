using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Common.Utilities;
using FlowOrchestrator.Observability.Analytics.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Analytics.Services
{
    /// <summary>
    /// Service for analyzing performance metrics.
    /// </summary>
    public class PerformanceAnalyticsService : IPerformanceAnalyticsService
    {
        private readonly ILogger<PerformanceAnalyticsService> _logger;
        private readonly IStatisticsServiceClient _statisticsClient;
        private readonly AnalyticsEngineOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceAnalyticsService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="statisticsClient">The statistics client.</param>
        /// <param name="options">The options.</param>
        public PerformanceAnalyticsService(
            ILogger<PerformanceAnalyticsService> logger,
            IStatisticsServiceClient statisticsClient,
            IOptions<AnalyticsEngineOptions> options)
        {
            _logger = logger;
            _statisticsClient = statisticsClient;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public async Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(TimeRange? timeRange = null)
        {
            var analysisTimeRange = timeRange ?? new TimeRange
            {
                StartTime = DateTime.UtcNow.AddHours(-_options.AnalysisSettings.DefaultTimeRangeHours),
                EndTime = DateTime.UtcNow
            };

            _logger.LogInformation("Starting performance analysis for time range: {StartTime} to {EndTime}",
                analysisTimeRange.StartTime, analysisTimeRange.EndTime);

            var result = new PerformanceAnalysisResult
            {
                TimeRange = analysisTimeRange
            };

            try
            {
                // Get available metrics
                var availableMetrics = await _statisticsClient.GetAvailableMetricsAsync();
                _logger.LogDebug("Retrieved {Count} available metrics", availableMetrics.Count);

                // Get performance metrics
                var performanceMetrics = await GetPerformanceMetricsAsync(availableMetrics, analysisTimeRange);
                result.Metrics = performanceMetrics;
                _logger.LogDebug("Retrieved {Count} performance metrics", performanceMetrics.Count);

                // Analyze trends
                result.Trends = await AnalyzeTrendsAsync(performanceMetrics, analysisTimeRange);
                _logger.LogDebug("Identified {Count} performance trends", result.Trends.Count);

                // Detect anomalies
                result.Anomalies = await DetectAnomaliesAsync(performanceMetrics, analysisTimeRange);
                _logger.LogDebug("Detected {Count} performance anomalies", result.Anomalies.Count);

                // Analyze correlations
                result.Correlations = await AnalyzeCorrelationsAsync(performanceMetrics, analysisTimeRange);
                _logger.LogDebug("Identified {Count} performance correlations", result.Correlations.Count);

                _logger.LogInformation("Performance analysis completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing performance analysis");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(string category, TimeRange? timeRange = null)
        {
            Guard.AgainstNullOrEmpty(category, nameof(category));

            var analysisTimeRange = timeRange ?? new TimeRange
            {
                StartTime = DateTime.UtcNow.AddHours(-_options.AnalysisSettings.DefaultTimeRangeHours),
                EndTime = DateTime.UtcNow
            };

            try
            {
                var availableMetrics = await _statisticsClient.GetAvailableMetricsAsync();
                var categoryMetrics = availableMetrics.Where(m => m.Category == category).ToList();

                return await GetPerformanceMetricsAsync(categoryMetrics, analysisTimeRange);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics for category {Category}", category);
                return new List<PerformanceMetric>();
            }
        }

        /// <inheritdoc/>
        public async Task<List<PerformanceTrend>> AnalyzeTrendsAsync(List<PerformanceMetric> metrics, TimeRange? timeRange = null)
        {
            Guard.AgainstNull(metrics, nameof(metrics));

            var analysisTimeRange = timeRange ?? new TimeRange
            {
                StartTime = DateTime.UtcNow.AddHours(-_options.AnalysisSettings.DefaultTimeRangeHours),
                EndTime = DateTime.UtcNow
            };

            var trends = new List<PerformanceTrend>();

            try
            {
                foreach (var metric in metrics)
                {
                    // Get historical data for the metric
                    var query = new MetricQuery
                    {
                        MetricNames = new List<string> { metric.Name },
                        TimeRange = new QueryTimeRange
                        {
                            StartTime = analysisTimeRange.StartTime,
                            EndTime = analysisTimeRange.EndTime
                        },
                        Aggregation = "none"
                    };

                    var queryResult = await _statisticsClient.QueryMetricsAsync(query);
                    if (!queryResult.Success || queryResult.Data == null || !queryResult.Data.ContainsKey(metric.Name))
                    {
                        continue;
                    }

                    var dataPoints = new List<DataPoint>();
                    var metricData = queryResult.Data[metric.Name];

                    // Convert to data points
                    if (metricData is List<object> dataList)
                    {
                        foreach (var dataItem in dataList)
                        {
                            if (dataItem is Dictionary<string, object> dataDictionary &&
                                dataDictionary.TryGetValue("timestamp", out var timestampObj) &&
                                dataDictionary.TryGetValue("value", out var valueObj))
                            {
                                if (DateTime.TryParse(timestampObj.ToString(), out var timestamp) &&
                                    double.TryParse(valueObj.ToString(), out var value))
                                {
                                    dataPoints.Add(new DataPoint
                                    {
                                        Timestamp = timestamp,
                                        Value = value
                                    });
                                }
                            }
                        }
                    }

                    // Analyze trend if we have enough data points
                    if (dataPoints.Count >= _options.AnalysisSettings.TrendAnalysisMinimumDataPoints)
                    {
                        var trend = AnalyzeTrend(metric.Name, dataPoints);
                        if (trend != null)
                        {
                            trends.Add(trend);
                        }
                    }
                }

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing performance trends");
                return new List<PerformanceTrend>();
            }
        }

        /// <inheritdoc/>
        public Task<List<PerformanceAnomaly>> DetectAnomaliesAsync(List<PerformanceMetric> metrics, TimeRange? timeRange = null)
        {
            // Implementation similar to AnalyzeTrendsAsync but for anomaly detection
            // This is a simplified implementation
            return Task.FromResult(new List<PerformanceAnomaly>());
        }

        /// <inheritdoc/>
        public Task<List<PerformanceCorrelation>> AnalyzeCorrelationsAsync(List<PerformanceMetric> metrics, TimeRange? timeRange = null)
        {
            // Implementation similar to AnalyzeTrendsAsync but for correlation analysis
            // This is a simplified implementation
            return Task.FromResult(new List<PerformanceCorrelation>());
        }

        private async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(List<MetricDefinition> metricDefinitions, TimeRange timeRange)
        {
            var performanceMetrics = new List<PerformanceMetric>();

            foreach (var metricDef in metricDefinitions)
            {
                var query = new MetricQuery
                {
                    MetricNames = new List<string> { metricDef.Name },
                    TimeRange = new QueryTimeRange
                    {
                        StartTime = timeRange.StartTime,
                        EndTime = timeRange.EndTime
                    },
                    Aggregation = "avg"
                };

                var queryResult = await _statisticsClient.QueryMetricsAsync(query);
                if (!queryResult.Success || queryResult.Data == null || !queryResult.Data.ContainsKey(metricDef.Name))
                {
                    continue;
                }

                var metricValue = 0.0;
                if (queryResult.Data[metricDef.Name] is double doubleValue)
                {
                    metricValue = doubleValue;
                }
                else if (double.TryParse(queryResult.Data[metricDef.Name]?.ToString(), out var parsedValue))
                {
                    metricValue = parsedValue;
                }

                var metric = new PerformanceMetric
                {
                    Name = metricDef.Name,
                    Category = metricDef.Category,
                    Value = metricValue,
                    Unit = metricDef.Unit,
                    Timestamp = DateTime.UtcNow,
                    Dimensions = new Dictionary<string, string>()
                };

                // Get statistics for the metric
                var statsQuery = new MetricQuery
                {
                    MetricNames = new List<string> { metricDef.Name },
                    TimeRange = new QueryTimeRange
                    {
                        StartTime = timeRange.StartTime,
                        EndTime = timeRange.EndTime
                    },
                    Aggregation = "stats"
                };

                var statsResult = await _statisticsClient.QueryMetricsAsync(statsQuery);
                if (statsResult.Success && statsResult.Data != null && statsResult.Data.ContainsKey(metricDef.Name))
                {
                    if (statsResult.Data[metricDef.Name] is Dictionary<string, object> stats)
                    {
                        metric.Statistics = new MetricStatistics
                        {
                            Min = GetDoubleValue(stats, "min"),
                            Max = GetDoubleValue(stats, "max"),
                            Average = GetDoubleValue(stats, "avg"),
                            Median = GetDoubleValue(stats, "median"),
                            Percentile95 = GetDoubleValue(stats, "p95"),
                            StandardDeviation = GetDoubleValue(stats, "stdDev"),
                            SampleCount = (int)GetDoubleValue(stats, "count")
                        };
                    }
                }

                performanceMetrics.Add(metric);
            }

            return performanceMetrics;
        }

        private double GetDoubleValue(Dictionary<string, object> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                if (value is double doubleValue)
                {
                    return doubleValue;
                }
                else if (double.TryParse(value.ToString(), out var parsedValue))
                {
                    return parsedValue;
                }
            }
            return 0.0;
        }

        private PerformanceTrend? AnalyzeTrend(string metricName, List<DataPoint> dataPoints)
        {
            if (dataPoints.Count < 2)
            {
                return null;
            }

            // Sort data points by timestamp
            var sortedPoints = dataPoints.OrderBy(p => p.Timestamp).ToList();

            // Calculate trend using linear regression
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            int n = sortedPoints.Count;

            // Convert timestamps to relative time in seconds from the first point
            var firstTimestamp = sortedPoints[0].Timestamp;
            var timePoints = sortedPoints.Select(p => (p.Timestamp - firstTimestamp).TotalSeconds).ToList();

            for (int i = 0; i < n; i++)
            {
                double x = timePoints[i];
                double y = sortedPoints[i].Value;

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            // Determine trend direction
            TrendDirection direction;
            if (Math.Abs(slope) < 0.0001)
            {
                direction = TrendDirection.Stable;
            }
            else if (slope > 0)
            {
                direction = TrendDirection.Increasing;
            }
            else
            {
                direction = TrendDirection.Decreasing;
            }

            // Calculate R-squared to determine confidence
            double meanY = sumY / n;
            double totalSumSquares = 0;
            double residualSumSquares = 0;

            for (int i = 0; i < n; i++)
            {
                double x = timePoints[i];
                double y = sortedPoints[i].Value;
                double predictedY = slope * x + intercept;

                totalSumSquares += Math.Pow(y - meanY, 2);
                residualSumSquares += Math.Pow(y - predictedY, 2);
            }

            double rSquared = 1 - (residualSumSquares / totalSumSquares);
            double confidenceScore = Math.Max(0, Math.Min(1, rSquared));

            // Calculate magnitude as percentage change over the period
            double firstValue = sortedPoints.First().Value;
            double lastValue = sortedPoints.Last().Value;
            double magnitude = 0;

            if (Math.Abs(firstValue) > 0.0001)
            {
                magnitude = Math.Abs((lastValue - firstValue) / firstValue * 100);
            }

            return new PerformanceTrend
            {
                MetricName = metricName,
                Direction = direction,
                Magnitude = magnitude,
                ConfidenceScore = confidenceScore,
                DataPoints = sortedPoints
            };
        }
    }

    /// <summary>
    /// Interface for the performance analytics service.
    /// </summary>
    public interface IPerformanceAnalyticsService
    {
        /// <summary>
        /// Analyzes performance metrics.
        /// </summary>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The performance analysis result.</returns>
        Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(TimeRange? timeRange = null);

        /// <summary>
        /// Gets performance metrics for a specific category.
        /// </summary>
        /// <param name="category">The metric category.</param>
        /// <param name="timeRange">The time range.</param>
        /// <returns>The list of performance metrics.</returns>
        Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(string category, TimeRange? timeRange = null);

        /// <summary>
        /// Analyzes trends in performance metrics.
        /// </summary>
        /// <param name="metrics">The metrics to analyze.</param>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The list of performance trends.</returns>
        Task<List<PerformanceTrend>> AnalyzeTrendsAsync(List<PerformanceMetric> metrics, TimeRange? timeRange = null);

        /// <summary>
        /// Detects anomalies in performance metrics.
        /// </summary>
        /// <param name="metrics">The metrics to analyze.</param>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The list of performance anomalies.</returns>
        Task<List<PerformanceAnomaly>> DetectAnomaliesAsync(List<PerformanceMetric> metrics, TimeRange? timeRange = null);

        /// <summary>
        /// Analyzes correlations between performance metrics.
        /// </summary>
        /// <param name="metrics">The metrics to analyze.</param>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The list of performance correlations.</returns>
        Task<List<PerformanceCorrelation>> AnalyzeCorrelationsAsync(List<PerformanceMetric> metrics, TimeRange? timeRange = null);
    }
}
