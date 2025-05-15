using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Management.Services;
using MassTransit;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FlowOrchestrator Service Manager API",
        Version = "v1",
        Description = "API for managing services in the FlowOrchestrator system"
    });
});

// Configure MassTransit
builder.Services.AddMassTransit(x =>
{
    // Configure message consumers
    x.AddConsumer<ImporterServiceManager>();
    x.AddConsumer<ProcessorServiceManager>();
    x.AddConsumer<ExporterServiceManager>();

    // Configure the bus
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// Register service managers
builder.Services.AddSingleton<ImporterServiceManager>();
builder.Services.AddSingleton<ProcessorServiceManager>();
builder.Services.AddSingleton<ExporterServiceManager>();
builder.Services.AddSingleton<SourceEntityManager>();
builder.Services.AddSingleton<DestinationEntityManager>();
builder.Services.AddSingleton<SourceAssignmentEntityManager>();
builder.Services.AddSingleton<DestinationAssignmentEntityManager>();
builder.Services.AddSingleton<TaskSchedulerEntityManager>();
builder.Services.AddSingleton<ScheduledFlowEntityManager>();

// Initialize service managers with default configuration
builder.Services.AddHostedService<ServiceManagerInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowOrchestrator Service Manager API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Service manager initializer
public class ServiceManagerInitializer : BackgroundService
{
    private readonly ImporterServiceManager _importerServiceManager;
    private readonly ProcessorServiceManager _processorServiceManager;
    private readonly ExporterServiceManager _exporterServiceManager;
    private readonly SourceEntityManager _sourceEntityManager;
    private readonly DestinationEntityManager _destinationEntityManager;
    private readonly SourceAssignmentEntityManager _sourceAssignmentEntityManager;
    private readonly DestinationAssignmentEntityManager _destinationAssignmentEntityManager;
    private readonly TaskSchedulerEntityManager _taskSchedulerEntityManager;
    private readonly ScheduledFlowEntityManager _scheduledFlowEntityManager;
    private readonly ILogger<ServiceManagerInitializer> _logger;

    public ServiceManagerInitializer(
        ImporterServiceManager importerServiceManager,
        ProcessorServiceManager processorServiceManager,
        ExporterServiceManager exporterServiceManager,
        SourceEntityManager sourceEntityManager,
        DestinationEntityManager destinationEntityManager,
        SourceAssignmentEntityManager sourceAssignmentEntityManager,
        DestinationAssignmentEntityManager destinationAssignmentEntityManager,
        TaskSchedulerEntityManager taskSchedulerEntityManager,
        ScheduledFlowEntityManager scheduledFlowEntityManager,
        ILogger<ServiceManagerInitializer> logger)
    {
        _importerServiceManager = importerServiceManager;
        _processorServiceManager = processorServiceManager;
        _exporterServiceManager = exporterServiceManager;
        _sourceEntityManager = sourceEntityManager;
        _destinationEntityManager = destinationEntityManager;
        _sourceAssignmentEntityManager = sourceAssignmentEntityManager;
        _destinationAssignmentEntityManager = destinationAssignmentEntityManager;
        _taskSchedulerEntityManager = taskSchedulerEntityManager;
        _scheduledFlowEntityManager = scheduledFlowEntityManager;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Initialize service managers with default configuration
            var defaultConfig = new ConfigurationParameters();
            defaultConfig.SetParameter("ServiceName", "ServiceManager");
            defaultConfig.SetParameter("LogLevel", "Information");

            _importerServiceManager.Initialize(defaultConfig);
            _processorServiceManager.Initialize(defaultConfig);
            _exporterServiceManager.Initialize(defaultConfig);
            _sourceEntityManager.Initialize(defaultConfig);
            _destinationEntityManager.Initialize(defaultConfig);
            _sourceAssignmentEntityManager.Initialize(defaultConfig);
            _destinationAssignmentEntityManager.Initialize(defaultConfig);
            _taskSchedulerEntityManager.Initialize(defaultConfig);
            _scheduledFlowEntityManager.Initialize(defaultConfig);

            _logger.LogInformation("Service managers initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize service managers");
        }

        return Task.CompletedTask;
    }
}
