# 5. Flow Definition and Execution

## 5.1 Flow Definition Framework

The Flow Definition Framework provides a standardized approach for creating, validating, and managing flow definitions within the FlowOrchestrator system.

### 5.1.1 Declarative Flow Definition Language

The Flow Definition Framework uses a declarative approach to flow definition, allowing clear specification of flow structure and behavior:

```json
{
  "flowDefinition": {
    "id": "FLOW-001",
    "version": "1.0.0",
    "description": "Customer data processing flow",
    "components": {
      "importer": {
        "serviceId": "REST-IMP-001",
        "version": "2.1.0",
        "configuration": {
          "endpoint": "${source.endpoint}",
          "authType": "OAUTH2"
        }
      },
      "processors": [
        {
          "serviceId": "JSON-PROC-001",
          "version": "1.2.0",
          "stepId": "main:1",
          "configuration": {
            "transformationRules": "customer-transform-rules"
          }
        },
        {
          "serviceId": "VALIDATION-PROC-001",
          "version": "2.0.1",
          "stepId": "main:2",
          "configuration": {
            "validationSchema": "customer-validation-schema"
          }
        }
      ],
      "exporter": {
        "serviceId": "DB-EXP-001",
        "version": "1.5.0",
        "configuration": {
          "connectionString": "${destination.connectionString}",
          "tableName": "customers"
        }
      }
    },
    "connections": {
      "importer": {
        "outputs": ["main:1"]
      },
      "processors": {
        "main:1": {
          "outputs": ["main:2"]
        },
        "main:2": {
          "outputs": ["exporter"]
        }
      }
    }
  }
}
```

Key features of the definition language include:
- Clear component identification with versioning
- Configuration parameter specification
- Connection topology definition
- Branch path identification
- Parameter substitution capabilities

### 5.1.2 Flow Structure Validation

Flow structure validation ensures that the defined flow meets all structural requirements:

- **Completeness Checks**:
  - Verifies that all required components are present
  - Ensures all components have valid identifiers and versions
  - Checks that all required configuration parameters are provided
  - Validates that all connection points are properly defined

- **Topology Validation**:
  - Ensures the flow has exactly one importer
  - Verifies that all processors are connected
  - Checks that all branches terminate at exporters
  - Validates that branch paths are properly defined
  - Ensures no cycles exist in the flow graph
  - Verifies that branches only merge at exporters

- **Component Compatibility**:
  - Validates that connected components are compatible
  - Verifies version compatibility between components
  - Ensures protocol compatibility for external connections
  - Checks that processors support required data types
  - Validates merge strategy support at exporters

- **Schema Compatibility Validation**:
  - **Processor Chain Schema Validation**:
    - Validates that output schema of processor N matches input schema of processor N+1
    - Ensures data type compatibility throughout the processing chain
    - Verifies field mapping and transformation requirements between processors
  - **Importer to Processor Schema Validation**:
    - Validates that importer output format matches the input schema of first processors
    - Ensures ImportResult structure is compatible with processor expectations
    - Verifies that all required fields from importer are available to processors
  - **Processor to Exporter Schema Validation**:
    - Validates that last processor output schemas match exporter input requirements
    - Ensures ProcessingResult structure is compatible with exporter expectations
    - Verifies that final transformation produces required fields for export
  - **Schema Validation Timing**:
    - Occurs during flow construction time validation
    - Prevents deployment of flows with incompatible data transformations
    - Provides detailed error messages for schema mismatches
    - Enables proactive identification of transformation issues

### 5.1.3 Processing Chain Composition

Processing chains are composed of one or more processor services arranged in a directed acyclic graph:

- **Chain Structure**:
  - Multiple entry points allowed for direct branching from importer
  - Branch points can create multiple parallel processing paths
  - Each branch follows an independent path to termination
  - No merging of branches within the processing chain
  - Each processor has a unique step identifier

- **Processor Configuration**:
  - Each processor has its own configuration
  - Configurations can include shared parameters
  - Parameter substitution allows dynamic configuration
  - Version-specific configuration parameters

- **Data Flow Management**:
  - Data types are validated between processors
  - Transformation rules ensure data compatibility
  - Error handling defined at processor level
  - Performance optimization through parallel processing

### 5.1.4 Branch Configuration

Branches are explicitly defined during flow configuration:

- **Branch Definition**:
  - Branch paths assigned during configuration
  - Each branch has a unique identifier
  - Branches originate from the importer or from processors
  - Branch topology defined in connection configuration

