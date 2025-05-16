using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using FlowOrchestrator.Observability.Monitoring.HealthChecks;
using FlowOrchestrator.Observability.Monitoring.Models;
using FlowOrchestrator.Observability.Monitoring.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FlowOrchestrator Monitoring Framework API",
        Version = "v1",
        Description = "API for monitoring the FlowOrchestrator system",
        Contact = new OpenApiContact
        {
            Name = "FlowOrchestrator Team"
        }
    });

    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Configure options
builder.Services.Configure<MonitoringOptions>(
    builder.Configuration.GetSection("Monitoring"));

// Configure OpenTelemetry
var openTelemetryOptions = new OpenTelemetryOptions
{
    ServiceName = "FlowOrchestrator.MonitoringFramework",
    ServiceVersion = "1.0.0",
    EnableConsoleExporter = builder.Environment.IsDevelopment(),
    EnableOtlpExporter = true,
    OtlpEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint") ?? "http://localhost:4317",
    TracingSamplingRate = 1.0
};

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(openTelemetryOptions.ServiceName, serviceVersion: openTelemetryOptions.ServiceVersion)
    .AddTelemetrySdk()
    .AddEnvironmentVariableDetector();

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

// Register services
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();
builder.Services.AddSingleton<IResourceUtilizationMonitorService, ResourceUtilizationMonitorService>();
builder.Services.AddSingleton<IActiveFlowMonitorService, ActiveFlowMonitorService>();
builder.Services.AddSingleton<IPerformanceAnomalyDetectorService, PerformanceAnomalyDetectorService>();
builder.Services.AddSingleton<IAlertManagerService, AlertManagerService>();

// Configure health checks
builder.Services.AddHealthChecks()
    .AddCheck<MonitoringFrameworkHealthCheck>("MonitoringFramework")
    .AddCheck<ResourceUtilizationHealthCheck>("ResourceUtilization");

// Add HttpClient for service discovery
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowOrchestrator Monitoring Framework API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers
app.MapControllers();

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

app.MapHealthChecks("/health/ui", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
