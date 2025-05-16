using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlowOrchestrator.Observability.Analytics.Models;
using FlowOrchestrator.Observability.Analytics.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure OpenAPI
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Configure options
builder.Services.Configure<AnalyticsEngineOptions>(
    builder.Configuration.GetSection("AnalyticsEngine"));

// Configure HTTP clients
builder.Services.AddHttpClient<IStatisticsServiceClient, StatisticsServiceClient>()
    .AddPolicyHandler(GetRetryPolicy());

// Register services
builder.Services.AddSingleton<IPerformanceAnalyticsService, PerformanceAnalyticsService>();
builder.Services.AddSingleton<IUsagePatternService, UsagePatternService>();
builder.Services.AddSingleton<IOptimizationRecommendationService, OptimizationRecommendationService>();

// Configure health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck<StatisticsServiceHealthCheck>("statistics-service");

// Configure OpenTelemetry
var openTelemetryOptions = builder.Configuration.GetSection("OpenTelemetry").Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(openTelemetryOptions.ServiceName, serviceInstanceId: openTelemetryOptions.ServiceInstanceId))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Configure health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => true
});

app.MapControllers();

app.Run();

// Helper methods
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// Health check for Statistics Service
public class StatisticsServiceHealthCheck : IHealthCheck
{
    private readonly IStatisticsServiceClient _statisticsClient;
    private readonly ILogger<StatisticsServiceHealthCheck> _logger;

    public StatisticsServiceHealthCheck(
        IStatisticsServiceClient statisticsClient,
        ILogger<StatisticsServiceHealthCheck> logger)
    {
        _statisticsClient = statisticsClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var providers = await _statisticsClient.GetStatisticsProvidersAsync();
            return providers != null
                ? HealthCheckResult.Healthy("Statistics Service is available")
                : HealthCheckResult.Degraded("Statistics Service returned no providers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for Statistics Service");
            return HealthCheckResult.Unhealthy("Statistics Service is unavailable", ex);
        }
    }
}

// OpenTelemetry options
public class OpenTelemetryOptions
{
    public string ServiceName { get; set; } = "FlowOrchestrator.AnalyticsEngine";
    public string ServiceInstanceId { get; set; } = "analytics-engine-01";
    public bool EnableConsoleExporter { get; set; } = true;
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
}
