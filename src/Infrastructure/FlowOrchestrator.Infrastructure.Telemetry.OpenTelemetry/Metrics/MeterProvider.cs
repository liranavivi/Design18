using System.Diagnostics.Metrics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics
{
    /// <summary>
    /// Provider for OpenTelemetry Meter instances.
    /// </summary>
    public class MeterProvider
    {
        private readonly Meter _meter;
        private readonly Dictionary<string, object> _instruments = new();
        private readonly OpenTelemetryOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeterProvider"/> class.
        /// </summary>
        /// <param name="options">The OpenTelemetry options.</param>
        public MeterProvider(IOptions<OpenTelemetryOptions> options)
        {
            _options = options.Value;
            _meter = new Meter(_options.ServiceName, _options.ServiceVersion);
        }

        /// <summary>
        /// Gets the meter instance.
        /// </summary>
        public Meter Meter => _meter;

        /// <summary>
        /// Creates or gets a counter instrument.
        /// </summary>
        /// <typeparam name="T">The counter value type.</typeparam>
        /// <param name="name">The counter name.</param>
        /// <param name="description">The counter description.</param>
        /// <param name="unit">The counter unit.</param>
        /// <returns>The counter instrument.</returns>
        public Counter<T> GetCounter<T>(string name, string description = "", string unit = "")
            where T : struct
        {
            string key = $"counter_{name}_{typeof(T).Name}";
            if (!_instruments.TryGetValue(key, out var instrument))
            {
                instrument = _meter.CreateCounter<T>(name, unit, description);
                _instruments[key] = instrument;
            }
            return (Counter<T>)instrument;
        }

        /// <summary>
        /// Creates or gets a histogram instrument.
        /// </summary>
        /// <typeparam name="T">The histogram value type.</typeparam>
        /// <param name="name">The histogram name.</param>
        /// <param name="description">The histogram description.</param>
        /// <param name="unit">The histogram unit.</param>
        /// <returns>The histogram instrument.</returns>
        public Histogram<T> GetHistogram<T>(string name, string description = "", string unit = "")
            where T : struct
        {
            string key = $"histogram_{name}_{typeof(T).Name}";
            if (!_instruments.TryGetValue(key, out var instrument))
            {
                instrument = _meter.CreateHistogram<T>(name, unit, description);
                _instruments[key] = instrument;
            }
            return (Histogram<T>)instrument;
        }

        /// <summary>
        /// Creates or gets an up-down counter instrument.
        /// </summary>
        /// <typeparam name="T">The up-down counter value type.</typeparam>
        /// <param name="name">The up-down counter name.</param>
        /// <param name="description">The up-down counter description.</param>
        /// <param name="unit">The up-down counter unit.</param>
        /// <returns>The up-down counter instrument.</returns>
        public UpDownCounter<T> GetUpDownCounter<T>(string name, string description = "", string unit = "")
            where T : struct
        {
            string key = $"updowncounter_{name}_{typeof(T).Name}";
            if (!_instruments.TryGetValue(key, out var instrument))
            {
                instrument = _meter.CreateUpDownCounter<T>(name, unit, description);
                _instruments[key] = instrument;
            }
            return (UpDownCounter<T>)instrument;
        }

        /// <summary>
        /// Creates or gets an observable gauge instrument.
        /// </summary>
        /// <typeparam name="T">The observable gauge value type.</typeparam>
        /// <param name="name">The observable gauge name.</param>
        /// <param name="observeValue">The function to observe the value.</param>
        /// <param name="description">The observable gauge description.</param>
        /// <param name="unit">The observable gauge unit.</param>
        /// <returns>The observable gauge instrument.</returns>
        public ObservableGauge<T> GetObservableGauge<T>(string name, Func<T> observeValue, string description = "", string unit = "")
            where T : struct
        {
            string key = $"observablegauge_{name}_{typeof(T).Name}";
            if (!_instruments.TryGetValue(key, out var instrument))
            {
                instrument = _meter.CreateObservableGauge(name, () => observeValue(), unit, description);
                _instruments[key] = instrument;
            }
            return (ObservableGauge<T>)instrument;
        }
    }
}
