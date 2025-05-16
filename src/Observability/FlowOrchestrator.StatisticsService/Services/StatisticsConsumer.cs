using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Common.Utilities;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics;
using FlowOrchestrator.Observability.Statistics.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Statistics.Services
{
    /// <summary>
    /// Implementation of IStatisticsConsumer for the statistics service.
    /// </summary>
    public class StatisticsConsumer : IStatisticsConsumer
    {
        private readonly ILogger<StatisticsConsumer> _logger;
        private readonly MetricsService _metricsService;
        private readonly StatisticsOptions _options;
        private readonly Dictionary<string, MetricDefinition> _metricDefinitions = new();
        private readonly Dictionary<string, AlertDefinition> _alertDefinitions = new();
        private readonly Dictionary<string, List<(SubscriptionHandle Handle, NotificationHandler Handler)>> _subscriptions = new();
        private readonly List<string> _supportedStatisticKeys = new();
        private readonly List<string> _supportedSourceTypes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="metricsService">The metrics service.</param>
        /// <param name="options">The options.</param>
        public StatisticsConsumer(
            ILogger<StatisticsConsumer> logger,
            MetricsService metricsService,
            IOptions<StatisticsOptions> options)
        {
            _logger = logger;
            _metricsService = metricsService;
            _options = options.Value;
            
            InitializeSupportedKeys();
            InitializeSupportedSourceTypes();
        }

        /// <inheritdoc/>
        public string ConsumerId => _options.ServiceId;

        /// <inheritdoc/>
        public string ConsumerType => _options.ServiceType;

        /// <inheritdoc/>
        public void ConsumeStatistics(Dictionary<string, object> statistics, StatisticsSource source)
        {
            Guard.AgainstNull(statistics, nameof(statistics));
            Guard.AgainstNull(source, nameof(source));

            try
            {
                foreach (var statistic in statistics)
                {
                    ConsumeStatistic(statistic.Key, statistic.Value, source);
                }
                
                _logger.LogDebug("Consumed {Count} statistics from source {SourceId} of type {SourceType}", 
                    statistics.Count, source.SourceId, source.SourceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming statistics from source {SourceId}", source.SourceId);
            }
        }

        /// <inheritdoc/>
        public void ConsumeStatistic(string key, object value, StatisticsSource source)
        {
            Guard.AgainstNullOrEmpty(key, nameof(key));
            Guard.AgainstNull(source, nameof(source));

            try
            {
                // Create a metric record
                var metricRecord = new MetricRecord
                {
                    Name = key,
                    Value = value,
                    Timestamp = source.Timestamp,
                    ComponentId = source.SourceId,
                    ComponentType = source.SourceType,
                    ContextAttributes = new Dictionary<string, string>
                    {
                        ["source.id"] = source.SourceId,
                        ["source.type"] = source.SourceType
                    }
                };
                
                // Add any metadata from the source
                foreach (var metadata in source.Metadata)
                {
                    if (metadata.Value is string stringValue)
                    {
                        metricRecord.ContextAttributes[metadata.Key] = stringValue;
                    }
                    else
                    {
                        metricRecord.ContextAttributes[metadata.Key] = metadata.Value.ToString() ?? string.Empty;
                    }
                }
                
                // Process the metric
                ProcessMetric(metricRecord);
                
                _logger.LogTrace("Consumed statistic: {Key}, Value: {Value}, Source: {SourceId}", 
                    key, value, source.SourceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming statistic {Key} from source {SourceId}", key, source.SourceId);
            }
        }

        /// <inheritdoc/>
        public List<string> GetSupportedStatisticKeys()
        {
            return new List<string>(_supportedStatisticKeys);
        }

        /// <inheritdoc/>
        public List<string> GetSupportedSourceTypes()
        {
            return new List<string>(_supportedSourceTypes);
        }

        /// <summary>
        /// Queries metrics based on the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The query result.</returns>
        public QueryResult QueryMetrics(MetricQuery query)
        {
            Guard.AgainstNull(query, nameof(query));

            try
            {
                var result = new QueryResult
                {
                    Query = query,
                    Success = true,
                    ExecutionTimeMs = 0
                };
                
                var startTime = DateTime.UtcNow;
                
                // TODO: Implement actual query logic here
                // This would typically involve querying a database or in-memory store
                
                result.ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying metrics");
                return new QueryResult
                {
                    Query = query,
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionTimeMs = 0
                };
            }
        }

        /// <summary>
        /// Gets the available metrics.
        /// </summary>
        /// <returns>The available metrics.</returns>
        public List<MetricDefinition> GetAvailableMetrics()
        {
            return _metricDefinitions.Values.ToList();
        }

        /// <summary>
        /// Gets the configured alerts.
        /// </summary>
        /// <returns>The configured alerts.</returns>
        public List<AlertDefinition> GetConfiguredAlerts()
        {
            return _alertDefinitions.Values.ToList();
        }

        /// <summary>
        /// Subscribes to a metric.
        /// </summary>
        /// <param name="metricName">The metric name.</param>
        /// <param name="handler">The notification handler.</param>
        /// <returns>The subscription handle.</returns>
        public SubscriptionHandle SubscribeToMetric(string metricName, NotificationHandler handler)
        {
            Guard.AgainstNullOrEmpty(metricName, nameof(metricName));
            Guard.AgainstNull(handler, nameof(handler));

            try
            {
                var handle = new SubscriptionHandle(metricName, Guid.NewGuid().ToString());
                
                if (!_subscriptions.TryGetValue(metricName, out var handlers))
                {
                    handlers = new List<(SubscriptionHandle, NotificationHandler)>();
                    _subscriptions[metricName] = handlers;
                }
                
                handlers.Add((handle, handler));
                
                _logger.LogInformation("Subscribed to metric: {MetricName}, SubscriptionId: {SubscriptionId}", 
                    metricName, handle.SubscriptionId);
                
                return handle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to metric {MetricName}", metricName);
                throw;
            }
        }

        /// <summary>
        /// Unsubscribes from a metric.
        /// </summary>
        /// <param name="handle">The subscription handle.</param>
        public void UnsubscribeFromMetric(SubscriptionHandle handle)
        {
            Guard.AgainstNull(handle, nameof(handle));

            try
            {
                if (_subscriptions.TryGetValue(handle.MetricName, out var handlers))
                {
                    var subscription = handlers.FirstOrDefault(h => h.Handle.SubscriptionId == handle.SubscriptionId);
                    if (subscription != default)
                    {
                        handlers.Remove(subscription);
                        
                        if (handlers.Count == 0)
                        {
                            _subscriptions.Remove(handle.MetricName);
                        }
                        
                        _logger.LogInformation("Unsubscribed from metric: {MetricName}, SubscriptionId: {SubscriptionId}", 
                            handle.MetricName, handle.SubscriptionId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from metric {MetricName}, SubscriptionId: {SubscriptionId}", 
                    handle.MetricName, handle.SubscriptionId);
                throw;
            }
        }

        private void ProcessMetric(MetricRecord metricRecord)
        {
            // Register the metric definition if it doesn't exist
            if (!_metricDefinitions.ContainsKey(metricRecord.Name))
            {
                var metricDefinition = new MetricDefinition
                {
                    Name = metricRecord.Name,
                    Description = $"Metric: {metricRecord.Name}",
                    Unit = "count",
                    Type = DetermineMetricType(metricRecord.Name, metricRecord.Value)
                };
                
                _metricDefinitions[metricRecord.Name] = metricDefinition;
            }
            
            // Check for alerts
            CheckAlerts(metricRecord);
            
            // Notify subscribers
            NotifySubscribers(metricRecord);
        }

        private MetricType DetermineMetricType(string metricName, object value)
        {
            if (metricName.EndsWith(".duration") || metricName.EndsWith(".time") || metricName.EndsWith(".latency"))
            {
                return MetricType.Timer;
            }
            
            if (metricName.EndsWith(".rate") || metricName.EndsWith(".throughput"))
            {
                return MetricType.Meter;
            }
            
            if (metricName.EndsWith(".distribution") || metricName.EndsWith(".histogram"))
            {
                return MetricType.Histogram;
            }
            
            if (value is double || value is float)
            {
                return MetricType.Gauge;
            }
            
            return MetricType.Counter;
        }

        private void CheckAlerts(MetricRecord metricRecord)
        {
            foreach (var alert in _alertDefinitions.Values)
            {
                if (alert.Enabled && alert.MetricName == metricRecord.Name)
                {
                    // Check if the metric matches the filter attributes
                    bool matchesFilter = true;
                    foreach (var filter in alert.FilterAttributes)
                    {
                        if (!metricRecord.ContextAttributes.TryGetValue(filter.Key, out var value) || value != filter.Value)
                        {
                            matchesFilter = false;
                            break;
                        }
                    }
                    
                    if (matchesFilter && metricRecord.Value is IComparable comparable)
                    {
                        // Check if the threshold is exceeded
                        bool thresholdExceeded = false;
                        
                        if (comparable is double doubleValue)
                        {
                            thresholdExceeded = alert.Operator switch
                            {
                                ComparisonOperator.GreaterThan => doubleValue > alert.Threshold,
                                ComparisonOperator.GreaterThanOrEqual => doubleValue >= alert.Threshold,
                                ComparisonOperator.LessThan => doubleValue < alert.Threshold,
                                ComparisonOperator.LessThanOrEqual => doubleValue <= alert.Threshold,
                                ComparisonOperator.Equal => Math.Abs(doubleValue - alert.Threshold) < 0.0001,
                                ComparisonOperator.NotEqual => Math.Abs(doubleValue - alert.Threshold) >= 0.0001,
                                _ => false
                            };
                        }
                        else
                        {
                            var comparisonResult = comparable.CompareTo(alert.Threshold);
                            thresholdExceeded = alert.Operator switch
                            {
                                ComparisonOperator.GreaterThan => comparisonResult > 0,
                                ComparisonOperator.GreaterThanOrEqual => comparisonResult >= 0,
                                ComparisonOperator.LessThan => comparisonResult < 0,
                                ComparisonOperator.LessThanOrEqual => comparisonResult <= 0,
                                ComparisonOperator.Equal => comparisonResult == 0,
                                ComparisonOperator.NotEqual => comparisonResult != 0,
                                _ => false
                            };
                        }
                        
                        if (thresholdExceeded)
                        {
                            // TODO: Trigger alert notification
                            _logger.LogWarning("Alert triggered: {AlertName}, Metric: {MetricName}, Value: {Value}, Threshold: {Threshold}", 
                                alert.Name, metricRecord.Name, metricRecord.Value, alert.Threshold);
                        }
                    }
                }
            }
        }

        private void NotifySubscribers(MetricRecord metricRecord)
        {
            if (_subscriptions.TryGetValue(metricRecord.Name, out var handlers))
            {
                foreach (var (handle, handler) in handlers)
                {
                    try
                    {
                        // Check if the metric matches the filter attributes
                        bool matchesFilter = true;
                        foreach (var filter in handle.FilterAttributes)
                        {
                            if (!metricRecord.ContextAttributes.TryGetValue(filter.Key, out var value) || value != filter.Value)
                            {
                                matchesFilter = false;
                                break;
                            }
                        }
                        
                        if (matchesFilter)
                        {
                            handler(metricRecord.Name, metricRecord.Value, metricRecord.ContextAttributes);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error notifying subscriber for metric {MetricName}, SubscriptionId: {SubscriptionId}", 
                            metricRecord.Name, handle.SubscriptionId);
                    }
                }
            }
        }

        private void InitializeSupportedKeys()
        {
            _supportedStatisticKeys.AddRange(new[]
            {
                "flow.execution.count",
                "flow.execution.duration",
                "flow.execution.success",
                "flow.execution.failure",
                "branch.execution.count",
                "branch.execution.duration",
                "importer.operation.count",
                "importer.operation.duration",
                "processor.operation.count",
                "processor.operation.duration",
                "exporter.operation.count",
                "exporter.operation.duration",
                "memory.usage",
                "cpu.usage",
                "request.count",
                "request.duration",
                "error.count"
            });
        }

        private void InitializeSupportedSourceTypes()
        {
            _supportedSourceTypes.AddRange(new[]
            {
                "FlowOrchestrator",
                "Importer",
                "Processor",
                "Exporter",
                "Orchestrator",
                "BranchController",
                "MemoryManager",
                "ServiceManager",
                "FlowManager",
                "ConfigurationManager",
                "VersionManager",
                "TaskScheduler"
            });
        }
    }
}
