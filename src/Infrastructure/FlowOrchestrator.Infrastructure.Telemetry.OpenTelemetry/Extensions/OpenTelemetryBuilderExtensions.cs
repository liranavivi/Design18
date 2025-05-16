using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Extensions
{
    /// <summary>
    /// Extension methods for configuring OpenTelemetry builders.
    /// </summary>
    public static class OpenTelemetryBuilderExtensions
    {
        /// <summary>
        /// Configures the resource builder with FlowOrchestrator-specific attributes.
        /// </summary>
        /// <param name="builder">The resource builder.</param>
        /// <param name="options">The OpenTelemetry options.</param>
        /// <returns>The resource builder.</returns>
        public static ResourceBuilder AddFlowOrchestratorAttributes(
            this ResourceBuilder builder,
            OpenTelemetryOptions options)
        {
            return builder
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("flow_orchestrator.version", options.ServiceVersion),
                    new KeyValuePair<string, object>("flow_orchestrator.instance_id", options.ServiceInstanceId)
                });
        }

        /// <summary>
        /// Configures the metrics provider with FlowOrchestrator-specific metrics.
        /// </summary>
        /// <param name="builder">The metrics provider builder.</param>
        /// <param name="options">The OpenTelemetry options.</param>
        /// <returns>The metrics provider builder.</returns>
        public static MeterProviderBuilder AddFlowOrchestratorMetrics(
            this MeterProviderBuilder builder,
            OpenTelemetryOptions options)
        {
            return builder
                .AddMeter(options.ServiceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(options.ServiceName, options.ServiceVersion, options.ServiceInstanceId)
                    .AddFlowOrchestratorAttributes(options));
        }

        /// <summary>
        /// Configures the trace provider with FlowOrchestrator-specific tracing.
        /// </summary>
        /// <param name="builder">The trace provider builder.</param>
        /// <param name="options">The OpenTelemetry options.</param>
        /// <returns>The trace provider builder.</returns>
        public static TracerProviderBuilder AddFlowOrchestratorTracing(
            this TracerProviderBuilder builder,
            OpenTelemetryOptions options)
        {
            return builder
                .AddSource(options.ServiceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(options.ServiceName, options.ServiceVersion, options.ServiceInstanceId)
                    .AddFlowOrchestratorAttributes(options))
                .SetSampler(new TraceIdRatioBasedSampler(options.TracingSamplingRate));
        }
    }
}
