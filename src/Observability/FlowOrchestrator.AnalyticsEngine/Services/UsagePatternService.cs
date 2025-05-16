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
    /// Service for analyzing usage patterns.
    /// </summary>
    public class UsagePatternService : IUsagePatternService
    {
        private readonly ILogger<UsagePatternService> _logger;
        private readonly IStatisticsServiceClient _statisticsClient;
        private readonly AnalyticsEngineOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsagePatternService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="statisticsClient">The statistics client.</param>
        /// <param name="options">The options.</param>
        public UsagePatternService(
            ILogger<UsagePatternService> logger,
            IStatisticsServiceClient statisticsClient,
            IOptions<AnalyticsEngineOptions> options)
        {
            _logger = logger;
            _statisticsClient = statisticsClient;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public async Task<UsagePatternAnalysisResult> AnalyzeUsagePatternsAsync(TimeRange? timeRange = null)
        {
            var analysisTimeRange = timeRange ?? new TimeRange
            {
                StartTime = DateTime.UtcNow.AddHours(-_options.AnalysisSettings.DefaultTimeRangeHours),
                EndTime = DateTime.UtcNow
            };

            _logger.LogInformation("Starting usage pattern analysis for time range: {StartTime} to {EndTime}",
                analysisTimeRange.StartTime, analysisTimeRange.EndTime);

            var result = new UsagePatternAnalysisResult
            {
                TimeRange = analysisTimeRange
            };

            try
            {
                // Analyze flow usage patterns
                result.FlowUsagePatterns = await AnalyzeFlowUsagePatternsAsync(analysisTimeRange);
                _logger.LogDebug("Analyzed {Count} flow usage patterns", result.FlowUsagePatterns.Count);

                // Analyze component usage patterns
                result.ComponentUsagePatterns = await AnalyzeComponentUsagePatternsAsync(analysisTimeRange);
                _logger.LogDebug("Analyzed {Count} component usage patterns", result.ComponentUsagePatterns.Count);

                // Analyze resource usage patterns
                result.ResourceUsagePatterns = await AnalyzeResourceUsagePatternsAsync(analysisTimeRange);
                _logger.LogDebug("Analyzed {Count} resource usage patterns", result.ResourceUsagePatterns.Count);

                // Analyze temporal patterns
                result.TemporalPatterns = await AnalyzeTemporalPatternsAsync(analysisTimeRange);
                _logger.LogDebug("Analyzed {Count} temporal patterns", result.TemporalPatterns.Count);

                _logger.LogInformation("Usage pattern analysis completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing usage pattern analysis");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<FlowUsagePattern>> AnalyzeFlowUsagePatternsAsync(TimeRange? timeRange = null)
        {
            var analysisTimeRange = timeRange ?? new TimeRange
            {
                StartTime = DateTime.UtcNow.AddHours(-_options.AnalysisSettings.DefaultTimeRangeHours),
                EndTime = DateTime.UtcNow
            };

            try
            {
                var flowUsagePatterns = new List<FlowUsagePattern>();

                // Query flow execution metrics
                var query = new MetricQuery
                {
                    MetricNames = new List<string> { "flow.execution.count", "flow.execution.duration", "flow.execution.success", "flow.data.volume" },
                    TimeRange = new QueryTimeRange
                    {
                        StartTime = analysisTimeRange.StartTime,
                        EndTime = analysisTimeRange.EndTime
                    },
                    GroupBy = new List<string> { "flowId", "flowName" },
                    Aggregation = "sum"
                };

                var queryResult = await _statisticsClient.QueryMetricsAsync(query);
                if (!queryResult.Success || queryResult.Data == null)
                {
                    return flowUsagePatterns;
                }

                // Process query results
                if (queryResult.GroupedData != null)
                {
                    foreach (var group in queryResult.GroupedData)
                    {
                        if (group.Key.Split('|').Length < 2)
                        {
                            continue;
                        }

                        var keyParts = group.Key.Split('|');
                        var flowId = keyParts[0];
                        var flowName = keyParts[1];

                        var executionCount = 0;
                        var executionDuration = 0.0;
                        var successCount = 0;
                        var dataVolume = 0L;

                        if (group.Value.TryGetValue("flow.execution.count", out var countObj) &&
                            int.TryParse(countObj.ToString(), out var count))
                        {
                            executionCount = count;
                        }

                        if (group.Value.TryGetValue("flow.execution.duration", out var durationObj) &&
                            double.TryParse(durationObj.ToString(), out var duration))
                        {
                            executionDuration = duration;
                        }

                        if (group.Value.TryGetValue("flow.execution.success", out var successObj) &&
                            int.TryParse(successObj.ToString(), out var success))
                        {
                            successCount = success;
                        }

                        if (group.Value.TryGetValue("flow.data.volume", out var volumeObj) &&
                            long.TryParse(volumeObj.ToString(), out var volume))
                        {
                            dataVolume = volume;
                        }

                        var successRate = executionCount > 0 ? (double)successCount / executionCount : 0;
                        var avgExecutionTime = executionCount > 0 ? executionDuration / executionCount : 0;
                        var avgDataVolume = executionCount > 0 ? dataVolume / executionCount : 0;

                        // Get error patterns for this flow
                        var errorPatterns = await GetFlowErrorPatternsAsync(flowId, analysisTimeRange);

                        // Determine execution frequency pattern
                        var frequencyPattern = await DetermineExecutionFrequencyPatternAsync(flowId, analysisTimeRange);

                        flowUsagePatterns.Add(new FlowUsagePattern
                        {
                            FlowId = flowId,
                            FlowName = flowName,
                            ExecutionCount = executionCount,
                            AverageExecutionTimeMs = avgExecutionTime,
                            SuccessRate = successRate,
                            AverageDataVolumeBytes = avgDataVolume,
                            ExecutionFrequencyPattern = frequencyPattern,
                            CommonErrorPatterns = errorPatterns
                        });
                    }
                }

                return flowUsagePatterns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing flow usage patterns");
                return new List<FlowUsagePattern>();
            }
        }

        /// <inheritdoc/>
        public Task<List<ComponentUsagePattern>> AnalyzeComponentUsagePatternsAsync(TimeRange? timeRange = null)
        {
            // Implementation similar to AnalyzeFlowUsagePatternsAsync but for components
            // This is a simplified implementation
            return Task.FromResult(new List<ComponentUsagePattern>());
        }

        /// <inheritdoc/>
        public Task<List<ResourceUsagePattern>> AnalyzeResourceUsagePatternsAsync(TimeRange? timeRange = null)
        {
            // Implementation similar to AnalyzeFlowUsagePatternsAsync but for resources
            // This is a simplified implementation
            return Task.FromResult(new List<ResourceUsagePattern>());
        }

        /// <inheritdoc/>
        public Task<List<TemporalPattern>> AnalyzeTemporalPatternsAsync(TimeRange? timeRange = null)
        {
            // Implementation for analyzing temporal patterns
            // This is a simplified implementation
            return Task.FromResult(new List<TemporalPattern>());
        }

        private async Task<List<ErrorPattern>> GetFlowErrorPatternsAsync(string flowId, TimeRange timeRange)
        {
            try
            {
                var errorPatterns = new List<ErrorPattern>();

                // Query error metrics for the flow
                var query = new MetricQuery
                {
                    MetricNames = new List<string> { "flow.error.count" },
                    TimeRange = new QueryTimeRange
                    {
                        StartTime = timeRange.StartTime,
                        EndTime = timeRange.EndTime
                    },
                    Filter = new Dictionary<string, string> { { "flowId", flowId } },
                    GroupBy = new List<string> { "errorType", "errorMessage" },
                    Aggregation = "sum"
                };

                var queryResult = await _statisticsClient.QueryMetricsAsync(query);
                if (!queryResult.Success || queryResult.Data == null || queryResult.GroupedData == null)
                {
                    return errorPatterns;
                }

                // Get total error count
                int totalErrorCount = 0;
                if (queryResult.Data.TryGetValue("flow.error.count", out var totalObj) &&
                    int.TryParse(totalObj.ToString(), out var total))
                {
                    totalErrorCount = total;
                }

                // Process grouped error data
                foreach (var group in queryResult.GroupedData)
                {
                    if (group.Key.Split('|').Length < 2)
                    {
                        continue;
                    }

                    var keyParts = group.Key.Split('|');
                    var errorType = keyParts[0];
                    var errorMessage = keyParts[1];

                    int errorCount = 0;
                    if (group.Value.TryGetValue("flow.error.count", out var countObj) &&
                        int.TryParse(countObj.ToString(), out var count))
                    {
                        errorCount = count;
                    }

                    double frequency = totalErrorCount > 0 ? (double)errorCount / totalErrorCount * 100 : 0;

                    errorPatterns.Add(new ErrorPattern
                    {
                        ErrorType = errorType,
                        ErrorMessagePattern = errorMessage,
                        OccurrenceCount = errorCount,
                        FrequencyPercentage = frequency,
                        CommonContextAttributes = new Dictionary<string, string> { { "flowId", flowId } }
                    });
                }

                return errorPatterns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting error patterns for flow {FlowId}", flowId);
                return new List<ErrorPattern>();
            }
        }

        private async Task<string> DetermineExecutionFrequencyPatternAsync(string flowId, TimeRange timeRange)
        {
            try
            {
                // Query hourly execution counts
                var query = new MetricQuery
                {
                    MetricNames = new List<string> { "flow.execution.count" },
                    TimeRange = new QueryTimeRange
                    {
                        StartTime = timeRange.StartTime,
                        EndTime = timeRange.EndTime
                    },
                    Filter = new Dictionary<string, string> { { "flowId", flowId } },
                    GroupBy = new List<string> { "hour" },
                    Aggregation = "sum"
                };

                var queryResult = await _statisticsClient.QueryMetricsAsync(query);
                if (!queryResult.Success || queryResult.GroupedData == null)
                {
                    return "Unknown";
                }

                // Analyze hourly distribution
                var hourlyDistribution = new Dictionary<int, int>();
                foreach (var group in queryResult.GroupedData)
                {
                    if (int.TryParse(group.Key, out var hour) &&
                        group.Value.TryGetValue("flow.execution.count", out var countObj) &&
                        int.TryParse(countObj.ToString(), out var count))
                    {
                        hourlyDistribution[hour] = count;
                    }
                }

                // Determine pattern based on distribution
                if (hourlyDistribution.Count == 0)
                {
                    return "Infrequent";
                }

                var totalExecutions = hourlyDistribution.Values.Sum();
                var avgExecutionsPerHour = totalExecutions / 24.0;
                var peakHour = hourlyDistribution.OrderByDescending(kv => kv.Value).First().Key;
                var peakExecutions = hourlyDistribution[peakHour];

                if (peakExecutions > avgExecutionsPerHour * 3)
                {
                    return $"Peak at {peakHour:D2}:00";
                }
                else if (hourlyDistribution.Values.Max() - hourlyDistribution.Values.Min() < avgExecutionsPerHour * 0.5)
                {
                    return "Uniform";
                }
                else if (hourlyDistribution.Where(kv => kv.Key >= 9 && kv.Key <= 17).Sum(kv => kv.Value) > totalExecutions * 0.7)
                {
                    return "Business Hours";
                }
                else if (hourlyDistribution.Where(kv => kv.Key >= 0 && kv.Key <= 8).Sum(kv => kv.Value) > totalExecutions * 0.5)
                {
                    return "Overnight";
                }
                else
                {
                    return "Variable";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining execution frequency pattern for flow {FlowId}", flowId);
                return "Unknown";
            }
        }
    }

    /// <summary>
    /// Interface for the usage pattern service.
    /// </summary>
    public interface IUsagePatternService
    {
        /// <summary>
        /// Analyzes usage patterns.
        /// </summary>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The usage pattern analysis result.</returns>
        Task<UsagePatternAnalysisResult> AnalyzeUsagePatternsAsync(TimeRange? timeRange = null);

        /// <summary>
        /// Analyzes flow usage patterns.
        /// </summary>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The list of flow usage patterns.</returns>
        Task<List<FlowUsagePattern>> AnalyzeFlowUsagePatternsAsync(TimeRange? timeRange = null);

        /// <summary>
        /// Analyzes component usage patterns.
        /// </summary>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The list of component usage patterns.</returns>
        Task<List<ComponentUsagePattern>> AnalyzeComponentUsagePatternsAsync(TimeRange? timeRange = null);

        /// <summary>
        /// Analyzes resource usage patterns.
        /// </summary>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The list of resource usage patterns.</returns>
        Task<List<ResourceUsagePattern>> AnalyzeResourceUsagePatternsAsync(TimeRange? timeRange = null);

        /// <summary>
        /// Analyzes temporal patterns.
        /// </summary>
        /// <param name="timeRange">The time range for analysis.</param>
        /// <returns>The list of temporal patterns.</returns>
        Task<List<TemporalPattern>> AnalyzeTemporalPatternsAsync(TimeRange? timeRange = null);
    }
}
