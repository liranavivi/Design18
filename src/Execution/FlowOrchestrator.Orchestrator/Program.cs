using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Extensions;
using FlowOrchestrator.Management.Scheduling.Messaging.Commands;
using FlowOrchestrator.Orchestrator.Messaging.Consumers;
using FlowOrchestrator.Orchestrator.Repositories;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FlowOrchestrator Orchestrator API",
        Version = "v1",
        Description = "Central coordination service for flow execution and management in the FlowOrchestrator system"
    });
});

// Register services
builder.Services.AddSingleton<OrchestratorService>();
builder.Services.AddSingleton<BranchManagementService>();
builder.Services.AddSingleton<MemoryAddressService>();
builder.Services.AddSingleton<MergeStrategyService>();
builder.Services.AddSingleton<ErrorHandlingService>();
builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddSingleton<FlowOrchestrator.Orchestrator.Repositories.IExecutionRepository, FlowOrchestrator.Orchestrator.Repositories.InMemoryExecutionRepository>();

// Configure MassTransit
builder.Services.AddSingleton<TriggerFlowCommandConsumer>();
builder.Services.AddSingleton<ProcessCommandResultConsumer>();
builder.Services.AddSingleton<ImportCommandResultConsumer>();
builder.Services.AddSingleton<ExportCommandResultConsumer>();

// Register message bus
builder.Services.AddSingleton<IMessageBus, FlowOrchestrator.Infrastructure.Messaging.MassTransit.Implementation.MassTransitMessageBus>();

// Register message consumers
builder.Services.AddSingleton<TriggerFlowCommandConsumer>();

// Initialize the orchestrator service
builder.Services.AddHostedService<OrchestratorInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Initializes the orchestrator service.
/// </summary>
public class OrchestratorInitializer : BackgroundService
{
    private readonly ILogger<OrchestratorInitializer> _logger;
    private readonly OrchestratorService _orchestratorService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrchestratorInitializer"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="orchestratorService">The orchestrator service.</param>
    public OrchestratorInitializer(
        ILogger<OrchestratorInitializer> logger,
        OrchestratorService orchestratorService)
    {
        _logger = logger;
        _orchestratorService = orchestratorService;
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Initializing orchestrator service");

            // Initialize the orchestrator service with default configuration
            var defaultConfig = new ConfigurationParameters();
            defaultConfig.SetParameter("ServiceId", "ORCHESTRATOR-SERVICE");
            defaultConfig.SetParameter("MaxConcurrentExecutions", 100);
            _orchestratorService.Initialize(defaultConfig);

            _logger.LogInformation("Orchestrator service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize orchestrator service");
        }

        return Task.CompletedTask;
    }
}
