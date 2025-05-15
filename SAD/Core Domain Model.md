# 2. Core Domain Model

## 2.1 Entity Framework Overview

The FlowOrchestrator system is built on a comprehensive entity model that defines the relationships and interactions between various components. The entity model is organized into four main categories:

1. **Flow Entities**: Define the structure and behavior of data processing workflows
2. **Configuration Entities**: Define the locations and protocols for data sources and destinations
3. **Connection Entities**: Define the relationships between sources/destinations and importers/exporters
4. **Scheduling Entities**: Define when and how flows are executed

The following diagram illustrates the high-level relationships between these entity types:

```
┌─ Flow Entities ─────────┐      ┌─ Configuration Entities ─┐
│                         │      │                          │
│  ┌─────────────────┐    │      │   ┌────────────────┐     │
│  │ Processing Chain│    │      │   │  Source Entity │     │
│  └─────────────────┘    │      │   └────────────────┘     │
│          │              │      │          │               │
│          ▼              │      │          ▼               │
│  ┌─────────────────┐    │      │   ┌────────────────┐     │
│  │   Flow Entity   │    │      │   │Destination Entity    │
│  └─────────────────┘    │      │   └────────────────┘     │
│                         │      │                          │
└─────────────────────────┘      └──────────────────────────┘
          │                                 │
          │                                 │
          ▼                                 ▼
┌─ Connection Entities ──┐      ┌─ Scheduling Entities ─────┐
│                        │      │                           │
│  ┌────────────────┐    │      │   ┌────────────────┐      │
│  │Source Assignment    │      │   │ Task Scheduler │      │
│  └────────────────┘    │      │   └────────────────┘      │
│          │             │      │          │                │
│          ▼             │      │          ▼                │
│  ┌────────────────┐    │      │   ┌────────────────┐      │
│  │Destination     │    │      │   │ Scheduled Flow │      │
│  │Assignment      │    │      │   └────────────────┘      │
│  └────────────────┘    │      │                           │
│                        │      │                           │
└────────────────────────┘      └───────────────────────────┘
```

### 2.1.1 Common Entity Properties

All entities in the FlowOrchestrator system share certain standard properties:

**Version Properties**:
- **Version**: Semantic version number (MAJOR.MINOR.PATCH)
- **CreatedTimestamp**: When the entity was first created
- **LastModifiedTimestamp**: When the entity was last modified
- **VersionDescription**: Human-readable description of this version
- **PreviousVersionId**: Reference to the previous version (if applicable)
- **VersionStatus**: ACTIVE, DEPRECATED, or ARCHIVED

**Identifier Properties**:
- Unique identifier within its entity type
- Hierarchical naming for related entities
- Version-aware identification

**Configuration Properties**:
- Configuration schema version
- Validation rules
- Default values

## 2.2 Flow Model

### 2.2.1 Processing Chain Entity
- **Definition**: A directed acyclic graph of processor services that defines data transformation logic
- **Properties**:
  - Contains only processor services with no importers or exporters
  - Can start with one or multiple entry points to allow direct branching from the importer
  - Can split into multiple parallel sub-flow processes (branches) at any point for parallel processing
  - Each sub-flow process can further split into additional parallel branches
  - Once a flow process splits, the resulting branches cannot merge back together within the processing chain
  - Each branch must flow independently to its termination point
  - No partial merging of branches is permitted at any stage within the processing chain
  - Ensures a strict forward-only data flow with no convergence points
- **Purpose**: Enables complex but deterministic transformation logic with predictable data flow paths

### 2.2.2 Flow Entity
- **Definition**: A complete end-to-end data pipeline connecting sources to destinations
- **Properties**:
  - Begins with exactly one importer service as the entry point
  - Connected to at least one processing chain for data transformation
  - A single importer can connect directly to multiple parallel processing chains (branches) without requiring an intermediate processor
  - Every processing chain branch must terminate, either by:
    - Connecting directly to its own dedicated exporter service, or
    - Merging with other branches at a single exporter service
  - Multiple branches from different processing chains can converge at a single exporter
  - All data paths must be terminated with an exporter - no unterminated branches allowed
  - Enforces complete data flow from source to destination with no "dead ends"
