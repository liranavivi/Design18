using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry
{
    /// <summary>
    /// Implementation of IStatisticsProvider using OpenTelemetry.
    /// </summary>
    public class OpenTelemetryProvider : IStatisticsProvider
    {
        private readonly MetricsService _metricsService;
        private readonly ILogger<OpenTelemetryProvider> _logger;
        private readonly OpenTelemetryOptions _options;
        private readonly Dictionary<string, object> _statistics = new();
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTelemetryProvider"/> class.
        /// </summary>
        /// <param name="metricsService">The metrics service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The OpenTelemetry options.</param>
        public OpenTelemetryProvider(
            MetricsService metricsService,
            ILogger<OpenTelemetryProvider> logger,
            IOptions<OpenTelemetryOptions> options)
        {
            _metricsService = metricsService;
            _logger = logger;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public string ProviderId => $"OpenTelemetryProvider-{_options.ServiceInstanceId}";

        /// <inheritdoc/>
        public string ProviderType => "OpenTelemetry";

        /// <inheritdoc/>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>(_statistics);
        }

        /// <inheritdoc/>
        public object GetStatistic(string key)
        {
            return _statistics.TryGetValue(key, out var value) ? value : 0;
        }

        /// <inheritdoc/>
        public void ResetStatistics()
        {
            _statistics.Clear();
            _operationStartTimes.Clear();
        }

        /// <inheritdoc/>
        public void StartOperation(string operationName)
        {
            try
            {
                _operationStartTimes[operationName] = DateTime.UtcNow;
                _metricsService.StartOperation(operationName);
                
                // Record operation start count
                IncrementCounter($"{operationName}.start");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting operation {OperationName}", operationName);
            }
        }

        /// <inheritdoc/>
        public void EndOperation(string operationName, OperationResult result)
        {
            try
            {
                if (_operationStartTimes.TryGetValue(operationName, out var startTime))
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    
                    // Record duration
                    RecordDuration($"{operationName}.duration", (long)duration);
                    
                    // Record result
                    var resultString = TelemetryUtils.OperationResultToString(result);
                    var tags = new Dictionary<string, string> { ["result"] = resultString };
                    
                    _metricsService.EndOperation(operationName, resultString, tags);
                    
                    // Record operation end count
                    IncrementCounter($"{operationName}.end");
                    
                    // Record result-specific count
                    IncrementCounter($"{operationName}.{resultString}");
                    
                    _operationStartTimes.Remove(operationName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending operation {OperationName}", operationName);
            }
        }

        /// <inheritdoc/>
        public void RecordMetric(string metricName, object value)
        {
            try
            {
                _statistics[metricName] = value;
                
                if (value is double doubleValue)
                {
                    _metricsService.RecordGauge(metricName, doubleValue);
                }
                else if (value is int intValue)
                {
                    _metricsService.RecordGauge(metricName, intValue);
                }
                else if (value is long longValue)
                {
                    _metricsService.RecordGauge(metricName, longValue);
                }
                else
                {
                    _logger.LogWarning("Unsupported metric value type for {MetricName}: {ValueType}", 
                        metricName, value.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording metric {MetricName}", metricName);
            }
        }

        /// <inheritdoc/>
        public void IncrementCounter(string counterName, int increment = 1)
        {
            try
            {
                if (!_statistics.TryGetValue(counterName, out var currentValue))
                {
                    currentValue = 0;
                }
                
                if (currentValue is int intValue)
                {
                    _statistics[counterName] = intValue + increment;
                }
                else if (currentValue is long longValue)
                {
                    _statistics[counterName] = longValue + increment;
                }
                else
                {
                    _statistics[counterName] = increment;
                }
                
                _metricsService.RecordCounter(counterName, increment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter {CounterName}", counterName);
            }
        }

        /// <inheritdoc/>
        public void RecordDuration(string durationName, long milliseconds)
        {
            try
            {
                _statistics[durationName] = milliseconds;
                _metricsService.RecordHistogram(durationName, milliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording duration {DurationName}", durationName);
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, object> GetOperationStatistics()
        {
            return new Dictionary<string, object>(_statistics);
        }
    }
}
