# FlowOrchestrator.Domain

## Overview

The FlowOrchestrator.Domain project is a class library that contains domain models and entities for the FlowOrchestrator system. It provides concrete implementations of the abstract interfaces defined in the FlowOrchestrator.Abstractions project.

## Purpose

This library serves as the domain model layer of the FlowOrchestrator system, providing:

- Abstract base classes for all entity types
- Concrete implementations of entity classes
- Validation logic for entities
- Entity relationship management

## Key Components

### Abstract Entity Classes

- **AbstractEntity**: Base implementation of `IEntity` interface
- **AbstractFlowEntity**: Base implementation of `IFlowEntity` interface
- **AbstractProcessingChainEntity**: Base implementation for processing chain entities
- **AbstractSourceEntity**: Base implementation for source entities
- **AbstractDestinationEntity**: Base implementation for destination entities
- **AbstractSourceAssignmentEntity**: Base implementation for source assignment entities
- **AbstractDestinationAssignmentEntity**: Base implementation for destination assignment entities
- **AbstractScheduledFlowEntity**: Base implementation for scheduled flow entities
- **AbstractTaskSchedulerEntity**: Base implementation for task scheduler entities

### Concrete Entity Classes

- **FlowEntity**: Concrete implementation of `AbstractFlowEntity`
- **ProcessingChainEntity**: Concrete implementation of `AbstractProcessingChainEntity`
- **FileSourceEntity**: Concrete implementation of `AbstractSourceEntity` for file sources
- **FileDestinationEntity**: Concrete implementation of `AbstractDestinationEntity` for file destinations
- **SourceAssignmentEntity**: Concrete implementation of `AbstractSourceAssignmentEntity`
- **DestinationAssignmentEntity**: Concrete implementation of `AbstractDestinationAssignmentEntity`
- **ScheduledFlowEntity**: Concrete implementation of `AbstractScheduledFlowEntity`
- **TaskSchedulerEntity**: Concrete implementation of `AbstractTaskSchedulerEntity`

## Usage

The entities in this library are used by various services in the FlowOrchestrator system to:

1. Define flow structures
2. Configure data sources and destinations
3. Establish connections between sources/destinations and importers/exporters
4. Schedule flow executions
5. Validate entity configurations

## Dependencies

- **FlowOrchestrator.Abstractions**: Core interfaces and abstract classes that define the system's contract

## Namespace

All components in this library are under the `FlowOrchestrator.Domain` namespace.

## Example

```csharp
// Create a new flow entity
var flowEntity = new FlowEntity
{
    FlowId = "flow-001",
    Name = "Sample Flow",
    Description = "A sample flow for demonstration purposes",
    ImporterServiceId = "file-importer-001",
    ProcessorServiceIds = new List<string> { "json-processor-001" },
    ExporterServiceIds = new List<string> { "file-exporter-001" },
    IsEnabled = true
};

// Validate the flow entity
var validationResult = flowEntity.Validate();
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
    }
}
```