- **Branch Origination Clarification**:
  - Branches can originate in two ways:
    1. Directly from the importer: Multiple processors connected directly to one importer
    2. After a processor: One processor output feeding multiple downstream processors
  - In both cases, branches are explicitly defined during flow configuration
  - Branch paths are assigned during flow configuration and used in memory addressing
- **Merge Clarification**:
  - Branches cannot merge within a processing chain
  - Branches can only merge at exporters, which are not part of processing chains
  - Exporters must implement merge capabilities to accept multiple branches
  - Merge strategies are configured at the exporter level
- **Purpose**: Creates complete, executable data processing pipelines with well-defined input and output points

### 2.2.3 Flow Entity Structure Example

The following diagram illustrates a Flow Entity with multiple branches and merge points:

```
                        ┌───────────────┐
                        │  Importer     │
                        │  Service      │
                        └───────┬───────┘
                                │
                    ┌───────────┴────────────┐
                    │                        │
          ┌─────────▼────────┐    ┌──────────▼───────┐
          │  Processor A1    │    │  Processor B1    │
          │  (Branch A)      │    │  (Branch B)      │
          └─────────┬────────┘    └──────────┬───────┘
                    │                        │
          ┌─────────▼────────┐    ┌──────────▼───────┐
          │  Processor A2    │    │  Processor B2    │
          │  (Branch A)      │    │  (Branch B)      │
          └─────────┬────────┘    └──────────┬───────┘
                    │                        │
                    └────────────┬───────────┘
                                 │
                       ┌─────────▼─────────┐
                       │  Exporter         │
                       │  Service          │
                       │  (with merge      │
                       │   strategy)       │
                       └───────────────────┘
```

## 2.3 Connection Model

### 2.3.1 Source Entity
- **Definition**: Abstract interface defining a data source location and access protocol
- **Properties**:
  - Defines contract for source address information
  - Specifies interface for connection protocol definition
  - Provides abstraction for data retrieval protocol
  - Ensures address uniqueness within a version context (protocol + address + version must be unique)
  - Includes validation interfaces for configuration parameters
- **Purpose**: Provides a standardized abstraction for all source types

### 2.3.2 Destination Entity
- **Definition**: Abstract interface defining a data destination location and delivery protocol
- **Properties**:
  - Defines contract for destination address information
  - Specifies interface for delivery protocol definition
  - Provides abstraction for data delivery protocol
  - Ensures address uniqueness within a version context (protocol + address + version must be unique)
  - Includes validation interfaces for configuration parameters
- **Purpose**: Provides a standardized abstraction for all destination types

### 2.3.3 Source Assignment Entity
- **Definition**: Abstract interface defining the relationship between source and importer
- **Properties**:
  - Defines contract for connecting a source entity to an importer service
  - Specifies interface for protocol compatibility validation
  - Provides abstraction for connection-specific configuration
  - Ensures uniqueness (each Source Entity can only be used in one Source Assignment)
  - Includes configuration validation interfaces
- **Purpose**: Provides a standardized abstraction for source-importer relationships

### 2.3.4 Destination Assignment Entity
- **Definition**: Abstract interface defining the relationship between destination and exporter
- **Properties**:
  - Defines contract for connecting a destination entity to an exporter service
  - Specifies interface for protocol compatibility validation
  - Provides abstraction for delivery-specific configuration
  - Ensures uniqueness (each Destination Entity can only be used in one Destination Assignment)
  - Includes configuration validation interfaces
- **Purpose**: Provides a standardized abstraction for destination-exporter relationships

## 2.4 Execution Model

### 2.4.1 Task Scheduler Entity
- **Definition**: Abstract interface defining an active scheduling component
- **Properties**:
  - Defines contract for timing and frequency specifications
  - Provides interface for execution parameters
  - Supports one-time and recurring execution patterns
  - Contains message construction capabilities for direct triggering
  - Includes timer management for periodic execution
  - Maintains lifecycle state (CREATED, INACTIVE, ACTIVE, PAUSED, FAILED, TERMINATED)
  - Stores execution context received from Orchestrator Service
  - Incorporates configuration validation interfaces
- **Control Relationship**:
  - Operates under the control of the Orchestrator Service
  - State transitions occur only through Orchestrator Service commands
  - Reports state changes back to Orchestrator Service
  - Does not participate in orchestration after triggering im
  ## 2.5 Entity Relationships and Uniqueness Constraints

