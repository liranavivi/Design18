using FlowOrchestrator.Management.Configuration.Services;
using MassTransit;
using Microsoft.OpenApi.Models;
using System.Reflection;
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
        Title = "FlowOrchestrator Configuration Manager API",
        Version = "v1",
        Description = "API for managing system and service configurations in the FlowOrchestrator system"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure MassTransit
builder.Services.AddMassTransit(x =>
{
    // Configure the bus
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// Register configuration services
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

// Initialize configuration service with default configuration
builder.Services.AddHostedService<ConfigurationManagerInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowOrchestrator Configuration Manager API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Configuration manager initializer
public class ConfigurationManagerInitializer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConfigurationManagerInitializer> _logger;

    public ConfigurationManagerInitializer(
        IServiceProvider serviceProvider,
        ILogger<ConfigurationManagerInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Initializing configuration manager...");

            // The ConfigurationService is already initialized with default schemas in its constructor
            _logger.LogInformation("Configuration manager initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize configuration manager");
        }

        return Task.CompletedTask;
    }
}
