# FlowOrchestrator.MemoryManager

## Overview

The FlowOrchestrator.MemoryManager is a worker service that manages shared memory for efficient data transfer between components in the FlowOrchestrator system. It provides memory allocation, lifecycle management, and access control services.

## Purpose

This service serves as the central memory management component for the FlowOrchestrator system, providing:

- Memory allocation and deallocation
- Memory lifecycle management
- Memory access control
- Memory usage statistics
- Memory cleanup for completed executions and branches

## Key Components

### MemoryManagerService

The `MemoryManagerService` is the main service that implements the `IMemoryManager` interface. It provides:

- Service lifecycle management (Initialize, Terminate)
- State management (UNINITIALIZED, INITIALIZING, READY, PROCESSING, ERROR, TERMINATING, TERMINATED)
- Memory allocation and deallocation
- Memory access validation
- Memory cleanup

### MemoryAllocationService

The `MemoryAllocationService` handles memory allocation and deallocation. It provides:

- Memory address generation following the established pattern
- Memory allocation and deallocation
- Memory existence checking
- Memory statistics collection
- Memory cleanup for executions and branches

### MemoryAccessControlService

The `MemoryAccessControlService` handles memory access control. It provides:

- Access validation based on execution context
- Access control list management
- Branch isolation enforcement

### Message Consumers

The service includes the following message consumers:

- `AllocateMemoryCommandConsumer`: Consumes commands to allocate memory
- `DeallocateMemoryCommandConsumer`: Consumes commands to deallocate memory
- `CleanupExecutionMemoryCommandConsumer`: Consumes commands to clean up memory for an execution

### API Controllers

The service exposes the following API endpoints:

- `POST /api/memory/allocate`: Allocates memory
- `DELETE /api/memory/deallocate`: Deallocates memory
- `GET /api/memory/exists`: Checks if a memory address exists
- `GET /api/memory/statistics`: Gets memory statistics
- `DELETE /api/memory/cleanup/execution/{executionId}`: Cleans up memory for an execution
- `DELETE /api/memory/cleanup/branch/{executionId}/{branchId}`: Cleans up memory for a branch
- `POST /api/memory/validate-access`: Validates access to a memory address

## Configuration

The service is configured through the `appsettings.json` file. Here's an example configuration:

```json
{
  "MemoryManager": {
    "ServiceId": "MEMORY-MANAGER-SERVICE",
    "CleanupIntervalMinutes": 15,
    "MonitoringIntervalMinutes": 5,
    "MaxMemoryAgeHours": 24
  },
  "Hazelcast": {
    "ClusterName": "flow-orchestrator",
    "NetworkAddresses": [ "localhost:5701" ]
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
- `FlowOrchestrator.Common`: Common utilities and helpers
- `FlowOrchestrator.Infrastructure.Data.Hazelcast`: Hazelcast integration for distributed memory

## Memory Addressing Pattern

The service uses a hierarchical memory addressing scheme to ensure proper data isolation and flow:

```
{executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}:{additionalInfo}
```

Example:
```
EXEC-123:FLOW-001:PROCESS:branchA:2:ProcessingResult:TransformedData
```

## Usage Examples

### Allocating Memory

```csharp
// Using the API
var client = new HttpClient();
var request = new MemoryAllocationRequest
{
    ExecutionId = "EXEC-123",
    FlowId = "FLOW-001",
    StepType = "PROCESS",
    BranchPath = "branchA",
    StepId = "2",
    DataType = "ProcessingResult",
    TimeToLiveSeconds = 3600
};
var response = await client.PostAsJsonAsync("https://localhost:5001/api/memory/allocate", request);
var result = await response.Content.ReadFromJsonAsync<MemoryAllocationResult>();
```

### Deallocating Memory

```csharp
// Using the API
var client = new HttpClient();
var response = await client.DeleteAsync($"https://localhost:5001/api/memory/deallocate?memoryAddress={memoryAddress}");
var success = await response.Content.ReadFromJsonAsync<bool>();
```

### Cleaning Up Execution Memory

```csharp
// Using the API
var client = new HttpClient();
var response = await client.DeleteAsync($"https://localhost:5001/api/memory/cleanup/execution/{executionId}");
var cleanedUpCount = await response.Content.ReadFromJsonAsync<int>();
```
