using System.Diagnostics.Metrics;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models
{
    /// <summary>
    /// Configuration options for OpenTelemetry integration.
    /// </summary>
    public class OpenTelemetryOptions
    {
        /// <summary>
        /// Gets or sets the service name.
        /// </summary>
        public string ServiceName { get; set; } = "FlowOrchestrator";

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        public string ServiceVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the service instance ID.
        /// </summary>
        public string ServiceInstanceId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the OTLP endpoint.
        /// </summary>
        public string OtlpEndpoint { get; set; } = "http://localhost:4317";

        /// <summary>
        /// Gets or sets a value indicating whether to enable console exporter.
        /// </summary>
        public bool EnableConsoleExporter { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to enable OTLP exporter.
        /// </summary>
        public bool EnableOtlpExporter { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable metrics.
        /// </summary>
        public bool EnableMetrics { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable tracing.
        /// </summary>
        public bool EnableTracing { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable logging.
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Gets or sets the metrics collection interval in milliseconds.
        /// </summary>
        public int MetricsCollectionIntervalMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the metrics export interval in milliseconds.
        /// </summary>
        public int MetricsExportIntervalMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the sampling rate for tracing (0.0 to 1.0).
        /// </summary>
        public double TracingSamplingRate { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the maximum number of attributes per span.
        /// </summary>
        public int MaxAttributesPerSpan { get; set; } = 128;

        /// <summary>
        /// Gets or sets the maximum number of events per span.
        /// </summary>
        public int MaxEventsPerSpan { get; set; } = 128;

        /// <summary>
        /// Gets or sets the maximum number of links per span.
        /// </summary>
        public int MaxLinksPerSpan { get; set; } = 128;

        /// <summary>
        /// Gets or sets the maximum number of attributes per event.
        /// </summary>
        public int MaxAttributesPerEvent { get; set; } = 128;

        /// <summary>
        /// Gets or sets the maximum number of attributes per link.
        /// </summary>
        public int MaxAttributesPerLink { get; set; } = 128;

        /// <summary>
        /// Gets or sets the histogram boundaries for duration metrics.
        /// </summary>
        public double[] DurationHistogramBoundaries { get; set; } = new double[]
        {
            1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000
        };
    }
}
