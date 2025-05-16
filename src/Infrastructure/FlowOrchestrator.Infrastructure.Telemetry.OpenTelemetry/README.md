# FlowOrchestrator OpenTelemetry Integration

This document provides instructions for integrating OpenTelemetry into FlowOrchestrator services using the recommended hybrid approach.

## Overview

The FlowOrchestrator OpenTelemetry integration provides:

- Distributed tracing
- Metrics collection
- Logging
- Statistics collection

The integration follows a hybrid approach:

- **Abstract interfaces** in service code for loose coupling and testability
- **Concrete implementations** in DI registration for specific functionality
- **Extension methods** for consistent configuration

## Integration Steps

### 1. Add NuGet Package References

Add the following package references to your project:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\Infrastructure\FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry\FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.csproj" />
</ItemGroup>
```

### 2. Update appsettings.json

Add the OpenTelemetry configuration section to your appsettings.json file:

```json
{
  "OpenTelemetry": {
    "ServiceName": "FlowOrchestrator.YourService",
    "ServiceVersion": "1.0.0",
    "ServiceInstanceId": "your-service-001",
    "ServiceNamespace": "FlowOrchestrator.YourNamespace",
    "OtlpEndpoint": "http://localhost:4317",
    "EnableConsoleExporter": true,
    "EnableOtlpExporter": true,
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "MetricsCollectionIntervalMs": 1000,
    "MetricsExportIntervalMs": 5000,
    "TracingSamplingRate": 1.0,
    "MaxAttributesPerSpan": 128,
    "MaxEventsPerSpan": 128
  }
}
```

### 3. Register OpenTelemetry Services

Update your Program.cs file to register the OpenTelemetry services:

```csharp
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Extensions;

// Add OpenTelemetry
builder.Services.AddFlowOrchestratorOpenTelemetry(builder.Configuration);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddFlowOrchestratorOpenTelemetryLogging(builder.Configuration);
});
```

### 4. Use Abstract Interfaces in Service Code

Inject the abstract interfaces into your services:

```csharp
using FlowOrchestrator.Abstractions.Statistics;

public class YourService
{
    private readonly IStatisticsProvider _statisticsProvider;
    
    public YourService(IStatisticsProvider statisticsProvider)
    {
        _statisticsProvider = statisticsProvider;
    }
    
    public void DoSomething()
    {
        _statisticsProvider.StartOperation("operation_name");
        try
        {
            // Business logic
            _statisticsProvider.IncrementCounter("operation_success");
            _statisticsProvider.EndOperation("operation_name", OperationResult.SUCCESS);
        }
        catch (Exception ex)
        {
            _statisticsProvider.IncrementCounter("operation_failure");
            _statisticsProvider.EndOperation("operation_name", OperationResult.FAILURE);
            throw;
        }
    }
}
```

### 5. Use LoggingService for Logging

Inject the LoggingService into your services:

```csharp
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Logging;

public class YourService
{
    private readonly LoggingService _loggingService;
    
    public YourService(LoggingService loggingService)
    {
        _loggingService = loggingService;
    }
    
    public void DoSomething()
    {
        _loggingService.LogInformation("Starting operation");
        
        try
        {
            // Business logic
            _loggingService.LogInformation("Operation completed successfully");
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Operation failed: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}
```

### 6. Use TelemetryMiddleware for Web APIs

Add the TelemetryMiddleware to your Web API pipeline:

```csharp
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Middleware;

// Configure the HTTP request pipeline
app.UseFlowOrchestratorTelemetry();
```

### 7. Extend AbstractServiceBase for Services

Create a service that extends AbstractServiceBase:

```csharp
using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Abstractions.Statistics;
using Microsoft.Extensions.Logging;

public class YourService : AbstractServiceBase
{
    public override string ServiceId => "YOUR-SERVICE-001";
    public override string Version => "1.0.0";
    public override string ServiceType => "YourService";
    
    public YourService(
        IStatisticsProvider statisticsProvider,
        ILogger<YourService> logger)
        : base(statisticsProvider, logger)
    {
    }
    
    protected override ValidationResult ValidateConfiguration(ConfigurationParameters parameters)
    {
        var result = new ValidationResult { IsValid = true };
        
        // Validate required parameters
        if (!parameters.HasParameter("RequiredParameter"))
        {
            result.IsValid = false;
            result.Errors.Add("RequiredParameter is required");
        }
        
        return result;
    }
    
    protected override void OnInitialize(ConfigurationParameters parameters)
    {
        // Initialize your service
    }
    
    protected override void OnReady()
    {
        // Called when the service is ready
    }
    
    protected override void OnError(Exception ex)
    {
        // Called when the service encounters an error
    }
    
    protected override void OnTerminate()
    {
        // Called when the service is being terminated
    }
}
```

## Best Practices

1. **Use Abstract Interfaces in Service Code**: Always depend on the abstract interfaces (`IStatisticsProvider`, etc.) rather than concrete implementations.

2. **Use Extension Methods for Registration**: Use the provided extension methods to register OpenTelemetry services in the DI container.

3. **Configure via appsettings.json**: Use configuration files for OpenTelemetry settings rather than hardcoding them.

4. **Centralize Telemetry Logic**: Keep telemetry-specific logic in the infrastructure layer, not in business services.

5. **Use Meaningful Operation Names**: Use consistent and meaningful names for operations, metrics, and counters.

6. **Add Context to Logs**: Include relevant context information in log messages.

7. **Handle Errors Gracefully**: Ensure that telemetry operations don't throw exceptions that could affect the main application flow.

## Troubleshooting

1. **No Telemetry Data**: Check that the OpenTelemetry configuration is correctly set up in appsettings.json.

2. **Missing Traces**: Ensure that operations are properly started and ended with `StartOperation` and `EndOperation`.

3. **Missing Metrics**: Verify that metrics are being recorded with `RecordMetric`, `IncrementCounter`, or `RecordDuration`.

4. **Logging Issues**: Check that the LoggingService is properly injected and used in your services.

## Example: Recording a Warning Message

```csharp
public void ProcessData(string data)
{
    if (string.IsNullOrEmpty(data))
    {
        _loggingService.LogWarning("Empty data received for processing");
        return;
    }
    
    // Process the data...
}
```

## Example: Recording Metrics

```csharp
public void ProcessData(string data)
{
    _statisticsProvider.StartOperation("process_data");
    
    try
    {
        // Process the data...
        _statisticsProvider.RecordMetric("data_size", data.Length);
        _statisticsProvider.IncrementCounter("data_processed");
        _statisticsProvider.EndOperation("process_data", OperationResult.SUCCESS);
    }
    catch (Exception ex)
    {
        _statisticsProvider.EndOperation("process_data", OperationResult.FAILURE);
        throw;
    }
}
```

## Example: Using TelemetryMiddleware

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Add telemetry middleware
    app.UseFlowOrchestratorTelemetry();
    
    // Other middleware...
    app.UseRouting();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```