- **Branch Constraints**:
  - Branches cannot merge within a processing chain
  - All branches must terminate at an exporter
  - Multiple branches can merge at a single exporter
  - Exporters must support merge operations for multiple inputs

- **Branch Properties**:
  - Branches can have different priorities
  - Resource allocation can be branch-specific
  - Error handling can be customized per branch
  - Performance monitoring is branch-aware

### 5.1.5 Merge Strategy Configuration

Merge strategies are configured at exporter level to handle multiple branch inputs:

- **Available Strategies**:
  - Last-Write-Wins: Uses output from most recently completed branch
  - Priority-Based: Uses output from highest-priority branch
  - Field-Level: Creates merged output by combining fields from multiple branches

- **Strategy Configuration**:
  - Configured during flow definition
  - Strategy-specific parameters
  - Branch priority mapping for priority-based strategies
  - Field mapping for field-level strategies
  - Conflict resolution rules

Example merge strategy configuration:
```json
{
  "mergeStrategy": {
    "type": "FIELD_LEVEL",
    "version": "1.0.0",
    "configuration": {
      "fieldMappings": [
        {
          "targetField": "customer",
          "sourceBranch": "branchA",
          "sourceField": "customerInfo"
        },
        {
          "targetField": "order",
          "sourceBranch": "branchB",
          "sourceField": "orderData"
        },
        {
          "targetField": "shipping",
          "sourceBranch": "branchC",
          "sourceField": "shippingDetails"
        }
      ],
      "conflictResolution": {
        "type": "PRIORITY_BASED",
        "branchPriorities": ["branchA", "branchC", "branchB"]
      }
    }
  }
}
```

## 5.2 Flow Execution Engine

The Flow Execution Engine is responsible for running defined flows, managing their execution, and handling runtime behavior.

### 5.2.1 Execution Planning

Before a flow is executed, the Flow Execution Engine performs detailed planning:

- **Validation Phase**:
  - Verifies all components are available in required versions
  - Validates configuration completeness
  - Checks component lifecycle states
  - Verifies resource availability
  - Validates version compatibility

- **Resource Allocation**:
  - Allocates memory addresses for all steps
  - Assigns processing resources
  - Determines execution priorities
  - Prepares branch execution contexts
  - Configures monitoring instrumentation

- **Execution Strategy**:
  - Determines optimal execution path
  - Plans parallel execution opportunities
  - Configures branch execution
  - Sets up error handling strategies
  - Prepares recovery points

### 5.2.2 Branch Activation

The Flow Execution Engine manages branch activation during flow execution:

- **Initial Activation**:
  - Creates branch execution contexts for all branches
  - Activates initial processors in each branch
  - Sets up branch monitoring
  - Initializes branch-specific metrics
  - Prepares memory locations for branch data

- **Dynamic Branching**:
  - Manages branch activation during execution
  - Handles branch creation at split points
  - Ensures proper resource allocation
  - Maintains branch execution state
  - Coordinates parallel execution

- **Branch Lifecycle**:
  - Tracks branch execution progress
  - Manages branch completion
  - Handles branch-specific errors
  - Coordinates branch termination
  - Updates branch metrics

### 5.2.3 Memory Allocation

The Flow Execution Engine manages memory allocation for flow execution:

- **Addressing Scheme**:
  - Uses hierarchical addressing format
  - Format: `{executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}`
  - Ensures globally unique addresses
  - Supports branch-specific addressing
  - Enables precise data targeting

- **Allocation Strategy**:
  - Pre-allocates addresses for known steps
  - Dynamically allocates additional addresses as needed
  - Implements branch-specific memory isolation
  - Manages memory lifecycle (allocation, use, cleanup)
  - Optimizes allocation based on data size and access patterns

- **Memory Optimization**:
  - Implements cleanup of intermediate results when no longer needed
  - Uses reference counting for shared data
  - Provides memory usage statistics for monitoring
  - Supports memory pressure detection and handling
  - Implements memory recovery during error scenarios

### 5.2.4 Message Routing

The Flow Execution Engine manages the routing of messages between components:

- **Message Structure**:
  - Contains command type (READ, WRITE, PROCESS)
  - Includes source and destination memory addresses
  - Contains execution context information
  - Includes component-specific instructions
  - Contains error handling directives

