# FlowOrchestrator.Infrastructure.Data.MongoDB

## Overview

The FlowOrchestrator.Infrastructure.Data.MongoDB project is a class library that provides MongoDB integration for data persistence in the FlowOrchestrator system. It implements the repository pattern for MongoDB and provides document mapping for entity classes.

## Purpose

This library serves as the data access layer for MongoDB in the FlowOrchestrator system, providing:

- Repository implementations for all entity types
- MongoDB connection management
- Document mapping for MongoDB
- Dependency injection extensions

## Key Components

### Repository Interfaces

- **IRepository<T>**: Generic repository interface for CRUD operations
- **IEntityRepository<T>**: Repository interface for entity operations

### MongoDB Configuration

- **MongoDbOptions**: Configuration options for MongoDB
- **MongoDbContext**: Context class for MongoDB connection management

### Base Repository Implementation

- **MongoRepository<T>**: Base implementation of the repository pattern for MongoDB
- **MongoEntityRepository<T>**: Base implementation of the entity repository for MongoDB

### Entity-Specific Repositories

- **MongoFlowEntityRepository**: Repository for flow entities
- **MongoSourceEntityRepository**: Repository for source entities
- **MongoDestinationEntityRepository**: Repository for destination entities
- **MongoProcessingChainEntityRepository**: Repository for processing chain entities
- **MongoSourceAssignmentEntityRepository**: Repository for source assignment entities
- **MongoDestinationAssignmentEntityRepository**: Repository for destination assignment entities
- **MongoScheduledFlowEntityRepository**: Repository for scheduled flow entities
- **MongoTaskSchedulerEntityRepository**: Repository for task scheduler entities

### Document Mapping

- **EntityMap<T>**: Base class for entity mapping
- **FlowEntityMap**: Mapping configuration for FlowEntity

### Dependency Injection Extensions

- **MongoDbServiceCollectionExtensions**: Extension methods for registering MongoDB services with the dependency injection container

## Usage

To use MongoDB for data persistence in your application:

1. Add the MongoDB connection string to your application configuration:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "FlowOrchestrator",
    "MaxConnectionPoolSize": 100,
    "ConnectionTimeoutSeconds": 30,
    "SocketTimeoutSeconds": 60,
    "ServerSelectionTimeoutSeconds": 30,
    "UseSsl": false,
    "RetryWrites": true,
    "RetryReads": true
  }
}
```

2. Register MongoDB services in your application's startup:

```csharp
services.AddMongoDb(Configuration);
```

3. Inject the repositories into your services:

```csharp
public class MyService
{
    private readonly IEntityRepository<FlowEntity> _flowRepository;

    public MyService(IEntityRepository<FlowEntity> flowRepository)
    {
        _flowRepository = flowRepository;
    }

    public async Task<FlowEntity> GetFlowAsync(string flowId)
    {
        return await _flowRepository.GetByIdAsync(flowId);
    }
}
```

## Dependencies

- **FlowOrchestrator.Abstractions**: Core interfaces and abstract classes that define the system's contract
- **FlowOrchestrator.Domain**: Domain models and entities for the system
- **MongoDB.Driver**: MongoDB .NET driver

## Namespace

All components in this library are under the `FlowOrchestrator.Infrastructure.Data.MongoDB` namespace.
