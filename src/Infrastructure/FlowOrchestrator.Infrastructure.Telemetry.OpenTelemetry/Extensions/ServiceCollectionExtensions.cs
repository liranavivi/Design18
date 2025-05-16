using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Logging;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using MeterProviderImpl = FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics.MeterProvider;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Extensions
{
    /// <summary>
    /// Extension methods for registering OpenTelemetry services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OpenTelemetry services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddFlowOrchestratorOpenTelemetry(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure options
            services.Configure<OpenTelemetryOptions>(configuration.GetSection("OpenTelemetry"));

            // Register services
            services.AddSingleton<MeterProviderImpl>();
            services.AddSingleton<ActivitySourceProvider>();
            services.AddSingleton<MetricsService>();
            services.AddSingleton<TracingService>();
            services.AddSingleton<LoggingService>();
            services.AddSingleton<LoggerAdapterFactory>();
            services.AddSingleton<ILoggerFactory>(provider => provider.GetRequiredService<LoggerAdapterFactory>());

            // Register statistics implementations
            services.AddSingleton<IStatisticsProvider, OpenTelemetryProvider>();
            services.AddSingleton<IStatisticsConsumer, OpenTelemetryConsumer>();
            services.AddSingleton<IStatisticsLifecycle, OpenTelemetryLifecycle>();

            // Configure OpenTelemetry
            var openTelemetryOptions = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

            // Configure resource
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    openTelemetryOptions.ServiceName,
                    serviceVersion: openTelemetryOptions.ServiceVersion,
                    serviceInstanceId: openTelemetryOptions.ServiceInstanceId)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector();

            // Configure OpenTelemetry
            services.AddOpenTelemetry()
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddRuntimeInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    // Add meter provider
                    builder.AddMeter(openTelemetryOptions.ServiceName);

                    // Configure exporters
                    if (openTelemetryOptions.EnableConsoleExporter)
                    {
                        builder.AddConsoleExporter();
                    }

                    if (openTelemetryOptions.EnableOtlpExporter)
                    {
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(openTelemetryOptions.OtlpEndpoint);
                        });
                    }
                })
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    // Add activity source provider
                    builder.AddSource(openTelemetryOptions.ServiceName);

                    // Configure exporters
                    if (openTelemetryOptions.EnableConsoleExporter)
                    {
                        builder.AddConsoleExporter();
                    }

                    if (openTelemetryOptions.EnableOtlpExporter)
                    {
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(openTelemetryOptions.OtlpEndpoint);
                        });
                    }

                    // Configure sampling
                    builder.SetSampler(new TraceIdRatioBasedSampler(openTelemetryOptions.TracingSamplingRate));
                });

            return services;
        }

        /// <summary>
        /// Adds OpenTelemetry logger to the logging builder.
        /// </summary>
        /// <param name="builder">The logging builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The logging builder.</returns>
        public static ILoggingBuilder AddFlowOrchestratorOpenTelemetryLogging(
            this ILoggingBuilder builder,
            IConfiguration configuration)
        {
            // Configure options
            var openTelemetryOptions = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

            // Add OpenTelemetry logging
            if (openTelemetryOptions.EnableLogging)
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(openTelemetryOptions.ServiceName, openTelemetryOptions.ServiceVersion, openTelemetryOptions.ServiceInstanceId));

                    // Configure exporters
                    // Note: OpenTelemetry.Logs doesn't support direct exporter configuration in the same way as metrics and tracing
                    // Logging exporters are configured through the OpenTelemetry SDK configuration
                });
            }

            return builder;
        }
    }
}
