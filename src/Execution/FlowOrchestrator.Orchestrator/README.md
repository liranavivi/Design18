# FlowOrchestrator.Orchestrator

Central coordination service for flow execution and management in the FlowOrchestrator system.

## Overview

The `FlowOrchestrator.Orchestrator` service is designed to:

1. Manage the execution of flows from activation through completion
2. Coordinate branch execution and parallel processing
3. Generate and manage memory addressing patterns
4. Implement merge strategies for combining data from multiple branches
5. Provide APIs for flow execution management

## Key Components

### OrchestratorService

The `OrchestratorService` is the core component of this service. It provides:

- Flow execution management
- Execution context tracking
- Service lifecycle management (Initialize, Terminate)
- State management (UNINITIALIZED, INITIALIZING, READY, PROCESSING, ERROR, TERMINATING, TERMINATED)

### BranchManagementService

The `BranchManagementService` handles branch execution and parallel processing. It provides:

- Branch execution context creation and management
- Branch status tracking
- Branch completion tracking
- Branch isolation logic

### MemoryAddressService

The `MemoryAddressService` generates and manages memory addresses for data exchange. It provides:

- Memory address generation following the established pattern
- Memory address parsing
- Input/output address generation for different service types

### MergeStrategyService

The `MergeStrategyService` implements merge strategies for combining data from multiple branches. It provides:

- Merge trigger determination based on branch status
- Merge strategy selection based on exporter capabilities
- Support for different merge policies (ALL_BRANCHES_COMPLETE, ANY_BRANCH_COMPLETE, etc.)

## API Endpoints

### Flow Execution API

- `POST /api/flowexecution/start`: Starts a new flow execution
- `GET /api/flowexecution/{executionId}/status`: Gets the status of a flow execution
- `POST /api/flowexecution/{executionId}/cancel`: Cancels a flow execution

### Branch Management API

- `GET /api/branch/execution/{executionId}`: Gets all branches for an execution
- `GET /api/branch/execution/{executionId}/branch/{branchId}`: Gets a specific branch for an execution
- `PUT /api/branch/execution/{executionId}/branch/{branchId}/status`: Updates the status of a branch

### Memory Management API

- `GET /api/memory/address`: Generates a memory address
- `GET /api/memory/parse`: Parses a memory address

## Message Consumers

The service includes the following message consumers:

- `TriggerFlowCommandConsumer`: Consumes commands to trigger flow execution

## Configuration

The service is configured through the `appsettings.json` file. Here's an example configuration:

```json
{
  "Orchestrator": {
    "ServiceId": "ORCHESTRATOR-SERVICE",
    "MaxConcurrentExecutions": 100,
    "DefaultMergePolicy": "ALL_BRANCHES_COMPLETE",
    "DefaultMergeStrategy": "Append",
    "MemoryAddressPattern": "{executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}"
  },
  "MessageBus": {
    "TransportType": "InMemory",
    "RetryCount": 3,
    "RetryIntervalSeconds": 5,
    "PrefetchCount": 16,
    "ConcurrencyLimit": 8
  }
}
```

## Dependencies

- `FlowOrchestrator.Abstractions`: Core interfaces and abstract classes
- `FlowOrchestrator.Domain`: Domain models and entities
- `FlowOrchestrator.Common`: Common utilities and helpers
- `FlowOrchestrator.Infrastructure.Messaging.MassTransit`: Messaging infrastructure

## Usage Examples

### Starting a Flow Execution

```csharp
// Using the API
var client = new HttpClient();
var request = new StartFlowRequest
{
    FlowId = "FLOW-001",
    FlowVersion = "1.0.0",
    ImporterServiceId = "FILE-IMPORTER-001",
    ProcessorServiceIds = new List<string> { "JSON-PROCESSOR-001" },
    ExporterServiceIds = new List<string> { "FILE-EXPORTER-001" },
    Parameters = new Dictionary<string, object>
    {
        { "SourcePath", "C:\\Data\\Input" },
        { "DestinationPath", "C:\\Data\\Output" }
    }
};
var response = await client.PostAsJsonAsync("https://localhost:5001/api/flowexecution/start", request);
var executionContext = await response.Content.ReadFromJsonAsync<ExecutionContext>();
```

### Getting Execution Status

```csharp
// Using the API
var client = new HttpClient();
var response = await client.GetAsync($"https://localhost:5001/api/flowexecution/{executionId}/status");
var status = await response.Content.ReadFromJsonAsync<ExecutionStatusResponse>();
```

## Memory Addressing Pattern

The service uses a hierarchical memory addressing scheme to ensure proper data isolation and flow:

```
{executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}
```

For example:
- `EXEC-123:FLOW-001:IMPORT:main:IMPORTER-001:Output`
- `EXEC-123:FLOW-001:PROCESS:branchA:PROCESSOR-001:Input`
- `EXEC-123:FLOW-001:PROCESS:branchA:PROCESSOR-001:Output`
- `EXEC-123:FLOW-001:EXPORT:main:EXPORTER-001:Input`
