using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Common.Utilities;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics;
using FlowOrchestrator.Observability.Statistics.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Statistics.Services
{
    /// <summary>
    /// Implementation of IStatisticsProvider for the statistics service.
    /// </summary>
    public class StatisticsProvider : IStatisticsProvider
    {
        private readonly ILogger<StatisticsProvider> _logger;
        private readonly MetricsService _metricsService;
        private readonly StatisticsOptions _options;
        private readonly Dictionary<string, object> _statistics = new();
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsProvider"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="metricsService">The metrics service.</param>
        /// <param name="options">The options.</param>
        public StatisticsProvider(
            ILogger<StatisticsProvider> logger,
            MetricsService metricsService,
            IOptions<StatisticsOptions> options)
        {
            _logger = logger;
            _metricsService = metricsService;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public string ProviderId => _options.ServiceId;

        /// <inheritdoc/>
        public string ProviderType => _options.ServiceType;

        /// <inheritdoc/>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>(_statistics);
        }

        /// <inheritdoc/>
        public object GetStatistic(string key)
        {
            Guard.AgainstNullOrEmpty(key, nameof(key));

            if (_statistics.TryGetValue(key, out var value))
            {
                return value;
            }

            return 0;
        }

        /// <inheritdoc/>
        public void ResetStatistics()
        {
            _statistics.Clear();
            _operationStartTimes.Clear();
            _logger.LogInformation("Statistics reset");
        }

        /// <inheritdoc/>
        public void StartOperation(string operationName)
        {
            Guard.AgainstNullOrEmpty(operationName, nameof(operationName));

            try
            {
                _operationStartTimes[operationName] = DateTime.UtcNow;
                IncrementCounter($"{operationName}.start");
                _logger.LogDebug("Started operation: {OperationName}", operationName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting operation {OperationName}", operationName);
            }
        }

        /// <inheritdoc/>
        public void EndOperation(string operationName, OperationResult result)
        {
            Guard.AgainstNullOrEmpty(operationName, nameof(operationName));

            try
            {
                if (_operationStartTimes.TryGetValue(operationName, out var startTime))
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    
                    // Record duration
                    RecordDuration($"{operationName}.duration", (long)duration);
                    
                    // Record result
                    var resultString = result.ToString().ToLowerInvariant();
                    
                    // Record operation end count
                    IncrementCounter($"{operationName}.end");
                    
                    // Record result-specific count
                    IncrementCounter($"{operationName}.{resultString}");
                    
                    _operationStartTimes.Remove(operationName);
                    
                    _logger.LogDebug("Ended operation: {OperationName}, Result: {Result}, Duration: {Duration}ms", 
                        operationName, resultString, duration);
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
            Guard.AgainstNullOrEmpty(metricName, nameof(metricName));

            try
            {
                _statistics[metricName] = value;
                
                if (value is long longValue)
                {
                    _metricsService.RecordCounter(metricName, longValue);
                }
                else if (value is int intValue)
                {
                    _metricsService.RecordCounter(metricName, intValue);
                }
                else if (value is double doubleValue)
                {
                    _metricsService.RecordHistogram(metricName, doubleValue);
                }
                else if (value is float floatValue)
                {
                    _metricsService.RecordHistogram(metricName, floatValue);
                }
                else
                {
                    _logger.LogWarning("Unsupported metric value type: {Type}", value.GetType().Name);
                }
                
                _logger.LogTrace("Recorded metric: {MetricName}, Value: {Value}", metricName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording metric {MetricName}", metricName);
            }
        }

        /// <inheritdoc/>
        public void IncrementCounter(string counterName, int increment = 1)
        {
            Guard.AgainstNullOrEmpty(counterName, nameof(counterName));

            try
            {
                if (_statistics.TryGetValue(counterName, out var value))
                {
                    if (value is int intValue)
                    {
                        _statistics[counterName] = intValue + increment;
                    }
                    else if (value is long longValue)
                    {
                        _statistics[counterName] = longValue + increment;
                    }
                    else
                    {
                        _statistics[counterName] = increment;
                    }
                }
                else
                {
                    _statistics[counterName] = increment;
                }
                
                _metricsService.RecordCounter(counterName, increment);
                _logger.LogTrace("Incremented counter: {CounterName}, Increment: {Increment}", counterName, increment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter {CounterName}", counterName);
            }
        }

        /// <inheritdoc/>
        public void RecordDuration(string durationName, long milliseconds)
        {
            Guard.AgainstNullOrEmpty(durationName, nameof(durationName));

            try
            {
                _statistics[durationName] = milliseconds;
                _metricsService.RecordHistogram(durationName, milliseconds);
                _logger.LogTrace("Recorded duration: {DurationName}, Milliseconds: {Milliseconds}", durationName, milliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording duration {DurationName}", durationName);
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, object> GetOperationStatistics()
        {
            var operationStats = new Dictionary<string, object>();
            
            foreach (var stat in _statistics)
            {
                if (stat.Key.Contains(".duration") || 
                    stat.Key.Contains(".start") || 
                    stat.Key.Contains(".end") || 
                    stat.Key.Contains(".success") || 
                    stat.Key.Contains(".failure") || 
                    stat.Key.Contains(".cancelled") || 
                    stat.Key.Contains(".timeout"))
                {
                    operationStats[stat.Key] = stat.Value;
                }
            }
            
            return operationStats;
        }
    }
}
