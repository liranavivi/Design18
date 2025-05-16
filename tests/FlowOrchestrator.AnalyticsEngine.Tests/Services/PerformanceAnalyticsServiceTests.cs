using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Observability.Analytics.Models;
using FlowOrchestrator.Observability.Analytics.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FlowOrchestrator.AnalyticsEngine.Tests.Services
{
    public class PerformanceAnalyticsServiceTests
    {
        private readonly Mock<ILogger<PerformanceAnalyticsService>> _loggerMock;
        private readonly Mock<IStatisticsServiceClient> _statisticsClientMock;
        private readonly IOptions<AnalyticsEngineOptions> _options;
        private readonly PerformanceAnalyticsService _service;

        public PerformanceAnalyticsServiceTests()
        {
            _loggerMock = new Mock<ILogger<PerformanceAnalyticsService>>();
            _statisticsClientMock = new Mock<IStatisticsServiceClient>();
            _options = Options.Create(new AnalyticsEngineOptions
            {
                AnalysisSettings = new AnalysisSettings
                {
                    DefaultTimeRangeHours = 24,
                    TrendAnalysisMinimumDataPoints = 5,
                    AnomalyDetectionSensitivity = 2.0,
                    PerformanceThresholds = new PerformanceThresholds
                    {
                        FlowExecutionTimeMs = 5000,
                        MemoryUsagePercent = 80,
                        CpuUsagePercent = 70,
                        DiskUsagePercent = 85
                    }
                }
            });

            _service = new PerformanceAnalyticsService(_loggerMock.Object, _statisticsClientMock.Object, _options);
        }

        [Fact]
        public async Task AnalyzePerformanceAsync_ShouldReturnAnalysisResult()
        {
            // Arrange
            var timeRange = new TimeRange
            {
                StartTime = DateTime.UtcNow.AddHours(-24),
                EndTime = DateTime.UtcNow
            };

            var metricDefinitions = new List<MetricDefinition>
            {
                new MetricDefinition
                {
                    Name = "system.cpu.usage",
                    Category = "System",
                    Unit = "Percent"
                },
                new MetricDefinition
                {
                    Name = "system.memory.usage",
                    Category = "System",
                    Unit = "Percent"
                }
            };

            var queryResult = new QueryResult
            {
                Success = true,
                Data = new Dictionary<string, object>
                {
                    { "system.cpu.usage", 65.5 },
                    { "system.memory.usage", 75.2 }
                }
            };

            var statsQueryResult = new QueryResult
            {
                Success = true,
                Data = new Dictionary<string, object>
                {
                    {
                        "system.cpu.usage", new Dictionary<string, object>
                        {
                            { "min", 50.0 },
                            { "max", 80.0 },
                            { "avg", 65.5 },
                            { "median", 65.0 },
                            { "p95", 78.0 },
                            { "stdDev", 5.0 },
                            { "count", 24 }
                        }
                    },
                    {
                        "system.memory.usage", new Dictionary<string, object>
                        {
                            { "min", 60.0 },
                            { "max", 85.0 },
                            { "avg", 75.2 },
                            { "median", 75.0 },
                            { "p95", 83.0 },
                            { "stdDev", 4.0 },
                            { "count", 24 }
                        }
                    }
                }
            };

            _statisticsClientMock.Setup(x => x.GetAvailableMetricsAsync())
                .ReturnsAsync(metricDefinitions);

            _statisticsClientMock.Setup(x => x.QueryMetricsAsync(It.IsAny<MetricQuery>()))
                .ReturnsAsync((MetricQuery q) =>
                {
                    if (q.Aggregation == "stats")
                    {
                        return statsQueryResult;
                    }
                    return queryResult;
                });

            // Act
            var result = await _service.AnalyzePerformanceAsync(timeRange);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(timeRange.StartTime, result.TimeRange.StartTime);
            Assert.Equal(timeRange.EndTime, result.TimeRange.EndTime);
            Assert.Equal(2, result.Metrics.Count);
            
            var cpuMetric = result.Metrics.Find(m => m.Name == "system.cpu.usage");
            Assert.NotNull(cpuMetric);
            Assert.Equal("System", cpuMetric.Category);
            Assert.Equal("Percent", cpuMetric.Unit);
            Assert.Equal(65.5, cpuMetric.Value);
            Assert.NotNull(cpuMetric.Statistics);
            Assert.Equal(50.0, cpuMetric.Statistics.Min);
            Assert.Equal(80.0, cpuMetric.Statistics.Max);
            Assert.Equal(65.5, cpuMetric.Statistics.Average);
            
            var memoryMetric = result.Metrics.Find(m => m.Name == "system.memory.usage");
            Assert.NotNull(memoryMetric);
            Assert.Equal("System", memoryMetric.Category);
            Assert.Equal("Percent", memoryMetric.Unit);
            Assert.Equal(75.2, memoryMetric.Value);
            Assert.NotNull(memoryMetric.Statistics);
            Assert.Equal(60.0, memoryMetric.Statistics.Min);
            Assert.Equal(85.0, memoryMetric.Statistics.Max);
            Assert.Equal(75.2, memoryMetric.Statistics.Average);
        }

        [Fact]
        public async Task GetPerformanceMetricsAsync_ShouldReturnMetricsForCategory()
        {
            // Arrange
            var category = "System";
            var timeRange = new TimeRange
            {
                StartTime = DateTime.UtcNow.AddHours(-24),
                EndTime = DateTime.UtcNow
            };

            var metricDefinitions = new List<MetricDefinition>
            {
                new MetricDefinition
                {
                    Name = "system.cpu.usage",
                    Category = "System",
                    Unit = "Percent"
                },
                new MetricDefinition
                {
                    Name = "system.memory.usage",
                    Category = "System",
                    Unit = "Percent"
                },
                new MetricDefinition
                {
                    Name = "flow.execution.time",
                    Category = "Flow",
                    Unit = "Milliseconds"
                }
            };

            var queryResult = new QueryResult
            {
                Success = true,
                Data = new Dictionary<string, object>
                {
                    { "system.cpu.usage", 65.5 },
                    { "system.memory.usage", 75.2 }
                }
            };

            var statsQueryResult = new QueryResult
            {
                Success = true,
                Data = new Dictionary<string, object>
                {
                    {
                        "system.cpu.usage", new Dictionary<string, object>
                        {
                            { "min", 50.0 },
                            { "max", 80.0 },
                            { "avg", 65.5 },
                            { "median", 65.0 },
                            { "p95", 78.0 },
                            { "stdDev", 5.0 },
                            { "count", 24 }
                        }
                    },
                    {
                        "system.memory.usage", new Dictionary<string, object>
                        {
                            { "min", 60.0 },
                            { "max", 85.0 },
                            { "avg", 75.2 },
                            { "median", 75.0 },
                            { "p95", 83.0 },
                            { "stdDev", 4.0 },
                            { "count", 24 }
                        }
                    }
                }
            };

            _statisticsClientMock.Setup(x => x.GetAvailableMetricsAsync())
                .ReturnsAsync(metricDefinitions);

            _statisticsClientMock.Setup(x => x.QueryMetricsAsync(It.IsAny<MetricQuery>()))
                .ReturnsAsync((MetricQuery q) =>
                {
                    if (q.Aggregation == "stats")
                    {
                        return statsQueryResult;
                    }
                    return queryResult;
                });

            // Act
            var result = await _service.GetPerformanceMetricsAsync(category, timeRange);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, metric => Assert.Equal(category, metric.Category));
            
            var cpuMetric = result.Find(m => m.Name == "system.cpu.usage");
            Assert.NotNull(cpuMetric);
            Assert.Equal("Percent", cpuMetric.Unit);
            Assert.Equal(65.5, cpuMetric.Value);
            
            var memoryMetric = result.Find(m => m.Name == "system.memory.usage");
            Assert.NotNull(memoryMetric);
            Assert.Equal("Percent", memoryMetric.Unit);
            Assert.Equal(75.2, memoryMetric.Value);
        }
    }
}