### 2.5.1 Entity Uniqueness Chain
- Source Entity address (protocol + address) must be unique across all Source Entities within the same version
- Each Source Entity can be used in only one Source Assignment
- Each Source Assignment can be used in only one Scheduled Flow
- Destination Entity address (protocol + address) must be unique across all Destination Entities within the same version
- Each Destination Entity can be used in only one Destination Assignment
- Each Destination Assignment can be used in only one Scheduled Flow
- The combination of Source Assignment and Destination Assignment must be unique across all Scheduled Flows

### 2.5.2 Step Identification Requirements
- Each step within a flow must have a unique identifier
- Step identifiers must be unique even when using the same processor type multiple times
- Branch paths must be encoded in step identifiers to maintain uniqueness across parallel branches
- Hierarchical step identification enables clear parent-child relationships
- Step IDs are generated and assigned during flow configuration
- The Orchestrator Service references these IDs during execution

### 2.5.3 Configuration Validation Relationships
- Each entity must validate its own configuration
- Composite entities must validate compatibility between constituent configurations
- Service managers ensure configuration validity during registration and updates
- Orchestrator verifies configuration validity before execution
- Validation occurs at registration time and before execution
- Version compatibility is validated as part of configuration validation

### 2.5.4 Schema Compatibility Requirements
- **Processor Chain Schema Validation**:
  - Each processor defines input and output schemas using SchemaDefinition
  - Output schema of processor N must be compatible with input schema of processor N+1
  - Schema compatibility validation occurs during flow construction
  - Incompatible schema chains prevent flow deployment
- **Importer to Processor Schema**:
  - Importer output format must match first processor input schemas
  - ImportResult structure includes metadata and validation information
  - First processors validate incoming data against their input schemas
- **Processor to Exporter Schema**:
  - Last processor output schemas must be compatible with exporter requirements
  - ProcessingResult structure provides standardized output format
  - Exporters validate received data before delivery
- **Schema Evolution Support**:
  - Schema definitions include version information
  - Backward compatibility can be maintained through versioning
  - Schema changes require version increments
  - Validation includes version-specific schema checking

## 2.1 Entity Framework Overview

The FlowOrchestrator system is built on a comprehensive entity model that defines the relationships and interactions between various components. The entity model is organized into four main categories:

1. **Flow Entities**: Define the structure and behavior of data processing workflows
2. **Configuration Entities**: Define the locations and protocols for data sources and destinations
3. **Connection Entities**: Define the relationships between sources/destinations and importers/exporters
4. **Scheduling Entities**: Define when and how flows are executed

The following diagram illustrates the high-level relationships between these entity types:

```
┌─ Flow Entities ─────────┐      ┌─ Configuration Entities ─┐
│                         │      │                          │
│  ┌─────────────────┐    │      │   ┌────────────────┐     │
│  │ Processing Chain│    │      │   │  Source Entity │     │
│  └─────────────────┘    │      │   └────────────────┘     │
│          │              │      │          │               │
│          ▼              │      │          ▼               │
│  ┌─────────────────┐    │      │   ┌────────────────┐     │
│  │   Flow Entity   │    │      │   │Destination Entity    │
│  └─────────────────┘    │      │   └────────────────┘     │
│                         │      │                          │
└─────────────────────────┘      └──────────────────────────┘
          │                                 │
          │                                 │
          ▼                                 ▼
┌─ Connection Entities ──┐      ┌─ Scheduling Entities ─────┐
│                        │      │                           │
│  ┌────────────────┐    │      │   ┌────────────────┐      │
│  │Source Assignment    │      │   │ Task Scheduler │      │
│  └────────────────┘    │      │   └────────────────┘      │
│          │             │      │          │                │
│          ▼             │      │          ▼                │
│  ┌────────────────┐    │      │   ┌────────────────┐      │
│  │Destination     │    │      │   │ Scheduled Flow │      │
│  │Assignment      │    │      │   └────────────────┘      │
│  └────────────────┘    │      │                           │
│                        │      │                           │
└────────────────────────┘      └───────────────────────────┘
```

### 2.1.1 Common Entity Properties

All entities in the FlowOrchestrator system share certain standard properties:

