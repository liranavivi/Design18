# FlowOrchestrator.FileExporter

A worker service for exporting data to file destinations in the FlowOrchestrator system.

## Overview

The `FlowOrchestrator.FileExporter` service is designed to:

1. Export data to various file formats (JSON, XML, CSV, text, binary)
2. Support different file system operations through the file protocol
3. Handle format conversion between different data formats
4. Support various merge strategies for combining data from multiple branches

## Key Components

### FileExporterService

The `FileExporterService` is the core component of this service. It extends the `ExporterServiceBase` class and implements:

- File protocol handling through the `FileProtocolAdapter`
- File writing logic for different formats
- Format conversion between different data formats
- Error handling and recovery mechanisms
- Metrics collection for monitoring and analytics

### Worker

The `Worker` class is a background service that:

- Initializes and manages the `FileExporterService`
- Handles the service lifecycle (start, stop, etc.)
- Processes export commands from the message bus
- Manages error handling and recovery at the service level

## Configuration

The service is configured through the `appsettings.json` file. Here's an example configuration:

```json
{
  "FileExporter": {
    "ServiceId": "FILE-EXPORTER-001",
    "ServiceType": "FileExporter",
    "Protocol": "file",
    "ConnectionTimeoutSeconds": 30,
    "OperationTimeoutSeconds": 60,
    "MaxRetryCount": 3,
    "RetryDelayMilliseconds": 1000,
    "UseExponentialBackoff": true,
    "BatchSize": 1000,
    "ValidateData": true,
    "MergeStrategy": "append",
    "File": {
      "BasePath": "C:\\Data\\Export",
      "FilePattern": "*.*",
      "Recursive": false,
      "Encoding": "utf-8",
      "BufferSize": 4096
    }
  }
}
```

### Configuration Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| ServiceId | Unique identifier for the service | FILE-EXPORTER-001 |
| ServiceType | Type of the service | FileExporter |
| Protocol | Protocol used by the service | file |
| ConnectionTimeoutSeconds | Timeout for connection operations | 30 |
| OperationTimeoutSeconds | Timeout for export operations | 60 |
| MaxRetryCount | Maximum number of retry attempts | 3 |
| RetryDelayMilliseconds | Delay between retry attempts | 1000 |
| UseExponentialBackoff | Whether to use exponential backoff for retries | true |
| BatchSize | Maximum number of records to export in a single batch | 1000 |
| ValidateData | Whether to validate data before exporting | true |
| MergeStrategy | Strategy for merging data from multiple branches | append |
| File.BasePath | Base path for file operations | C:\\Data\\Export |
| File.FilePattern | Pattern for file matching | *.* |
| File.Recursive | Whether to search subdirectories | false |
| File.Encoding | Encoding for text files | utf-8 |
| File.BufferSize | Buffer size for file operations | 4096 |

## Usage

The service is designed to be used as part of the FlowOrchestrator system. It receives export commands from the message bus and exports data to file destinations.

### Export Parameters

When sending an export command to the service, you can specify the following parameters:

- `DestinationId`: Identifier for the destination
- `DestinationLocation`: Location of the destination (file path)
- `DestinationConfiguration`: Additional configuration for the destination
  - `filePath`: Path to the file to write
  - `fileFormat`: Format of the file (e.g., application/json, text/csv)
  - `encoding`: Encoding to use for text files
  - `overwrite`: Whether to overwrite existing files
  - `createDirectory`: Whether to create directories if they don't exist

### Supported File Formats

The service supports the following file formats:

- JSON (application/json)
- XML (application/xml)
- CSV (text/csv)
- Plain text (text/plain)
- Binary (application/octet-stream)

### Merge Strategies

The service supports the following merge strategies:

- `append`: Append data from all branches
- `replace`: Replace existing data with new data
- `merge`: Merge data from all branches based on keys

## Dependencies

- `FlowOrchestrator.ExporterBase`: Base implementation for exporter services
- `FlowOrchestrator.ProtocolAdapters`: Protocol adapters for different protocols
- `FlowOrchestrator.Abstractions`: Core interfaces and abstract classes
- `FlowOrchestrator.Common`: Common utilities and helpers
