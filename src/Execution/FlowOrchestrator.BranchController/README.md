# FlowOrchestrator.BranchController

## Overview

The `FlowOrchestrator.BranchController` is a Worker Service responsible for managing branch execution and parallel processing in the FlowOrchestrator system. It handles branch context management, branch isolation logic, and branch completion tracking.

## Key Components

### BranchContextService

The `BranchContextService` handles branch execution context management. It provides:

- Branch execution context creation and management
- Branch status tracking
- Branch step tracking (completed and pending steps)
- Branch metadata management

### BranchIsolationService

The `BranchIsolationService` handles branch isolation logic. It provides:

- Memory address registration and validation
- Branch-specific resource allocation
- Branch isolation enforcement

### BranchCompletionService

The `BranchCompletionService` handles branch completion tracking. It provides:

- Branch completion detection
- Branch completion processing
- Branch failure handling
- Branch statistics collection

### TelemetryService

The `TelemetryService` handles telemetry for branch execution. It provides:

- Branch execution tracking
- Step execution tracking
- Performance metrics collection

## Message Consumers

The service includes the following message consumers:

- `CreateBranchCommandConsumer`: Consumes commands to create a new branch
- `UpdateBranchStatusCommandConsumer`: Consumes commands to update branch status
- `AddCompletedStepCommandConsumer`: Consumes commands to add a completed step to a branch
- `AddPendingStepCommandConsumer`: Consumes commands to add a pending step to a branch

## Message Types

### Commands

- `CreateBranchCommand`: Command to create a new branch
- `UpdateBranchStatusCommand`: Command to update the status of a branch
- `AddCompletedStepCommand`: Command to add a completed step to a branch
- `AddPendingStepCommand`: Command to add a pending step to a branch

### Events

- `BranchCreatedEvent`: Event indicating that a branch has been created
- `BranchStatusChangedEvent`: Event indicating that a branch status has changed
- `BranchCompletedEvent`: Event indicating that a branch has completed
- `BranchFailedEvent`: Event indicating that a branch has failed

## Models

### BranchExecutionContext

The `BranchExecutionContext` represents the context for a branch execution. It includes:

- Branch identifier and parent branch identifier
- Branch status (NEW, IN_PROGRESS, COMPLETED, FAILED, CANCELLED)
- Branch start and end timestamps
- Completed and pending steps
- Branch dependencies and merge points
- Branch configuration and parameters
- Branch metadata and statistics
- Memory addresses associated with the branch

## Configuration

The service is configured through the `appsettings.json` file. Here's an example configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "BranchController": {
    "HealthCheckInterval": "00:01:00"
  }
}
```

## Dependencies

- `FlowOrchestrator.Abstractions`: Core interfaces and abstract classes
- `FlowOrchestrator.Common`: Common utilities and helpers
- `FlowOrchestrator.Infrastructure.Messaging`: Messaging infrastructure

## Usage

The BranchController service is designed to be used as part of the FlowOrchestrator system. It interacts with other components through message-based communication.

### Creating a Branch

To create a branch, send a `CreateBranchCommand` to the message bus:

```csharp
await _messageBus.PublishAsync(new CreateBranchCommand
{
    ExecutionId = executionId,
    BranchId = branchId,
    ParentBranchId = parentBranchId,
    Parameters = parameters,
    Configuration = configuration,
    Priority = priority,
    TimeoutMs = timeoutMs,
    Context = executionContext
});
```

### Updating Branch Status

To update a branch status, send an `UpdateBranchStatusCommand` to the message bus:

```csharp
await _messageBus.PublishAsync(new UpdateBranchStatusCommand
{
    ExecutionId = executionId,
    BranchId = branchId,
    Status = status.ToString(),
    Context = executionContext
});
```

### Adding a Completed Step

To add a completed step to a branch, send an `AddCompletedStepCommand` to the message bus:

```csharp
await _messageBus.PublishAsync(new AddCompletedStepCommand
{
    ExecutionId = executionId,
    BranchId = branchId,
    StepId = stepId,
    Context = executionContext
});
```

### Adding a Pending Step

To add a pending step to a branch, send an `AddPendingStepCommand` to the message bus:

```csharp
await _messageBus.PublishAsync(new AddPendingStepCommand
{
    ExecutionId = executionId,
    BranchId = branchId,
    StepId = stepId,
    Context = executionContext
});
```
