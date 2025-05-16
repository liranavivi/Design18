using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry
{
    /// <summary>
    /// Implementation of IStatisticsConsumer using OpenTelemetry.
    /// </summary>
    public class OpenTelemetryConsumer : IStatisticsConsumer
    {
        private readonly MetricsService _metricsService;
        private readonly ILogger<OpenTelemetryConsumer> _logger;
        private readonly OpenTelemetryOptions _options;
        private readonly List<string> _supportedStatisticKeys = new();
        private readonly List<string> _supportedSourceTypes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTelemetryConsumer"/> class.
        /// </summary>
        /// <param name="metricsService">The metrics service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The OpenTelemetry options.</param>
        public OpenTelemetryConsumer(
            MetricsService metricsService,
            ILogger<OpenTelemetryConsumer> logger,
            IOptions<OpenTelemetryOptions> options)
        {
            _metricsService = metricsService;
            _logger = logger;
            _options = options.Value;
            
            InitializeSupportedKeys();
        }

        /// <inheritdoc/>
        public string ConsumerId => $"OpenTelemetryConsumer-{_options.ServiceInstanceId}";

        /// <inheritdoc/>
        public string ConsumerType => "OpenTelemetry";

        /// <inheritdoc/>
        public void ConsumeStatistics(Dictionary<string, object> statistics, StatisticsSource source)
        {
            try
            {
                var tags = new Dictionary<string, string>
                {
                    ["source.id"] = source.SourceId,
                    ["source.type"] = source.SourceType
                };
                
                foreach (var statistic in statistics)
                {
                    ConsumeStatistic(statistic.Key, statistic.Value, source);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming statistics from source {SourceId}", source.SourceId);
            }
        }

        /// <inheritdoc/>
        public void ConsumeStatistic(string key, object value, StatisticsSource source)
        {
            try
            {
                var tags = new Dictionary<string, string>
                {
                    ["source.id"] = source.SourceId,
                    ["source.type"] = source.SourceType
                };
                
                // Add any metadata as tags
                foreach (var metadata in source.Metadata)
                {
                    if (metadata.Value is string stringValue)
                    {
                        tags[metadata.Key] = stringValue;
                    }
                    else
                    {
                        tags[metadata.Key] = metadata.Value.ToString() ?? string.Empty;
                    }
                }
                
                // Record the statistic based on its type
                if (value is long longValue)
                {
                    if (key.EndsWith(".count") || key.EndsWith(".total"))
                    {
                        _metricsService.RecordCounter(key, longValue, tags);
                    }
                    else if (key.EndsWith(".duration") || key.EndsWith(".time"))
                    {
                        _metricsService.RecordHistogram(key, longValue, tags);
                    }
                    else
                    {
                        _metricsService.RecordGauge(key, longValue, tags);
                    }
                }
                else if (value is int intValue)
                {
                    if (key.EndsWith(".count") || key.EndsWith(".total"))
                    {
                        _metricsService.RecordCounter(key, intValue, tags);
                    }
                    else if (key.EndsWith(".duration") || key.EndsWith(".time"))
                    {
                        _metricsService.RecordHistogram(key, intValue, tags);
                    }
                    else
                    {
                        _metricsService.RecordGauge(key, intValue, tags);
                    }
                }
                else if (value is double doubleValue)
                {
                    if (key.EndsWith(".duration") || key.EndsWith(".time"))
                    {
                        _metricsService.RecordHistogram(key, doubleValue, tags);
                    }
                    else
                    {
                        _metricsService.RecordGauge(key, doubleValue, tags);
                    }
                }
                else if (value is float floatValue)
                {
                    if (key.EndsWith(".duration") || key.EndsWith(".time"))
                    {
                        _metricsService.RecordHistogram(key, floatValue, tags);
                    }
                    else
                    {
                        _metricsService.RecordGauge(key, floatValue, tags);
                    }
                }
                else
                {
                    _logger.LogWarning("Unsupported statistic value type for {StatisticKey}: {ValueType}",
                        key, value.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming statistic {StatisticKey} from source {SourceId}",
                    key, source.SourceId);
            }
        }

        /// <inheritdoc/>
        public List<string> GetSupportedStatisticKeys()
        {
            return _supportedStatisticKeys;
        }

        /// <inheritdoc/>
        public List<string> GetSupportedSourceTypes()
        {
            return _supportedSourceTypes;
        }

        private void InitializeSupportedKeys()
        {
            // Add supported statistic keys
            _supportedStatisticKeys.AddRange(new[]
            {
                // Operation statistics
                "operation.count",
                "operation.duration",
                "operation.success",
                "operation.failure",
                
                // Flow statistics
                "flow.execution.count",
                "flow.execution.duration",
                "flow.branch.count",
                "flow.branch.duration",
                "flow.step.count",
                "flow.step.duration",
                
                // Resource statistics
                "resource.memory.usage",
                "resource.cpu.usage",
                "resource.network.in",
                "resource.network.out",
                
                // Error statistics
                "error.count",
                "error.type"
            });
            
            // Add supported source types
            _supportedSourceTypes.AddRange(new[]
            {
                "ImporterService",
                "ProcessorService",
                "ExporterService",
                "OrchestratorService",
                "ManagerService",
                "BranchController",
                "MemoryManager",
                "RecoveryService"
            });
        }
    }
}
