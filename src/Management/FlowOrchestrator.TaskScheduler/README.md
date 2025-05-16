# FlowOrchestrator.TaskScheduler

This worker service is responsible for scheduling and triggering flow executions in the FlowOrchestrator system.

## Overview

The `FlowOrchestrator.TaskScheduler` service is designed to:

1. Schedule flows for execution based on configured schedules
2. Trigger flow executions at the scheduled times
3. Manage the lifecycle of scheduled flows
4. Provide an API for scheduling and triggering flows

## Key Components

### TaskSchedulerService

The `TaskSchedulerService` class is the core component of this service. It provides:

- Methods for scheduling flows for execution
- Methods for triggering immediate flow execution
- Integration with the Quartz scheduler service

### FlowExecutionService

The `FlowExecutionService` class is responsible for executing flows. It provides:

- Methods for executing flows
- Methods for publishing flow execution results
- Integration with the message bus

### SchedulerManager

The `SchedulerManager` class manages the lifecycle of the scheduler. It provides:

- Methods for initializing the scheduler
- Methods for shutting down the scheduler
- Methods for loading scheduled flows from the database

### Message Consumers

The service includes the following message consumers:

- `TriggerFlowCommandConsumer`: Consumes commands to trigger immediate flow execution
- `ScheduleFlowCommandConsumer`: Consumes commands to schedule flows for execution

## Message Contracts

The service defines the following message contracts:

- `TriggerFlowCommand`: Command to trigger a flow execution immediately
- `ScheduleFlowCommand`: Command to schedule a flow for execution
- `FlowExecutionResult`: Result of a flow execution
- `FlowSchedulingResult`: Result of scheduling a flow

## Configuration

The service can be configured using the following settings in `appsettings.json`:

```json
{
  "TaskScheduler": {
    "SchedulerName": "FlowOrchestratorTaskScheduler",
    "InstanceId": "AUTO",
    "ThreadCount": 10,
    "MakeSchedulerThreadDaemon": true,
    "AutoStart": true,
    "WaitForJobsToCompleteOnShutdown": true,
    "MaxConcurrentExecutions": 10,
    "DefaultTimeoutSeconds": 3600,
    "DefaultRetryCount": 3,
    "DefaultRetryDelaySeconds": 300
  },
  "Quartz": {
    "SchedulerName": "FlowOrchestratorQuartzScheduler",
    "InstanceId": "AUTO",
    "ThreadCount": 10,
    "MakeSchedulerThreadDaemon": true,
    "JobStoreType": "Quartz.Simpl.RAMJobStore, Quartz",
    "SkipUpdateCheck": true,
    "SerializerType": "json",
    "AutoStart": true,
    "WaitForJobsToCompleteOnShutdown": true
  }
}
```

## Usage Examples

### Scheduling a Flow

```csharp
// Create a schedule flow command
var command = new ScheduleFlowCommand(
    scheduledFlowEntityId: "scheduled-flow-001",
    flowEntityId: "flow-001",
    taskSchedulerEntityId: "task-scheduler-001",
    flowParameters: new Dictionary<string, string>
    {
        { "param1", "value1" },
        { "param2", "value2" }
    },
    replaceExisting: true,
    context: new ExecutionContext());

// Publish the command
await _messageBus.PublishAsync(command);
```

### Triggering a Flow

```csharp
// Create a trigger flow command
var command = new TriggerFlowCommand(
    scheduledFlowEntityId: "scheduled-flow-001",
    flowEntityId: "flow-001",
    taskSchedulerEntityId: "task-scheduler-001",
    flowParameters: new Dictionary<string, string>
    {
        { "param1", "value1" },
        { "param2", "value2" }
    },
    context: new ExecutionContext());

// Publish the command
await _messageBus.PublishAsync(command);
```

## Dependencies

- **FlowOrchestrator.Abstractions**: Core interfaces and abstract classes that define the system's contract
- **FlowOrchestrator.Domain**: Domain models and entities
- **FlowOrchestrator.Infrastructure.Scheduling.Quartz**: Quartz.NET integration for scheduling
- **FlowOrchestrator.Infrastructure.Messaging.MassTransit**: MassTransit integration for messaging

## Namespace

All components in this service are under the `FlowOrchestrator.Management.Scheduling` namespace.