**Version Properties**:
- **Version**: Semantic version number (MAJOR.MINOR.PATCH)
- **CreatedTimestamp**: When the entity was first created
- **LastModifiedTimestamp**: When the entity was last modified
- **VersionDescription**: Human-readable description of this version
- **PreviousVersionId**: Reference to the previous version (if applicable)
- **VersionStatus**: ACTIVE, DEPRECATED, or ARCHIVED

**Identifier Properties**:
- Unique identifier within its entity type
- Hierarchical naming for related entities
- Version-aware identification

**Configuration Properties**:
- Configuration schema version
- Validation rules
- Default values

## 2.2 Flow Model

### 2.2.1 Processing Chain Entity
- **Definition**: A directed acyclic graph of processor services that defines data transformation logic
- **Properties**:
  - Contains only processor services with no importers or exporters
  - Can start with one or multiple entry points to allow direct branching from the importer
  - Can split into multiple parallel sub-flow processes (branches) at any point for parallel processing
  - Each sub-flow process can further split into additional parallel branches
  - Once a flow process splits, the resulting branches cannot merge back together within the processing chain
  - Each branch must flow independently to its termination point
  - No partial merging of branches is permitted at any stage within the processing chain
  - Ensures a strict forward-only data flow with no convergence points
- **Purpose**: Enables complex but deterministic transformation logic with predictable data flow paths

### 2.2.2 Flow Entity
- **Definition**: A complete end-to-end data pipeline connecting sources to destinations
- **Properties**:
  - Begins with exactly one importer service as the entry point
  - Connected to at least one processing chain for data transformation
  - A single importer can connect directly to multiple parallel processing chains (branches) without requiring an intermediate processor
  - Every processing chain branch must terminate, either by:
    - Connecting directly to its own dedicated exporter service, or
    - Merging with other branches at a single exporter service
  - Multiple branches from different processing chains can converge at a single exporter
  - All data paths must be terminated with an exporter - no unterminated branches allowed
  - Enforces complete data flow from source to destination with no "dead ends"
- **Branch Origination Clarification**:
  - Branches can originate in two ways:
    1. Directly from the importer: Multiple processors connected directly to one importer
    2. After a processor: One processor output feeding multiple downstream processors
  - In both cases, branches are explicitly defined during flow configuration
  - Branch paths are assigned during flow configuration and used in memory addressing
- **Merge Clarification**:
  - Branches cannot merge within a processing chain
  - Branches can only merge at exporters, which are not part of processing chains
  - Exporters must implement merge capabilities to accept multiple branches
  - Merge strategies are configured at the exporter level
- **Purpose**: Creates complete, executable data processing pipelines with well-defined input and output points

### 2.2.3 Flow Entity Structure Example

The following diagram illustrates a Flow Entity with multiple branches and merge points:

```
                        ┌───────────────┐
                        │  Importer     │
                        │  Service      │
                        └───────┬───────┘
                                │
                    ┌───────────┴────────────┐
                    │                        │
          ┌─────────▼────────┐    ┌──────────▼───────┐
          │  Processor A1    │    │  Processor B1    │
          │  (Branch A)      │    │  (Branch B)      │
          └─────────┬────────┘    └──────────┬───────┘
                    │                        │
          ┌─────────▼────────┐    ┌──────────▼───────┐
          │  Processor A2    │    │  Processor B2    │
          │  (Branch A)      │    │  (Branch B)      │
          └─────────┬────────┘    └──────────┬───────┘
                    │                        │
                    └────────────┬───────────┘
                                 │
                       ┌─────────▼─────────┐
                       │  Exporter         │
                       │  Service          │
                       │  (with merge      │
                       │   strategy)       │
                       └───────────────────┘
```

## 2.3 Connection Model

### 2.3.1 Source Entity
- **Definition**: Abstract interface defining a data source location and access protocol
- **Properties**:
  - Defines contract for source address information
  - Specifies interface for connection protocol definition
  - Provides abstraction for data retrieval protocol
  - Ensures address uniqueness within a version context (protocol + address + version must be unique)
  - Includes validation interfaces for configuration parameters
- **Purpose**: Provides a standardized abstraction for all source types

### 2.3.2 Destination Entity
- **Definition**: Abstract interface defining a data destination location and delivery protocol
- **Properties**:
  - Defines contract for destination address information
  - Specifies interface for delivery protocol definition
  - Provides abstraction for data delivery protocol
  - Ensures address uniqueness within a version context (protocol + address + version must be unique)
  - Includes validation interfaces for configuration parameters