- **Routing Logic**:
  - Based on flow topology
  - Follows branch paths for parallel execution
  - Maintains execution order within branches
  - Handles completion notifications
  - Manages error propagation

- **Message Optimization**:
  - Batches messages when possible
  - Prioritizes messages based on flow requirements
  - Implements backpressure mechanisms
  - Provides message tracking for monitoring
  - Supports message replay for recovery

### 5.2.5 Execution Monitoring

The Flow Execution Engine includes comprehensive monitoring capabilities:

- **Real-Time Monitoring**:
  - Tracks execution progress
  - Monitors component health
  - Tracks resource utilization
  - Measures performance metrics
  - Detects execution anomalies

- **Flow Visualization**:
  - Provides real-time flow execution views
  - Shows branch execution status
  - Highlights active components
  - Displays performance metrics
  - Shows error conditions

- **Execution Analytics**:
  - Collects performance statistics
  - Analyzes execution patterns
  - Identifies optimization opportunities
  - Compares against historical performance
  - Provides execution forecasting

## 5.3 Branch Management

Branch management is a core capability of the FlowOrchestrator system, enabling parallel processing paths and complex transformation logic.

### 5.3.1 Branch Identification

#### Hierarchical Step ID Structure
- Format: `{flowId}:{branchPath}:{stepPosition}`
- Examples:
  - `FLOW-001:main:1` - First processor in main branch
  - `FLOW-001:branchA:1` - First processor in branch A
  - `FLOW-001:branchB:1` - First processor in branch B (parallel to branch A)
- Nested branch format: `{flowId}:{parentBranch}.{childBranch}:{stepPosition}`
- Example: `FLOW-001:main.subBranchA:2` - Second processor in a sub-branch
- Step IDs are generated and assigned during flow configuration
- IDs remain fixed throughout the flow lifecycle
- Orchestrator Service uses these IDs for message routing and memory addressing
- Each ID is guaranteed to be unique within a flow

#### Branch Path Encoding
- Uniquely identifies a branch within a flow
- Encodes the full parent-child relationship chain
- Supports multiple levels of nesting
- Used in memory addressing and message routing
- Included in observability data for traceability
- Branch paths are explicitly defined during flow configuration
- Paths are used to enforce branch isolation
- The Orchestrator Service uses branch paths to route messages to appropriate processors

### 5.3.2 Branch Creation

Branch creation occurs at two distinct times:

#### Configuration-Time Branch Definition
- Branches are explicitly defined during flow configuration
- Branch paths are assigned as part of the flow structure
- The Processing Chain Service Manager validates branch configurations
- Ensures branch isolation and proper termination
- Rejects invalid branch configurations
- Branch validation occurs at registration time, not at runtime
- Branch configurations are versioned along with their containing processing chain

#### Execution-Time Branch Context Creation
- The Orchestrator Service creates branch execution contexts at flow execution start
- Initial branch contexts are created before any processing begins
- Branch contexts track the runtime state of each branch
- Contexts are updated as processing progresses
- Contexts are used for message routing and memory addressing
- Branch contexts are cleaned up after flow completion
- Version information for all components is included in branch execution contexts

### 5.3.3 Branch Isolation

Branch isolation is a core principle that ensures independent execution of parallel branches:

#### Memory Isolation
- Each branch has dedicated memory locations for its outputs
- Branch paths are encoded in memory addresses
- Branch-specific processors can only access their designated memory locations
- The Orchestrator Service controls memory access through message routing
- Memory isolation prevents unintended cross-branch interactions
- Memory addresses follow the pattern: `{executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}`
- Example: `EXEC-123:FLOW-001:PROCESS:branchA:2:ProcessingResult:TransformedData`

#### Error Isolation
- Errors in one branch do not affect execution in other branches
- Branch-specific error handling contexts track errors independently
- Recovery operations can be applied to specific branches
- The Orchestrator Service can implement branch-specific recovery strategies
- Error isolation enables partial success scenarios where some branches complete despite others failing
- Version-specific error handling can be applied within branches
- Branch error history is maintained for analysis and optimization
- Circuit breaker patterns can be applied at the branch level

#### Resource Isolation
- Branch execution can be prioritized independently
- Resources can be allocated to branches based on priority
- Branch execution can be paused or resumed selectively
- Resource isolation enables optimized execution of critical branches
- Version-specific resource requirements can be considered in resource allocation
- Branch-level statistics help optimize resource allocation
- Performance metrics are tracked at the branch level
- Resource constraints can be defined at the branch level

