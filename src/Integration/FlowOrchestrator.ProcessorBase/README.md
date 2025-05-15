# FlowOrchestrator.ProcessorBase

This library provides the base implementation for all processor services in the FlowOrchestrator system. It includes the `AbstractProcessorService` class and common processor functionality.

## Overview

The `FlowOrchestrator.ProcessorBase` library is designed to:

1. Provide a standardized implementation pattern for processor services
2. Handle common functionality such as lifecycle management, error handling, and recovery
3. Support messaging through the `IMessageConsumer<ProcessCommand>` interface
4. Provide schema validation and compatibility checking
5. Support data transformation and processing

## Key Components

### AbstractProcessorService

The `AbstractProcessorService` class is the core component of this library. It provides:

- Implementation of the `IProcessorService` interface
- Implementation of the `IMessageConsumer<ProcessCommand>` interface
- Lifecycle management (initialization, processing, termination)
- Error handling and recovery mechanisms
- Schema validation integration

### Schema Validation Framework

The schema validation framework provides:

- Data validation against schema definitions
- Schema compatibility checking
- Support for various data types and validation rules

### Process Command Handling

The process command handling functionality provides:

- Base implementation for handling `ProcessCommand` messages
- Support for publishing processing results and errors

## Usage

To create a custom processor service, extend the `AbstractProcessorService` class and implement the required abstract methods:

```csharp
public class CustomProcessor : AbstractProcessorService
{
    public override string ServiceId => "custom-processor";
    public override string Version => "1.0.0";

    public override ProcessorCapabilities GetCapabilities()
    {
        return new ProcessorCapabilities
        {
            SupportedInputFormats = new List<string> { "application/json" },
            SupportedOutputFormats = new List<string> { "application/json" },
            SupportedTransformationTypes = new List<string> { "data-transformation" },
            SupportsValidation = true
        };
    }

    public override SchemaDefinition GetInputSchema()
    {
        return new SchemaDefinition
        {
            Name = "CustomProcessorInput",
            Version = "1.0.0",
            Fields = new List<SchemaField>
            {
                new SchemaField { Name = "field1", Type = "string", Required = true },
                new SchemaField { Name = "field2", Type = "integer", Required = false }
            }
        };
    }

    public override SchemaDefinition GetOutputSchema()
    {
        return new SchemaDefinition
        {
            Name = "CustomProcessorOutput",
            Version = "1.0.0",
            Fields = new List<SchemaField>
            {
                new SchemaField { Name = "result1", Type = "string", Required = true },
                new SchemaField { Name = "result2", Type = "integer", Required = true }
            }
        };
    }

    protected override ProcessingResult PerformProcessing(ProcessParameters parameters, ExecutionContext context)
    {
        // Custom processing logic here
        var result = new ProcessingResult();
        
        // Transform the data
        var transformedData = TransformData(parameters.InputData);
        
        // Create a data package
        result.TransformedData = new DataPackage
        {
            Content = transformedData,
            ContentType = "application/json",
            SchemaId = GetOutputSchema().Id
        };
        
        return result;
    }

    // Implement other required abstract methods...
}
```

## Dependencies

- **FlowOrchestrator.Abstractions**: Core interfaces and abstract classes that define the system's contract
- **FlowOrchestrator.Common**: Shared utilities, helpers, and common functionality

## Namespace

All components in this library are under the `FlowOrchestrator.Processing` namespace.