- **Purpose**: Provides a standardized abstraction for all destination types

### 2.3.3 Source Assignment Entity
- **Definition**: Abstract interface defining the relationship between source and importer
- **Properties**:
  - Defines contract for connecting a source entity to an importer service
  - Specifies interface for protocol compatibility validation
  - Provides abstraction for connection-specific configuration
  - Ensures uniqueness (each Source Entity can only be used in one Source Assignment)
  - Includes configuration validation interfaces
- **Purpose**: Provides a standardized abstraction for source-importer relationships

### 2.3.4 Destination Assignment Entity
- **Definition**: Abstract interface defining the relationship between destination and exporter
- **Properties**:
  - Defines contract for connecting a destination entity to an exporter service
  - Specifies interface for protocol compatibility validation
  - Provides abstraction for delivery-specific configuration
  - Ensures uniqueness (each Destination Entity can only be used in one Destination Assignment)
  - Includes configuration validation interfaces
- **Purpose**: Provides a standardized abstraction for destination-exporter relationships

## 2.4 Execution Model

### 2.4.1 Task Scheduler Entity
- **Definition**: Abstract interface defining an active scheduling component
- **Properties**:
  - Defines contract for timing and frequency specifications
  - Provides interface for execution parameters
  - Supports one-time and recurring execution patterns
  - Contains message construction capabilities for direct triggering
  - Includes timer management for periodic execution
  - Maintains lifecycle state (CREATED, INACTIVE, ACTIVE, PAUSED, FAILED, TERMINATED)
  - Stores execution context received from Orchestrator Service
  - Incorporates configuration validation interfaces
- **Control Relationship**:
  - Operates under the control of the Orchestrator Service
  - State transitions occur only through Orchestrator Service commands
  - Reports state changes back to Orchestrator Service
  - Does not participate in orchestration after triggering importer
- **Purpose**: Manages execution triggers for flow initiation based on timing parameters

### 2.4.2 Scheduled Flow Entity
- **Definition**: Abstract interface defining a complete executable flow unit
- **Properties**:
  - Defines contract for combining source assignment, destination assignment, flow entity, and task scheduler
  - Provides interface for execution status tracking
  - Specifies contract for flow execution parameters
  - Contains configuration for task scheduler activation
  - Ensures uniqueness (each Source Assignment and each Destination Assignment can only be used in one Scheduled Flow)
  - Includes configuration validation across all connected components
- **Purpose**: Standardizes the definition of a complete runnable process

### 2.4.3 Branch Execution Context
- Maintained by the Orchestrator Service for each active flow
- Created at flow execution start, before any branching occurs
- Tracks branch status (NEW, IN_PROGRESS, COMPLETED, FAILED)
- Records completed and pending steps within each branch
- Manages branch dependencies and merge points
- Contains branch-specific configuration and parameters
- Supports direct branching from importer to multiple initial processors
- Tracks service lifecycle states for all branch components
- Includes error handling context for branch-specific failures
- Manages branch priority for resource allocation
- Maintains branch-specific execution statistics
- Supports branch-level timeouts and circuit breakers
- Used to ensure proper branch isolation during parallel execution
- Referenced when applying merge strategies at exporters
- Includes version information for all component versions involved in branch execution

### 2.4.4. Memory Addressing

The FlowOrchestrator system uses a hierarchical memory addressing scheme to ensure proper data isolation and flow:

- **Address Format**: `{executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}`
- **Example**: `EXEC-123:FLOW-001:PROCESS:branchA:2:ProcessingResult:TransformedData`
- **Components**:
  - **executionId**: Unique identifier for the specific execution instance
  - **flowId**: Identifier of the flow being executed
  - **stepType**: Type of step (IMPORT, PROCESS, EXPORT)
  - **branchPath**: Path of the branch (e.g., main, branchA, branchB)
  - **stepId**: Position within the branch
  - **dataType**: Type of data being stored
  - **component**: Optional sub-component of a return type

This addressing scheme enables:
- Clear isolation between different executions
- Separation between parallel branches
- Precise targeting of specific data components
- Traceability of data flow through the system
- Controlled memory lifecycle management