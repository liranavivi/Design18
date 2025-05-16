using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Management.Flows;
using FlowOrchestrator.Management.Flows.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
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
        Title = "FlowOrchestrator Flow Manager API",
        Version = "v1",
        Description = "API for managing flow definitions and configurations in the FlowOrchestrator system"
    });
});

// Configure MassTransit
builder.Services.AddMassTransit(x =>
{
    // Configure message consumers
    x.AddConsumer<FlowEntityManager>();
    x.AddConsumer<ProcessingChainManager>();

    // Configure the bus
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// Register flow managers
builder.Services.AddScoped<FlowEntityManager>();
builder.Services.AddScoped<ProcessingChainManager>();
builder.Services.AddSingleton<FlowValidationService>();
builder.Services.AddSingleton<FlowVersioningService>();

// Initialize flow managers with default configuration
builder.Services.AddHostedService<FlowManagerInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowOrchestrator Flow Manager API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Flow manager initializer
public class FlowManagerInitializer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FlowManagerInitializer> _logger;

    public FlowManagerInitializer(
        IServiceProvider serviceProvider,
        ILogger<FlowManagerInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Initializing flow managers...");

            // Create default configuration
            var defaultConfig = new ConfigurationParameters();
            defaultConfig.Parameters.Add("StorageType", "InMemory");
            defaultConfig.Parameters.Add("EnableValidation", true);
            defaultConfig.Parameters.Add("EnableVersioning", true);

            // Create a scope to resolve scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                // Resolve the managers from the scope
                var flowEntityManager = scope.ServiceProvider.GetRequiredService<FlowEntityManager>();
                var processingChainManager = scope.ServiceProvider.GetRequiredService<ProcessingChainManager>();

                // Initialize managers
                flowEntityManager.Initialize(defaultConfig);
                processingChainManager.Initialize(defaultConfig);
            }

            _logger.LogInformation("Flow managers initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize flow managers");
        }

        return Task.CompletedTask;
    }
}
