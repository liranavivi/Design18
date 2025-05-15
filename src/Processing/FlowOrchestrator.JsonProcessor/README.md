# FlowOrchestrator.JsonProcessor

This worker service provides JSON processing and transformation capabilities for the FlowOrchestrator system.

## Overview

The `FlowOrchestrator.JsonProcessor` service is designed to:

1. Process and transform JSON data
2. Validate JSON data against schema definitions
3. Support various transformation types (mapping, filtering, extraction, etc.)
4. Integrate with the FlowOrchestrator messaging system

## Key Components

### JsonProcessorService

The `JsonProcessorService` class is the core component of this service. It extends the `AbstractProcessorService` class from the `FlowOrchestrator.ProcessorBase` library and provides:

- Implementation of the `IProcessorService` interface
- JSON transformation capabilities
- JSON schema validation
- Error handling and recovery mechanisms

### JsonTransformationEngine

The `JsonTransformationEngine` class provides the JSON transformation functionality, including:

- Field mapping
- Field filtering
- Data extraction
- Object flattening
- Custom transformations
- Array handling

### JsonSchemaValidator

The `JsonSchemaValidator` class provides JSON schema validation functionality, including:

- Validation against schema definitions
- Schema creation from JSON schema documents
- Error reporting

## Configuration

The service is configured through the `appsettings.json` file. Here's an example configuration:

```json
{
  "JsonProcessor": {
    "ServiceId": "JSON-PROCESSOR-001",
    "ServiceType": "JsonProcessor",
    "MaxDepth": 64,
    "MaxStringLength": 1048576,
    "AllowComments": false,
    "AllowTrailingCommas": false,
    "WriteIndented": false,
    "IgnoreNullValues": false,
    "UseCamelCase": true,
    "OperationTimeoutSeconds": 60,
    "MaxRetryCount": 3,
    "RetryDelayMilliseconds": 1000
  }
}
```

## Usage

The service can be used to transform JSON data in various ways:

### Mapping

Maps fields from source to destination based on field mappings.

### Filtering

Filters fields from the source based on include/exclude lists.

### Extraction

Extracts specific values using JSON path expressions.

### Flattening

Flattens nested objects into a flat structure.

### Custom Transformations

Applies custom transformations to fields.

## Example

Here's an example of how to use the JSON processor service:

```csharp
// Create process parameters
var parameters = new ProcessParameters
{
    InputData = new DataPackage
    {
        Content = JsonNode.Parse(@"{
            ""name"": ""John Doe"",
            ""age"": 30,
            ""email"": ""john.doe@example.com"",
            ""address"": {
                ""street"": ""123 Main St"",
                ""city"": ""Anytown"",
                ""zipCode"": ""12345""
            }
        }"),
        ContentType = "application/json"
    },
    Options = new Dictionary<string, object>
    {
        { "TransformationType", "Map" },
        { "FieldMappings", new Dictionary<string, string>
            {
                { "name", "fullName" },
                { "age", "age" },
                { "email", "contactInfo.email" },
                { "address.city", "location.city" }
            }
        }
    }
};

// Process the data
var result = jsonProcessorService.Process(parameters, new ExecutionContext());

// Access the transformed data
var transformedData = result.TransformedData.Content;
```

## Dependencies

- `FlowOrchestrator.ProcessorBase`
- `FlowOrchestrator.Abstractions`
- `FlowOrchestrator.Common`
- `System.Text.Json`
