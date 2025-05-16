# FlowOrchestrator.ConfigurationManager

## Overview

The FlowOrchestrator.ConfigurationManager is a Web API that manages system and service configurations in the FlowOrchestrator system. It provides endpoints for creating, updating, validating, and managing configuration entries and schemas.

## Key Components

### Configuration API

The Configuration API provides endpoints for managing configuration entries, including:

- Creating new configuration entries
- Retrieving configuration entries by ID
- Listing all configuration entries
- Filtering configuration entries by scope, target ID, or environment
- Updating configuration entries
- Deleting configuration entries
- Activating and deactivating configuration entries

### Configuration Validation

The Configuration Validation component ensures that configuration entries are valid and well-formed, including:

- Schema-based validation
- Required parameter validation
- Type checking
- Range validation
- Pattern matching
- Enumeration validation

### Environment-Specific Settings

The Environment-Specific Settings component allows for different configurations based on the deployment environment, including:

- Environment-specific configuration entries
- Environment variable substitution
- Environment-based configuration overrides

## Configuration

The FlowOrchestrator.ConfigurationManager can be configured using the following settings in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConfigurationManager": {
    "StorageType": "InMemory",
    "EnableValidation": true,
    "DefaultEnvironment": "Development"
  }
}
```

## API Endpoints

### Configuration Endpoints

- `GET /api/Configuration` - Get all configuration entries
- `GET /api/Configuration/{id}` - Get a configuration entry by ID
- `GET /api/Configuration/scope/{scope}` - Get configuration entries by scope
- `GET /api/Configuration/target/{targetId}` - Get configuration entries by target ID
- `GET /api/Configuration/environment/{environment}` - Get configuration entries by environment
- `POST /api/Configuration` - Create a new configuration entry
- `PUT /api/Configuration/{id}` - Update a configuration entry
- `DELETE /api/Configuration/{id}` - Delete a configuration entry
- `POST /api/Configuration/{id}/activate` - Activate a configuration entry
- `POST /api/Configuration/{id}/deactivate` - Deactivate a configuration entry

### Schema Endpoints

- `GET /api/Schema` - Get all configuration schemas
- `GET /api/Schema/{id}` - Get a configuration schema by ID
- `POST /api/Schema` - Create a new configuration schema
- `PUT /api/Schema/{id}` - Update a configuration schema
- `DELETE /api/Schema/{id}` - Delete a configuration schema
- `POST /api/Schema/validate` - Validate configuration values against a schema

## Running the API

To run the FlowOrchestrator.ConfigurationManager API, you can use the following command:

```bash
dotnet run --project src/Management/FlowOrchestrator.ConfigurationManager/FlowOrchestrator.ConfigurationManager.csproj
```

The API will be available at http://localhost:5115 by default.

## Example Usage

### Creating a Configuration Schema

```http
POST /api/Schema
Content-Type: application/json

{
  "name": "FileImporterConfiguration",
  "description": "Schema for file importer configuration",
  "version": "1.0.0",
  "parameters": [
    {
      "name": "BasePath",
      "description": "Base path for file operations",
      "type": "String",
      "required": true
    },
    {
      "name": "FilePattern",
      "description": "Pattern for matching files",
      "type": "String",
      "required": true,
      "defaultValue": "*.*"
    },
    {
      "name": "Recursive",
      "description": "Whether to search subdirectories",
      "type": "Boolean",
      "required": false,
      "defaultValue": false
    }
  ]
}
```

### Creating a Configuration Entry

```http
POST /api/Configuration
Content-Type: application/json

{
  "name": "FileImporter001Config",
  "description": "Configuration for File Importer 001",
  "version": "1.0.0",
  "scope": "Service",
  "targetId": "FILE-IMPORTER-001",
  "environment": "Development",
  "values": {
    "BasePath": "C:\\Data\\Import",
    "FilePattern": "*.csv",
    "Recursive": true
  },
  "schemaId": "schema-id-from-previous-response"
}
```

### Validating Configuration Values

```http
POST /api/Schema/validate
Content-Type: application/json

{
  "schemaId": "schema-id-from-previous-response",
  "values": {
    "BasePath": "C:\\Data\\Import",
    "FilePattern": "*.csv",
    "Recursive": true
  }
}
```
