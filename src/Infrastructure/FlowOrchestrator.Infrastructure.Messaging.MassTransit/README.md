# FlowOrchestrator.Infrastructure.Messaging.MassTransit

This library provides MassTransit integration for the FlowOrchestrator system, implementing the messaging infrastructure.

## Overview

The `FlowOrchestrator.Infrastructure.Messaging.MassTransit` library is designed to:

1. Provide a standardized implementation of the message bus using MassTransit
2. Handle consumer registration and message routing
3. Support various transport types (InMemory, RabbitMQ, Azure Service Bus)
4. Provide message serialization configuration
5. Bridge between FlowOrchestrator's messaging abstractions and MassTransit

## Key Components

### MassTransitMessageBus

The `MassTransitMessageBus` class is the core component of this library. It provides:

- Implementation of the `IMessageBus` interface
- Methods for publishing messages to all subscribers
- Methods for sending messages to specific endpoints
- Methods for request-response communication

### ConsumerAdapter

The `ConsumerAdapter<TMessage>` class adapts FlowOrchestrator's `IMessageConsumer<TMessage>` to MassTransit's `IConsumer<TMessage>`. It:

- Bridges between the two consumer interfaces
- Handles message consumption and error handling
- Adapts MassTransit's ConsumeContext to FlowOrchestrator's ConsumeContext

### ServiceCollectionExtensions

The `ServiceCollectionExtensions` class provides extension methods for registering MassTransit services:

- `AddFlowOrchestratorMessageBus`: Registers the message bus with the specified transport
- `AddFlowOrchestratorConsumer<TMessage, TConsumer>`: Registers a FlowOrchestrator consumer with MassTransit

## Usage Examples

### Registering the Message Bus

```csharp
// In Program.cs or Startup.cs
services.AddFlowOrchestratorMessageBus(options =>
{
    options.TransportType = TransportType.RabbitMq;
    options.HostAddress = "rabbitmq://localhost";
    options.Username = "guest";
    options.Password = "guest";
    options.RetryCount = 3;
    options.RetryIntervalSeconds = 5;
});
```

### Registering a Consumer

```csharp
// In Program.cs or Startup.cs
services.AddFlowOrchestratorConsumer<ImportCommand, ImportCommandConsumer>();
```

### Publishing a Message

```csharp
// In a service class
public class MyService
{
    private readonly IMessageBus _messageBus;

    public MyService(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task PublishCommandAsync(ImportCommand command)
    {
        await _messageBus.PublishAsync(command);
    }
}
```

### Implementing a Consumer

```csharp
public class ImportCommandConsumer : IMessageConsumer<ImportCommand>
{
    public async Task Consume(ConsumeContext<ImportCommand> context)
    {
        var command = context.Message;
        // Process the command
        await Task.CompletedTask;
    }
}
```

## Configuration Options

The `MessageBusOptions` class provides the following configuration options:

- `TransportType`: The transport type (InMemory, RabbitMQ, Azure Service Bus)
- `HostAddress`: The host address for the transport
- `Username`: The username for authentication
- `Password`: The password for authentication
- `VirtualHost`: The virtual host for RabbitMQ
- `RetryCount`: The number of retry attempts
- `RetryIntervalSeconds`: The interval between retry attempts in seconds
- `PrefetchCount`: The number of messages to prefetch
- `ConcurrencyLimit`: The maximum number of concurrent messages to process
