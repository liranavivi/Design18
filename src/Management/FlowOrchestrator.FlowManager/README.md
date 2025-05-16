# FlowOrchestrator.FlowManager

## Overview

The FlowOrchestrator.FlowManager is a Web API that manages flow definitions and configurations in the FlowOrchestrator system. It provides endpoints for creating, updating, validating, and versioning flow entities and processing chains.

## Key Components

### Flow Definition API

The Flow Definition API provides endpoints for managing flow entities, including:

- Creating new flow entities
- Retrieving flow entities by ID
- Listing all flow entities
- Updating flow entities
- Deleting flow entities
- Validating flow entities
- Creating new versions of flow entities

### Flow Validation Logic

The Flow Validation Logic ensures that flow entities are valid and well-formed, including:

- Validating flow structure (topology)
- Ensuring that flows have exactly one importer
- Verifying that all processors are connected
- Checking that all branches terminate at exporters
- Validating that there are no cycles in the flow graph

### Flow Versioning

The Flow Versioning component manages versions of flow entities, including:

- Creating new versions of flow entities
- Retrieving specific versions of flow entities
- Listing all versions of a flow entity

### Processing Chain Management

The Processing Chain Management component manages processing chains, including:

- Creating new processing chains
- Retrieving processing chains by ID
- Listing all processing chains
- Updating processing chains
- Deleting processing chains
- Creating new versions of processing chains

## API Endpoints

### Flow Entity Endpoints

- `GET /api/FlowEntity`: Get all flow entities
- `GET /api/FlowEntity/{id}`: Get a flow entity by ID
- `POST /api/FlowEntity`: Create a new flow entity
- `DELETE /api/FlowEntity/{id}`: Delete a flow entity
- `GET /api/FlowEntity/{id}/validate`: Validate a flow entity
- `GET /api/FlowEntity/{id}/versions`: Get all versions of a flow entity
- `POST /api/FlowEntity/{id}/versions`: Create a new version of a flow entity

### Processing Chain Endpoints

- `GET /api/ProcessingChain`: Get all processing chains
- `GET /api/ProcessingChain/{id}`: Get a processing chain by ID
- `POST /api/ProcessingChain`: Create a new processing chain
- `DELETE /api/ProcessingChain/{id}`: Delete a processing chain
- `GET /api/ProcessingChain/{id}/versions`: Get all versions of a processing chain
- `POST /api/ProcessingChain/{id}/versions`: Create a new version of a processing chain

## Dependencies

- **FlowOrchestrator.Abstractions**: Core interfaces and abstract classes that define the system's contract
- **FlowOrchestrator.Domain**: Domain models and entities for the system
- **FlowOrchestrator.Common**: Common utilities and helpers

## Usage

To use the FlowOrchestrator.FlowManager API, you can make HTTP requests to the endpoints described above. For example:

```http
# Create a new flow entity
POST http://localhost:5219/api/FlowEntity
Content-Type: application/json

{
  "flowId": "flow-001",
  "name": "Sample Flow",
  "description": "A sample flow for demonstration purposes",
  "importerServiceId": "file-importer-001",
  "processorServiceIds": ["json-processor-001"],
  "exporterServiceIds": ["file-exporter-001"],
  "isEnabled": true,
  "serviceId": "flow-001",
  "serviceType": "FLOW",
  "version": "1.0.0"
}
```

## Configuration

The FlowOrchestrator.FlowManager can be configured using the following settings:

- **StorageType**: The type of storage to use for flow entities and processing chains (default: "InMemory")
- **EnableValidation**: Whether to enable validation of flow entities and processing chains (default: true)
- **EnableVersioning**: Whether to enable versioning of flow entities and processing chains (default: true)

## Running the API

To run the FlowOrchestrator.FlowManager API, you can use the following command:

```bash
dotnet run --project src/Management/FlowOrchestrator.FlowManager/FlowOrchestrator.FlowManager.csproj
```

The API will be available at http://localhost:5219 by default.
