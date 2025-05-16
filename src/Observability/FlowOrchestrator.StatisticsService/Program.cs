using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using FlowOrchestrator.Observability.Statistics.Models;
using FlowOrchestrator.Observability.Statistics.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure OpenAPI
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Configure options
builder.Services.Configure<StatisticsOptions>(
    builder.Configuration.GetSection("StatisticsService"));

// Register services
builder.Services.AddSingleton<HistoricalDataService>();
builder.Services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
builder.Services.AddSingleton<StatisticsConsumer>();
builder.Services.AddSingleton<IStatisticsConsumer>(provider => provider.GetRequiredService<StatisticsConsumer>());
builder.Services.AddSingleton<IStatisticsLifecycle, StatisticsLifecycle>();

// Configure OpenTelemetry
var openTelemetryOptions = builder.Configuration.GetSection("OpenTelemetry").Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

// Configure resource
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(openTelemetryOptions.ServiceName, openTelemetryOptions.ServiceVersion, openTelemetryOptions.ServiceInstanceId)
    .AddAttributes(new Dictionary<string, object>
    {
        ["flow_orchestrator.component_type"] = "StatisticsService",
        ["flow_orchestrator.instance_id"] = openTelemetryOptions.ServiceInstanceId
    });

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
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

// Configure health checks
builder.Services.AddHealthChecks()
    .AddCheck<StatisticsServiceHealthCheck>("StatisticsService");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                Duration = entry.Value.Duration
            })
        };

        await context.Response.WriteAsJsonAsync(result);
    }
});

// Map controllers
app.MapControllers();

// Initialize the statistics lifecycle
var statisticsLifecycle = app.Services.GetRequiredService<IStatisticsLifecycle>();
statisticsLifecycle.Initialize();
statisticsLifecycle.StartCollection();

app.Run();
