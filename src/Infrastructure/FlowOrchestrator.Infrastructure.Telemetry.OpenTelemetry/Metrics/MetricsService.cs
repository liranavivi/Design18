using System.Diagnostics.Metrics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics
{
    /// <summary>
    /// Service for collecting and managing metrics.
    /// </summary>
    public class MetricsService
    {
        private readonly MeterProvider _meterProvider;
        private readonly ILogger<MetricsService> _logger;
        private readonly OpenTelemetryOptions _options;
        private readonly Dictionary<string, MetricDefinition> _metricDefinitions = new();
        private readonly Dictionary<string, object> _metricValues = new();
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsService"/> class.
        /// </summary>
        /// <param name="meterProvider">The meter provider.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The OpenTelemetry options.</param>
        public MetricsService(
            MeterProvider meterProvider,
            ILogger<MetricsService> logger,
            IOptions<OpenTelemetryOptions> options)
        {
            _meterProvider = meterProvider;
            _logger = logger;
            _options = options.Value;
            InitializeDefaultMetrics();
        }

        /// <summary>
        /// Records a counter metric.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="increment">The increment value.</param>
        /// <param name="tags">The metric tags.</param>
        public void RecordCounter(string name, long increment, Dictionary<string, string>? tags = null)
        {
            try
            {
                var counter = _meterProvider.GetCounter<long>(name);
                counter.Add(increment, CreateTagsObject(tags));

                // Update the metric value
                var currentValue = GetMetricValue(name);
                if (currentValue is long longValue)
                {
                    _metricValues[name] = longValue + increment;
                }
                else if (currentValue is int intValue)
                {
                    _metricValues[name] = intValue + increment;
                }
                else
                {
                    _metricValues[name] = increment;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording counter metric {MetricName}", name);
            }
        }

        /// <summary>
        /// Records a gauge metric.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The gauge value.</param>
        /// <param name="tags">The metric tags.</param>
        public void RecordGauge(string name, double value, Dictionary<string, string>? tags = null)
        {
            try
            {
                // For gauge metrics, we store the latest value
                _metricValues[name] = value;

                // We use observable gauges for reporting the current value
                if (!_metricDefinitions.ContainsKey(name))
                {
                    RegisterMetric(new MetricDefinition
                    {
                        Name = name,
                        Type = MetricType.Gauge,
                        Description = $"Gauge metric: {name}"
                    });

                    _meterProvider.GetObservableGauge(name, () => (double)_metricValues[name]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording gauge metric {MetricName}", name);
            }
        }

        /// <summary>
        /// Records a histogram metric.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The histogram value.</param>
        /// <param name="tags">The metric tags.</param>
        public void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null)
        {
            try
            {
                var histogram = _meterProvider.GetHistogram<double>(name);
                histogram.Record(value, CreateTagsObject(tags));
                _metricValues[name] = value; // Store the latest value
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording histogram metric {MetricName}", name);
            }
        }

        /// <summary>
        /// Starts tracking an operation for duration measurement.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        public void StartOperation(string operationName)
        {
            _operationStartTimes[operationName] = DateTime.UtcNow;
        }

        /// <summary>
        /// Ends tracking an operation and records its duration.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="result">The operation result.</param>
        /// <param name="tags">Additional tags to record.</param>
        public void EndOperation(string operationName, string result, Dictionary<string, string>? tags = null)
        {
            if (_operationStartTimes.TryGetValue(operationName, out var startTime))
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var metricName = $"{operationName}.duration";

                var allTags = tags ?? new Dictionary<string, string>();
                allTags["result"] = result;

                RecordHistogram(metricName, duration, allTags);
                RecordCounter($"{operationName}.count", 1, allTags);

                _operationStartTimes.Remove(operationName);
            }
        }

        /// <summary>
        /// Registers a metric definition.
        /// </summary>
        /// <param name="metricDefinition">The metric definition.</param>
        public void RegisterMetric(MetricDefinition metricDefinition)
        {
            _metricDefinitions[metricDefinition.Name] = metricDefinition;
        }

        /// <summary>
        /// Gets all registered metric definitions.
        /// </summary>
        /// <returns>The metric definitions.</returns>
        public IReadOnlyDictionary<string, MetricDefinition> GetMetricDefinitions()
        {
            return _metricDefinitions;
        }

        /// <summary>
        /// Gets the current value of a metric.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <returns>The metric value.</returns>
        public object GetMetricValue(string name)
        {
            return _metricValues.TryGetValue(name, out var value) ? value : 0;
        }

        /// <summary>
        /// Gets all current metric values.
        /// </summary>
        /// <returns>The metric values.</returns>
        public IReadOnlyDictionary<string, object> GetMetricValues()
        {
            return _metricValues;
        }

        private void InitializeDefaultMetrics()
        {
            // Register system metrics
            RegisterMetric(new MetricDefinition
            {
                Name = "system.memory.usage",
                Description = "Current memory usage",
                Unit = "bytes",
                Type = MetricType.Gauge
            });

            RegisterMetric(new MetricDefinition
            {
                Name = "system.cpu.usage",
                Description = "Current CPU usage",
                Unit = "percent",
                Type = MetricType.Gauge
            });

            // Register application metrics
            RegisterMetric(new MetricDefinition
            {
                Name = "application.requests.total",
                Description = "Total number of requests",
                Unit = "requests",
                Type = MetricType.Counter
            });

            RegisterMetric(new MetricDefinition
            {
                Name = "application.requests.duration",
                Description = "Request duration",
                Unit = "ms",
                Type = MetricType.Histogram,
                HistogramBoundaries = _options.DurationHistogramBoundaries
            });

            RegisterMetric(new MetricDefinition
            {
                Name = "application.errors.total",
                Description = "Total number of errors",
                Unit = "errors",
                Type = MetricType.Counter
            });
        }

        private static KeyValuePair<string, object?>[] CreateTagsObject(Dictionary<string, string>? tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return Array.Empty<KeyValuePair<string, object?>>();
            }

            return tags.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray();
        }
    }
}
