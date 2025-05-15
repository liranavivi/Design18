# FlowOrchestrator.ImporterBase

This library provides the base implementation for all importer services in the FlowOrchestrator system. It includes the `AbstractImporterService` class and common importer functionality.

## Overview

The `FlowOrchestrator.ImporterBase` library is designed to:

1. Provide a standardized implementation pattern for importer services
2. Handle common functionality such as lifecycle management, error handling, and recovery
3. Support messaging through the `IMessageConsumer<ImportCommand>` interface
4. Provide configuration validation and management

## Key Components

### AbstractImporterService

The `AbstractImporterService` is an abstract base class that implements the `IImporterService` and `IMessageConsumer<ImportCommand>` interfaces. It provides:

- Service lifecycle management (Initialize, Terminate)
- State management (UNINITIALIZED, INITIALIZING, READY, PROCESSING, ERROR, TERMINATING, TERMINATED)
- Error handling and recovery mechanisms
- Metrics collection
- Lifecycle hooks for customization

### ImporterServiceBase

The `ImporterServiceBase` is a concrete implementation of `AbstractImporterService` that provides common functionality for all importer services, including:

- Logging
- Message consumption
- Basic configuration validation
- Exception classification
- Error detail extraction
- Recovery mechanisms
- Default lifecycle hook implementations

### ImporterServiceConfiguration

The `ImporterServiceConfiguration` class provides a standardized configuration model for importer services, including:

- Service identification (ServiceId, Protocol)
- Connection settings (ConnectionTimeoutSeconds, OperationTimeoutSeconds)
- Retry settings (MaxRetryCount, RetryDelayMilliseconds, UseExponentialBackoff)
- Import settings (BatchSize, ValidateData)
- Schema settings (SchemaId, SchemaVersion)
- Additional parameters

### ImporterServiceException

The `ImporterServiceException` class and its derived classes provide standardized exception handling for importer services, including:

- Error codes
- Error details
- Specialized exceptions for configuration and connection errors

## Usage

### Creating a Custom Importer Service

To create a custom importer service, inherit from `ImporterServiceBase` and implement the required abstract methods:

```csharp
using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Integration.Importers;

namespace MyNamespace
{
    public class MyCustomImporter : ImporterServiceBase
    {
        public override string ServiceId => "CUSTOM-IMPORTER-001";
        public override string Version => "1.0.0";
        public override string ServiceType => "CustomImporter";
        public override string Protocol => "CUSTOM";

        public MyCustomImporter(ILogger logger) : base(logger)
        {
        }

        public override ImporterCapabilities GetCapabilities()
        {
            return new ImporterCapabilities
            {
                SupportedSourceTypes = new List<string> { "CustomSource" },
                SupportedDataFormats = new List<string> { "JSON", "XML" },
                SupportsValidation = true,
                SupportsStreaming = false,
                SupportsBatching = true,
                SupportsFiltering = true,
                SupportsTransformation = false,
                MaxBatchSize = 1000
            };
        }

        protected override ImportResult PerformImport(ImportParameters parameters, ExecutionContext context)
        {
            // Custom import logic here
            var result = new ImportResult();
            
            // Retrieve data from the source
            var data = RetrieveDataFromSource(parameters);
            
            // Create a data package
            result.Data = new DataPackage
            {
                Content = data,
                ContentType = "application/json",
                SchemaId = parameters.SchemaId,
                Source = new SourceInformation
                {
                    SourceId = parameters.SourceId,
                    SourceType = "CustomSource",
                    SourceLocation = parameters.SourceLocation
                }
            };
            
            // Add metadata
            result.SourceMetadata = new Dictionary<string, object>
            {
                { "RecordCount", 100 },
                { "ImportTime", DateTime.UtcNow }
            };
            
            return result;
        }

        private object RetrieveDataFromSource(ImportParameters parameters)
        {
            // Custom logic to retrieve data from the source
            return new { Data = "Sample data" };
        }
    }
}
```

### Configuring an Importer Service

```csharp
var configuration = new ImporterServiceConfiguration
{
    ServiceId = "CUSTOM-IMPORTER-001",
    Protocol = "CUSTOM",
    ConnectionTimeoutSeconds = 60,
    OperationTimeoutSeconds = 120,
    MaxRetryCount = 3,
    RetryDelayMilliseconds = 1000,
    UseExponentialBackoff = true,
    BatchSize = 500,
    ValidateData = true,
    SchemaId = "CUSTOM-SCHEMA-001",
    SchemaVersion = "1.0.0"
};

var configParams = configuration.ToConfigurationParameters();
myImporter.Initialize(configParams);
```

### Handling Import Commands

The `ImporterServiceBase` class provides a default implementation of the `Consume` method that handles `ImportCommand` messages:

```csharp
// This is handled automatically by the ImporterServiceBase
public override async Task Consume(ConsumeContext<ImportCommand> context)
{
    try
    {
        // Verify that this command is intended for this service
        if (context.Message.ImporterServiceId != ServiceId || context.Message.ImporterServiceVersion != Version)
        {
            return;
        }

        // Execute the import operation
        var result = Import(context.Message.Parameters, context.Message.Context);

        // Create and publish the result
        var commandResult = new ImportCommandResult(context.Message, result);
        await PublishResult(commandResult, context.CorrelationId);
    }
    catch (Exception ex)
    {
        // Handle and publish errors
        // ...
    }
}
```

## Error Handling

The `ImporterServiceBase` class provides a standardized approach to error handling:

```csharp
// This is handled automatically by the ImporterServiceBase
protected override string ClassifyException(Exception ex)
{
    return ex switch
    {
        ArgumentException => "INVALID_ARGUMENT",
        InvalidOperationException => "INVALID_OPERATION",
        TimeoutException => "TIMEOUT",
        IOException => "IO_ERROR",
        UnauthorizedAccessException => "UNAUTHORIZED",
        NotImplementedException => "NOT_IMPLEMENTED",
        _ => "UNKNOWN_ERROR"
    };
}
```

## Metrics Collection

The `AbstractImporterService` class provides methods for collecting metrics:

```csharp
// This is handled automatically by the AbstractImporterService
protected virtual void StartOperation(string operationName)
{
    _metrics[$"{operationName}.startTime"] = DateTime.UtcNow;
    _metrics[$"{operationName}.inProgress"] = true;
}

protected virtual void EndOperation(string operationName, bool success)
{
    if (_metrics.TryGetValue($"{operationName}.startTime", out var startTimeObj) && startTimeObj is DateTime startTime)
    {
        TimeSpan duration = DateTime.UtcNow - startTime;
        _metrics[$"{operationName}.duration"] = duration.TotalMilliseconds;
    }

    _metrics[$"{operationName}.endTime"] = DateTime.UtcNow;
    _metrics[$"{operationName}.success"] = success;
    _metrics[$"{operationName}.inProgress"] = false;
}
```
