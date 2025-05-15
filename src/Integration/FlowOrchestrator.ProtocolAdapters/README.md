# FlowOrchestrator.ProtocolAdapters

This library provides protocol-specific adapters for various data sources and destinations in the FlowOrchestrator system. It includes the `AbstractProtocol` and `AbstractProtocolHandler` classes, along with protocol-specific implementations.

## Overview

The `FlowOrchestrator.ProtocolAdapters` library is designed to:

1. Provide a standardized implementation pattern for communication protocols
2. Handle common functionality such as connection management, error handling, and data operations
3. Support various data formats and authentication methods
4. Provide protocol-specific implementations for common protocols like File and REST

## Key Components

### AbstractProtocol

The `AbstractProtocol` class is the core component of this library. It provides:

- Implementation of the `IProtocol` interface
- Protocol identification properties
- Capability discovery methods
- Connection parameter management
- Handler creation functionality

### AbstractProtocolHandler

The `AbstractProtocolHandler` class provides:

- Implementation of the `IProtocolHandler` interface
- Connection management methods
- Data operations (Read/Write)
- Error handling and recovery methods
- Resource management through the `IDisposable` interface

### Protocol Implementations

The library includes implementations for common protocols:

- **FileProtocol**: For file system operations

## Usage

To use a protocol in your application:

```csharp
// Create a protocol instance
var fileProtocol = new FileProtocol();

// Get protocol capabilities
var capabilities = fileProtocol.GetCapabilities();

// Get connection parameters
var connectionParams = fileProtocol.GetConnectionParameters();

// Create connection parameters
var parameters = new Dictionary<string, string>
{
    { "basePath", "C:\\Data" },
    { "filePattern", "*.json" },
    { "recursive", "true" }
};

// Validate connection parameters
var validationResult = fileProtocol.ValidateConnectionParameters(parameters);
if (!validationResult.IsValid)
{
    // Handle validation errors
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
    }
    return;
}

// Create a protocol handler
var handler = fileProtocol.CreateHandler(parameters);

// Connect
if (handler.Connect())
{
    try
    {
        // Read data
        var readOptions = new Dictionary<string, object>
        {
            { "filePath", "data.json" }
        };
        var data = handler.Read(readOptions);

        // Process data
        Console.WriteLine($"Read data: {data.Content}");

        // Write data
        var writeOptions = new Dictionary<string, object>
        {
            { "filePath", "output.json" }
        };
        handler.Write(data, writeOptions);
    }
    finally
    {
        // Disconnect
        handler.Disconnect();
    }
}
```

## Creating Custom Protocols

To create a custom protocol, extend the `AbstractProtocol` and `AbstractProtocolHandler` classes:

```csharp
public class CustomProtocol : AbstractProtocol
{
    public override string Name => "custom";
    public override string Description => "Custom protocol implementation";

    public override ProtocolCapabilities GetCapabilities()
    {
        // Define protocol capabilities
    }

    public override ConnectionParameters GetConnectionParameters()
    {
        // Define connection parameters
    }

    public override IProtocolHandler CreateHandler(Dictionary<string, string> parameters)
    {
        // Create and return a custom protocol handler
    }
}

public class CustomProtocolHandler : AbstractProtocolHandler
{
    public override string ProtocolName => "custom";
    public override bool IsConnected { get; }

    public CustomProtocolHandler(Dictionary<string, string> parameters)
        : base(parameters)
    {
        // Initialize handler
    }

    public override bool Connect()
    {
        // Implement connection logic
    }

    public override void Disconnect()
    {
        // Implement disconnection logic
    }

    public override DataPackage Read(Dictionary<string, object> options)
    {
        // Implement read logic
    }

    public override bool Write(DataPackage data, Dictionary<string, object> options)
    {
        // Implement write logic
    }

    protected override string ClassifyException(Exception ex)
    {
        // Classify exceptions
    }

    protected override Dictionary<string, object> GetErrorDetails(Exception ex)
    {
        // Get exception details
    }
}
```

## Dependencies

- **FlowOrchestrator.Abstractions**: Core interfaces and abstract classes that define the system's contract
- **FlowOrchestrator.Common**: Shared utilities, helpers, and common functionality

## Namespace

All components in this library are under the `FlowOrchestrator.Integration.Protocols` namespace.
