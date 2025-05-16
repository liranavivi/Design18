# FlowOrchestrator.Infrastructure.Data.Hazelcast

## Overview

The FlowOrchestrator.Infrastructure.Data.Hazelcast project is a class library that provides Hazelcast integration for in-memory data grid functionality in the FlowOrchestrator system. It implements distributed memory management, cache implementations, and cluster management.

## Purpose

This library serves as the in-memory data grid layer for the FlowOrchestrator system, providing:

- Distributed memory management
- Cache implementations
- Cluster management
- Repository pattern implementation for Hazelcast
- Dependency injection extensions

## Key Components

### Configuration

- **HazelcastOptions**: Configuration options for Hazelcast
- **NearCacheOptions**: Configuration options for near cache

### Core Components

- **HazelcastContext**: Context class for Hazelcast connection management

### Repository Interfaces

- **IHazelcastRepository<T>**: Generic repository interface for CRUD operations
- **IHazelcastEntityRepository<T>**: Repository interface for entity operations

### Repository Implementations

- **HazelcastRepository<T>**: Base implementation of the repository pattern for Hazelcast
- **HazelcastEntityRepository<T>**: Base implementation of the entity repository for Hazelcast

### Cache Components

- **IDistributedCache**: Interface for distributed cache operations
- **HazelcastDistributedCache**: Hazelcast implementation of the distributed cache

### Cluster Management

- **ClusterManager**: Class for managing Hazelcast cluster operations

### Dependency Injection Extensions

- **HazelcastServiceCollectionExtensions**: Extension methods for registering Hazelcast services with the dependency injection container

## Usage

### Configuration

Add the Hazelcast configuration to your application configuration:

```json
{
  "Hazelcast": {
    "ClusterName": "dev",
    "NetworkAddresses": [ "localhost:5701" ],
    "ConnectionTimeoutSeconds": 30,
    "ConnectionRetryCount": 3,
    "ConnectionRetryDelayMs": 1000,
    "SmartRouting": true,
    "UseSsl": false,
    "MaxConnectionIdleTimeSeconds": 60,
    "MaxConcurrentInvocations": 100,
    "InvocationTimeoutSeconds": 120,
    "StatisticsEnabled": true,
    "StatisticsPeriodSeconds": 5,
    "NearCaches": {
      "default-cache": {
        "MaxSize": 10000,
        "TimeToLiveSeconds": 0,
        "MaxIdleSeconds": 0,
        "EvictionPolicy": "LRU",
        "InvalidateOnChange": true
      }
    }
  }
}
```

### Register Hazelcast Services

Register Hazelcast services in your application's startup:

```csharp
services.AddHazelcast(Configuration);
```

Or with custom options:

```csharp
services.AddHazelcast(options =>
{
    options.ClusterName = "dev";
    options.NetworkAddresses = new List<string> { "localhost:5701" };
    options.ConnectionTimeoutSeconds = 30;
    // ... other options
});
```

### Register Repositories

Register repositories for your entity types:

```csharp
services.AddHazelcastEntityRepository<FlowEntity, HazelcastEntityRepository<FlowEntity>>("flow-entities");
services.AddHazelcastEntityRepository<FileSourceEntity, HazelcastEntityRepository<FileSourceEntity>>("file-sources");
services.AddHazelcastEntityRepository<FileDestinationEntity, HazelcastEntityRepository<FileDestinationEntity>>("file-destinations");
// ... other repositories
```

### Use the Distributed Cache

Inject and use the distributed cache in your services:

```csharp
public class MyService
{
    private readonly IDistributedCache _cache;

    public MyService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string> GetValueAsync(string key)
    {
        return await _cache.GetOrSetAsync<string>(key, async () =>
        {
            // Compute the value if not in cache
            return "computed-value";
        }, expirationSeconds: 300);
    }
}
```

### Use the Cluster Manager

Inject and use the cluster manager in your services:

```csharp
public class MyService
{
    private readonly ClusterManager _clusterManager;

    public MyService(ClusterManager clusterManager)
    {
        _clusterManager = clusterManager;
    }

    public async Task<bool> IsClusterHealthyAsync()
    {
        return await _clusterManager.IsClusterHealthyAsync();
    }

    public async Task<Dictionary<string, object>> GetClusterStatisticsAsync()
    {
        return await _clusterManager.GetClusterStatisticsAsync();
    }
}
```

## Dependencies

- Hazelcast.Net: The official Hazelcast .NET client
- Microsoft.Extensions.DependencyInjection.Abstractions: For dependency injection support
- Microsoft.Extensions.Options.ConfigurationExtensions: For configuration support
- Microsoft.Extensions.Logging.Abstractions: For logging support
- FlowOrchestrator.Abstractions: For core interfaces and abstractions
