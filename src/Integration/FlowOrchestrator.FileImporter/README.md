# FlowOrchestrator.FileImporter

A worker service for importing data from file sources in the FlowOrchestrator system.

## Overview

The `FlowOrchestrator.FileImporter` service is designed to:

1. Import data from various file sources (JSON, XML, CSV, text, binary)
2. Parse and validate the imported data
3. Create standardized data packages for further processing
4. Support various file system operations through the file protocol

## Key Components

### FileImporterService

The `FileImporterService` is the core component of this service. It extends the `ImporterServiceBase` class and implements:

- File-specific import operations
- File format detection and parsing
- Metadata generation for imported files
- Error handling and classification for file operations

### FileParserUtility

The `FileParserUtility` provides functionality for parsing different file formats:

- JSON parsing
- XML parsing
- CSV parsing
- Text handling
- Binary data handling

## Configuration

The service can be configured through the `appsettings.json` file:

```json
{
  "FileImporter": {
    "ServiceId": "FILE-IMPORTER-001",
    "ServiceType": "FileImporter",
    "Protocol": "file",
    "ConnectionTimeoutSeconds": 30,
    "OperationTimeoutSeconds": 60,
    "MaxRetryCount": 3,
    "RetryDelayMilliseconds": 1000,
    "File": {
      "BasePath": "C:\\Data\\Import",
      "FilePattern": "*.json",
      "Recursive": "true",
      "Encoding": "utf-8"
    }
  }
}
```

### Configuration Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| ServiceId | Unique identifier for the service | FILE-IMPORTER-001 |
| ServiceType | Type of the service | FileImporter |
| Protocol | Protocol used by the service | file |
| ConnectionTimeoutSeconds | Timeout for connection operations | 30 |
| OperationTimeoutSeconds | Timeout for import operations | 60 |
| MaxRetryCount | Maximum number of retry attempts | 3 |
| RetryDelayMilliseconds | Delay between retry attempts | 1000 |
| File.BasePath | Base path for file operations | - |
| File.FilePattern | Pattern for file matching | *.* |
| File.Recursive | Whether to search subdirectories | false |
| File.Encoding | Encoding for text files | utf-8 |

## Usage

The service can be used in two ways:

1. As a standalone worker service that listens for import commands
2. As a library that can be integrated into other services

### Standalone Service

To run as a standalone service:

```bash
dotnet run --project src/Integration/FlowOrchestrator.FileImporter/FlowOrchestrator.FileImporter.csproj
```

### Library Integration

To use as a library:

```csharp
// Create and configure the service
var fileImporter = new FileImporterService(logger);
fileImporter.Initialize(configParams);

// Create import parameters
var parameters = new ImportParameters
{
    SourceId = "file-source-001",
    SourceLocation = "C:\\Data\\Import",
    SourceConfiguration = new Dictionary<string, object>
    {
        { "filePath", "data.json" },
        { "encoding", "utf-8" }
    }
};

// Execute the import operation
var result = fileImporter.Import(parameters, new ExecutionContext());

// Process the result
if (result.Success)
{
    var data = result.Data;
    // Process the imported data
}
```

## Dependencies

- FlowOrchestrator.Abstractions
- FlowOrchestrator.Common
- FlowOrchestrator.ImporterBase
- FlowOrchestrator.ProtocolAdapters

## Error Handling

The service provides detailed error information for various file-related errors:

- FILE_NOT_FOUND: File not found
- DIRECTORY_NOT_FOUND: Directory not found
- IO_ERROR: I/O error
- ACCESS_DENIED: Access denied
- INVALID_JSON_FORMAT: Invalid JSON format
- INVALID_OPERATION: Invalid operation
- INVALID_ARGUMENT: Invalid argument
- GENERAL_ERROR: General error
