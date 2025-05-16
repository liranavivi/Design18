# FlowOrchestrator.MonitoringFramework

The FlowOrchestrator.MonitoringFramework is a Web API that provides comprehensive monitoring capabilities for the FlowOrchestrator system. It offers health check endpoints, system status dashboard, and resource utilization monitoring.

## Features

- **Health Check Endpoints**: Monitor the health of all system components
- **System Status Dashboard**: Provides a comprehensive view of the system's status
- **Resource Utilization Monitoring**: Tracks CPU, memory, disk, and network usage
- **Flow Execution Monitoring**: Tracks currently executing flows and their status
- **Performance Anomaly Detection**: Identifies performance anomalies in real-time
- **Alert Management**: Manages and dispatches system alerts

## API Endpoints

### Health Checks

- `GET /api/healthcheck`: Get the health status of all services
- `GET /api/healthcheck/service/{serviceId}`: Get the health status of a specific service
- `GET /api/healthcheck/discover`: Discover available services
- `POST /api/healthcheck/register`: Register a service for health monitoring
- `POST /api/healthcheck/unregister`: Unregister a service from health monitoring

### Resource Utilization

- `GET /api/resourceutilization`: Get the current resource utilization
- `GET /api/resourceutilization/history`: Get the resource utilization history
- `GET /api/resourceutilization/service/{serviceId}`: Get the resource utilization for a specific service
- `GET /api/resourceutilization/thresholds`: Get the resource utilization thresholds
- `POST /api/resourceutilization/thresholds`: Set a resource utilization threshold

### Flow Monitoring

- `GET /api/flowmonitoring`: Get all active flows
- `GET /api/flowmonitoring/{flowId}/{executionId}`: Get an active flow by ID
- `GET /api/flowmonitoring/status/{status}`: Get active flows by status
- `GET /api/flowmonitoring/{flowId}/history`: Get flow execution history
- `GET /api/flowmonitoring/{flowId}/statistics`: Get flow execution statistics
- `POST /api/flowmonitoring/register`: Register a flow execution for monitoring
- `POST /api/flowmonitoring/{flowId}/{executionId}/update`: Update a flow execution status

### Alerts

- `GET /api/alerts`: Get all alerts
- `GET /api/alerts/severity/{severity}`: Get alerts by severity
- `GET /api/alerts/category/{category}`: Get alerts by category
- `GET /api/alerts/source/{source}`: Get alerts by source
- `GET /api/alerts/{alertId}`: Get an alert by ID
- `POST /api/alerts`: Create an alert
- `POST /api/alerts/{alertId}/acknowledge`: Acknowledge an alert
- `POST /api/alerts/{alertId}/resolve`: Resolve an alert
- `POST /api/alerts/detect/resource`: Detect resource anomalies
- `POST /api/alerts/detect/flow`: Detect flow execution anomalies

### System Status

- `GET /api/systemstatus/dashboard`: Get the system status dashboard data
- `GET /api/systemstatus/overview`: Get the system overview

## Configuration

The monitoring framework can be configured using the `appsettings.json` file:

```json
"Monitoring": {
  "ServiceId": "FlowOrchestrator.MonitoringFramework",
  "Version": "1.0.0",
  "HealthCheckIntervalSeconds": 30,
  "ResourceMonitoringIntervalSeconds": 15,
  "FlowMonitoringIntervalSeconds": 10,
  "AnomalyDetectionIntervalSeconds": 60,
  "AlertCheckIntervalSeconds": 30,
  "DataRetentionDays": 30,
  "CpuUsageThresholdPercent": 80.0,
  "MemoryUsageThresholdPercent": 80.0,
  "DiskUsageThresholdPercent": 85.0,
  "EnableAutoDiscovery": true,
  "ServiceDiscoveryEndpoints": [
    "http://localhost:5000/api/services",
    "http://localhost:5001/api/services"
  ]
}
```

## Running the API

To run the FlowOrchestrator.MonitoringFramework API, you can use the following command:

```bash
dotnet run --project src/Observability/FlowOrchestrator.MonitoringFramework/FlowOrchestrator.MonitoringFramework.csproj
```

The API will be available at `http://localhost:5280` by default.

## Dependencies

- FlowOrchestrator.Abstractions
- FlowOrchestrator.Common
- FlowOrchestrator.Infrastructure.Telemetry
- ASP.NET Core Health Checks
- OpenTelemetry
