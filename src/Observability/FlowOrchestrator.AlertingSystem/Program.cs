using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Configuration;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Extensions;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Implementation;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry;
using FlowOrchestrator.Observability.Alerting;
using FlowOrchestrator.Observability.Alerting.Configuration;
using FlowOrchestrator.Observability.Alerting.Consumers;
using FlowOrchestrator.Observability.Alerting.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

// Configure options
builder.Services.Configure<AlertingSystemOptions>(
    builder.Configuration.GetSection("AlertingSystem"));

// Register services
builder.Services.AddSingleton<AlertRuleEngineService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<AlertHistoryService>();
builder.Services.AddSingleton<MetricsCollectorService>();

// Register HttpClient
builder.Services.AddHttpClient("NotificationService");
builder.Services.AddHttpClient("MetricsCollector");

// Register message consumers
builder.Services.AddSingleton<SystemEventConsumer>();
builder.Services.AddSingleton<MetricThresholdEventConsumer>();

// Configure MassTransit
builder.Services.AddFlowOrchestratorMessageBus(options =>
{
    options.TransportType = TransportType.InMemory;
    options.RetryCount = 3;
    options.RetryIntervalSeconds = 5;
});

// Register message bus
builder.Services.AddSingleton<IMessageBus, MassTransitMessageBus>();

// Configure OpenTelemetry
var openTelemetryOptions = builder.Configuration.GetSection("AlertingSystem:OpenTelemetry").Get<OpenTelemetryOptions>()
    ?? new OpenTelemetryOptions();

// Configure resource
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(openTelemetryOptions.ServiceName)
    .AddTelemetrySdk()
    .AddAttributes(new Dictionary<string, object>
    {
        ["service.namespace"] = openTelemetryOptions.ServiceNamespace,
        ["service.version"] = openTelemetryOptions.ServiceVersion
    });

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder
            .SetResourceBuilder(resourceBuilder)
            .AddRuntimeInstrumentation()
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

// Configure health checks
builder.Services.AddHealthChecks()
    .AddCheck("AlertingSystem", () => HealthCheckResult.Healthy("Alerting System is running"));

// Register worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
