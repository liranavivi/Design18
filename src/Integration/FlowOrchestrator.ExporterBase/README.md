# FlowOrchestrator.ExporterBase

This library provides the base implementation for all exporter services in the FlowOrchestrator system. It includes the `AbstractExporterService` class and common exporter functionality.

## Overview

The `FlowOrchestrator.ExporterBase` library is designed to:

1. Provide a standardized implementation pattern for exporter services
2. Handle common functionality such as lifecycle management, error handling, and recovery
3. Support messaging through the `IMessageConsumer<ExportCommand>` interface
4. Provide configuration validation and management
5. Support branch merging functionality

## Key Components

### AbstractExporterService

The `AbstractExporterService` is an abstract base class that implements the `IExporterService` and `IMessageConsumer<ExportCommand>` interfaces. It provides:

- Service lifecycle management (Initialize, Terminate)
- State management (UNINITIALIZED, INITIALIZING, READY, PROCESSING, ERROR, TERMINATING, TERMINATED)
- Error handling and recovery mechanisms
- Metrics collection
- Lifecycle hooks for customization

### ExporterServiceBase

The `ExporterServiceBase` is a concrete implementation of `AbstractExporterService` that provides common functionality for all exporter services, including:

- Logging
- Message consumption
- Basic configuration validation
- Exception classification
- Error detail extraction
- Recovery mechanisms
- Default lifecycle hook implementations

### ExporterServiceConfiguration

The `ExporterServiceConfiguration` class provides a standardized configuration model for exporter services, including:

- Service identification (ServiceId, Protocol)
- Connection settings (ConnectionTimeoutSeconds, OperationTimeoutSeconds)
- Retry settings (MaxRetryCount, RetryDelayMilliseconds, UseExponentialBackoff)
- Export settings (BatchSize, ValidateData)
- Schema settings (SchemaId, SchemaVersion)
- Merge settings (MergeStrategy)
- Additional parameters

### ExporterServiceException

The `ExporterServiceException` class provides a standardized exception type for exporter services, including:

- Error code
- Error message
- Error details
- Inner exception

## Usage

### Creating a Custom Exporter Service

To create a custom exporter service, extend the `ExporterServiceBase` class and implement the required methods:

```csharp
namespace MyNamespace
{
    public class MyCustomExporter : ExporterServiceBase
    {
        public override string ServiceId => "CUSTOM-EXPORTER-001";
        public override string Version => "1.0.0";
        public override string ServiceType => "CustomExporter";
        public override string Protocol => "CUSTOM";

        public MyCustomExporter(ILogger logger) : base(logger)
        {
        }

        public override ExporterCapabilities GetCapabilities()
        {
            return new ExporterCapabilities
            {
                SupportedDestinationTypes = new List<string> { "CustomDestination" },
                SupportedDataFormats = new List<string> { "JSON", "XML" },
                SupportedMergeStrategies = new List<MergeStrategy> { MergeStrategy.Append, MergeStrategy.Replace },
                SupportsStreaming = false,
                SupportsBatching = true,
                SupportsTransactions = true,
                MaxBatchSize = 1000
            };
        }

        public override MergeCapabilities GetMergeCapabilities()
        {
            return new MergeCapabilities
            {
                SupportedMergeStrategies = new List<MergeStrategy> { MergeStrategy.Append, MergeStrategy.Replace },
                SupportsPartialMerge = true,
                SupportsConflictResolution = true
            };
        }

        protected override ExportResult PerformExport(ExportParameters parameters, ExecutionContext context)
        {
            // Custom logic to export data to the destination
            var result = new ExportResult
            {
                Success = true,
                DestinationMetadata = new Dictionary<string, object>
                {
                    ["ExportTime"] = DateTime.UtcNow,
                    ["RecordCount"] = parameters.Data.GetRecordCount()
                },
                Destination = new DestinationInformation(_configuration),
                Statistics = _metrics
            };

            return result;
        }

        public override ExportResult MergeBranches(Dictionary<string, DataPackage> branchData, MergeStrategy strategy, ExecutionContext context)
        {
            // Custom logic to merge data from multiple branches
            var mergedData = new DataPackage();

            // Implement merge strategy
            switch (strategy)
            {
                case MergeStrategy.Append:
                    // Append all branch data
                    foreach (var data in branchData.Values)
                    {
                        mergedData.Append(data);
                    }
                    break;
                case MergeStrategy.Replace:
                    // Use the last branch data
                    if (branchData.Count > 0)
                    {
                        mergedData = branchData.Values.Last();
                    }
                    break;
                default:
                    throw new NotSupportedException($"Merge strategy {strategy} is not supported.");
            }

            // Export the merged data
            var parameters = new ExportParameters
            {
                Data = mergedData,
                DestinationId = context.GetValue<string>("DestinationId"),
                DestinationLocation = context.GetValue<string>("DestinationLocation"),
                DestinationConfiguration = context.GetValue<Dictionary<string, object>>("DestinationConfiguration")
            };

            return Export(parameters, context);
        }
    }
}
```

### Configuring an Exporter Service

```csharp
var configuration = new ExporterServiceConfiguration
{
    ServiceId = "CUSTOM-EXPORTER-001",
    Protocol = "CUSTOM",
    ConnectionTimeoutSeconds = 60,
    OperationTimeoutSeconds = 120,
    MaxRetryCount = 3,
    RetryDelayMilliseconds = 1000,
    UseExponentialBackoff = true,
    BatchSize = 500,
    ValidateData = true,
    SchemaId = "CUSTOM-SCHEMA-001",
    SchemaVersion = "1.0.0",
    MergeStrategy = MergeStrategy.Append
};

var exporter = new MyCustomExporter(new ConsoleLogger());
exporter.Initialize(configuration);
```

## Metrics Collection

The `AbstractExporterService` class provides methods for collecting metrics:

```csharp
// This is handled automatically by the AbstractExporterService
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

## Message Handling

The `ExporterServiceBase` class provides a default implementation of the `Consume` method:

```csharp
// This is handled automatically by the ExporterServiceBase
public override async Task Consume(ConsumeContext<ExportCommand> context)
{
    try
    {
        // Verify that this command is intended for this service
        if (context.Message.ExporterServiceId != ServiceId || context.Message.ExporterServiceVersion != Version)
        {
            return;
        }

        // Execute the export operation
        var result = Export(context.Message.Parameters, context.Message.Context);

        // Create and publish the result
        var commandResult = new ExportCommandResult(context.Message, result);
        await PublishResult(commandResult, context.CorrelationId);
    }
    catch (Exception ex)
    {
        // Handle and publish errors
        // ...
    }
}
```
