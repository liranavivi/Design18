# FlowOrchestrator.VersionManager

## Overview

The FlowOrchestrator.VersionManager is a Web API service responsible for managing version compatibility and lifecycle in the FlowOrchestrator system. It provides functionality for tracking component versions, managing version status transitions, and validating compatibility between different components and versions.

## Key Components

### Version Compatibility Matrix

The Version Compatibility Matrix is a core concept that defines which specific versions of components can work together. It is maintained as a comprehensive cross-reference of all component versions and is used during flow validation to ensure compatible versions.

### Version Status Management

The service manages the lifecycle of component versions through status transitions:

- **DRAFT**: Version is in development and not ready for use
- **ACTIVE**: Version is active and can be used
- **DEPRECATED**: Version is deprecated but still usable
- **ARCHIVED**: Version is archived and no longer usable
- **DISABLED**: Version is temporarily disabled

### Upgrade/Downgrade Coordination

The service provides functionality for coordinating upgrades and downgrades between component versions, ensuring that all dependencies are compatible.

## API Endpoints

### Version Management

- `GET /api/versions/{componentType}/{componentId}/{version}`: Get version information for a specific component and version
- `GET /api/versions/{componentType}/{componentId}/history`: Get the version history for a specific component
- `PUT /api/versions/{componentType}/{componentId}/{version}/status`: Update the status of a specific component version
- `POST /api/versions`: Register a new version of a component
- `DELETE /api/versions/{componentType}/{componentId}/{version}`: Delete a specific component version
- `GET /api/versions/{componentType}`: Get all registered components of a specific type
- `GET /api/versions/types`: Get all registered component types

### Compatibility Management

- `GET /api/compatibility/{componentType}/{componentId}/{version}`: Get the compatibility matrix for a specific component and version
- `POST /api/compatibility/check`: Check if two component versions are compatible

## Data Models

### ComponentType

Enum representing the type of a component in the FlowOrchestrator system:

- `IMPORTER_SERVICE`
- `PROCESSOR_SERVICE`
- `EXPORTER_SERVICE`
- `SOURCE_ENTITY`
- `DESTINATION_ENTITY`
- `SOURCE_ASSIGNMENT_ENTITY`
- `DESTINATION_ASSIGNMENT_ENTITY`
- `FLOW_ENTITY`
- `PROCESSING_CHAIN_ENTITY`
- `TASK_SCHEDULER_ENTITY`
- `SCHEDULED_FLOW_ENTITY`
- `PROTOCOL`
- `PROTOCOL_HANDLER`
- `STATISTICS_PROVIDER`
- `OTHER`

### VersionInfo

Represents detailed information about a component version:

- `ComponentType`: The component type
- `ComponentId`: The component identifier
- `Version`: The version
- `CreatedTimestamp`: The timestamp when the version was created
- `LastModifiedTimestamp`: The timestamp when the version was last modified
- `VersionDescription`: The description of the version
- `PreviousVersionId`: The identifier of the previous version
- `VersionStatus`: The status of the version
- `Metadata`: Additional metadata for the version

### CompatibilityMatrix

Represents a compatibility matrix for a component, defining which other components and versions it is compatible with:

- `ComponentType`: The component type
- `ComponentId`: The component identifier
- `Version`: The component version
- `CompatibleWith`: The list of compatibility rules

### CompatibilityRule

Represents a compatibility rule between component types and versions:

- `ComponentType`: The component type that this rule applies to
- `VersionRange`: The version range that this rule applies to
- `Notes`: Notes about this compatibility rule
- `ConfigurationRequirements`: Configuration requirements for this compatibility rule

## Dependencies

- `FlowOrchestrator.Abstractions`: Core interfaces and abstract classes
- `FlowOrchestrator.Domain`: Domain models and entities
- `FlowOrchestrator.Common`: Shared utilities and helpers