### 5.3.4 Branch Execution Context

The Branch Execution Context is maintained by the Orchestrator Service for each active branch:

- Created at flow execution start, before any branching occurs
- Tracks branch status (NEW, IN_PROGRESS, COMPLETED, FAILED)
- Records completed and pending steps within the branch
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

### 5.3.5 Branch Termination

Branch termination is a critical aspect of branch management:

#### Termination Requirements
- Every branch created within a flow must terminate at an exporter
- No "dead end" branches are permitted in the system
- Each branch must have a defined path to an exporter
- Branches may terminate at dedicated exporters or merge with other branches at a shared exporter

#### Termination Process
- When a branch completes its final step, it notifies the Orchestrator Service
- The Orchestrator Service updates the branch execution context
- Branch completion may trigger merge operations if multiple branches converge at an exporter
- The Orchestrator Service coordinates branch completion with exporter activation
- Statistics about branch execution are captured at termination
- Branch memory is cleaned up after termination (unless needed for merge operations)
- Branch completion events are published for monitoring and analytics

#### Termination Validation
- Branch termination is validated at multiple points:
  - The Processing Chain Service Manager validates that each branch has at least one potential termination path
  - The Flow Service Manager validates that all branches connect to an exporter
  - Flows with unterminated branches are rejected during validation
  - The Orchestrator Service verifies branch termination during execution

## 5.4 Merge Strategy Implementation

Merge strategies are implemented at exporter level to handle data from multiple branches.

### 5.4.1 Last-Write-Wins Strategy
- Selects output from the most recently completed branch
- Tracks completion timestamps for each branch
- Simple implementation suitable for independent branches
- Configuration requires no additional parameters
- Appropriate when branches contain equivalent but alternative transformations
- Versioned to allow strategy evolution over time
- Example scenario: Multiple data enrichment branches with similar output structures
- Conflict resolution relies on timestamps, not branch priority

### 5.4.2 Priority-Based Strategy
- Selects output from the highest-priority branch that completes successfully
- Branch priorities are configured during flow setup
- Falls back to lower-priority branches if higher-priority branches fail
- Appropriate when branches have clear preference ordering
- Configuration requires branch priority mapping
- Versioned to allow strategy evolution over time
- Example scenario: Primary and backup data processing paths
- Prioritization can be based on data quality, source reliability, or other factors

### 5.4.3 Field-Level Strategy
- Creates a merged output combining fields from multiple branches
- Field mapping configuration specifies which branch supplies each field
- Supports complex merging of complementary branch outputs
- Appropriate when branches process different aspects of the same data
- Configuration requires detailed field mapping specification
- Versioned to allow mapping evolution over time
- Example scenario: One branch processes customer data, another processes order data
- Field-level conflict resolution rules can be specified

### 5.4.4 Merge Strategy Configuration
- Merge strategies are configured at the exporter level
- Configuration occurs during flow definition, not at runtime
- The Flow Service Manager validates merge strategy compatibility
- Exporters must support the configured merge strategy
- Merge strategy execution is coordinated by the Orchestrator Service
- Merge strategies have version information to allow evolution over time
- Multiple merge strategies can be used in the same flow at different exporters
- Merge strategy selection considers branch output schemas

### 5.4.5 Merge Execution Process

The merge execution process follows these steps:

1. **Branch Completion Tracking**:
   - The Orchestrator Service tracks completion of all branches connected to the exporter
   - Branch outputs are stored at their designated memory locations
   - Branch completion timestamps are recorded

2. **Merge Trigger Determination**:
   - Merge operation is triggered based on configured policy:
     - All branches complete
     - Any branch completes (with others timed out)
     - Critical branches complete
     - Timeout reached

3. **Strategy Application**:
   - The configured merge strategy is applied to branch outputs
   - Strategy-specific logic determines the final result
   - Branch metadata is considered in the merge decision
   - Conflict resolution rules are applied as needed

4. **Result Generation**:
   - The merged result is created according to strategy rules
   - Result is placed at the exporter's input memory location
   - Branch completion information is included in result metadata
   - Merge operation statistics are captured

5. **Export Activation**:
   - The Orchestrator Service activates the exporter with the merged result
   - Exporter processes the merged data according to its configuration
   - Export completion is reported back to the Orchestrator Service
   - Flow execution continues or completes based on flow topology