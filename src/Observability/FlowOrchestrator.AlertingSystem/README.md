# FlowOrchestrator.AlertingSystem

The FlowOrchestrator.AlertingSystem is a Worker Service that generates alerts based on system events and thresholds. It provides notification capabilities through various channels and maintains an alert history.

## Features

- **Alert Rule Engine**: Evaluates metrics against defined thresholds and generates alerts
- **Notification Channels**: Sends notifications through email, webhooks, and SMS
- **Alert History**: Tracks alert history with acknowledgment and resolution capabilities
- **Metrics Collection**: Collects metrics from various sources for alert evaluation
- **Message Consumers**: Processes system events and metric threshold events

## Architecture

The AlertingSystem is designed as a background worker service that periodically checks metrics against defined alert rules. It also listens for system events and metric threshold events from other components of the FlowOrchestrator system.

### Core Components

- **AlertRuleEngineService**: Evaluates metrics against alert rules
- **NotificationService**: Sends notifications through various channels
- **AlertHistoryService**: Tracks alert history and manages alert lifecycle
- **MetricsCollectorService**: Collects metrics from various sources

### Message Consumers

- **SystemEventConsumer**: Processes system events and generates alerts
- **MetricThresholdEventConsumer**: Processes metric threshold events and generates alerts

## Configuration

The AlertingSystem can be configured using the `appsettings.json` file:

```json
"AlertingSystem": {
  "ServiceId": "FlowOrchestrator.AlertingSystem",
  "Version": "1.0.0",
  "HealthCheckIntervalSeconds": 60,
  "AlertCheckIntervalSeconds": 30,
  "DataRetentionDays": 30,
  "NotificationChannels": {
    "Email": {
      "Enabled": true,
      "SmtpServer": "smtp.example.com",
      "SmtpPort": 587,
      "SmtpUsername": "alerts@example.com",
      "SmtpPassword": "password",
      "FromAddress": "alerts@example.com",
      "DefaultRecipients": [
        "admin@example.com"
      ]
    },
    "Webhook": {
      "Enabled": true,
      "Endpoints": [
        "http://localhost:5000/api/notifications"
      ]
    },
    "Sms": {
      "Enabled": false,
      "Provider": "Twilio",
      "AccountSid": "",
      "AuthToken": "",
      "FromNumber": "",
      "DefaultRecipients": []
    }
  },
  "AlertRules": [
    {
      "Name": "High CPU Usage",
      "Description": "Alert when CPU usage exceeds 80%",
      "MetricName": "system.cpu.usage",
      "Threshold": 80.0,
      "Operator": "GreaterThan",
      "Severity": "Warning",
      "Enabled": true
    }
  ]
}
```

## Alert Types

The AlertingSystem supports various alert types:

- **ThresholdAlert**: Based on metric threshold violations
- **AnomalyAlert**: Based on unusual metric patterns
- **TrendAlert**: Based on concerning metric trends
- **CorrelationAlert**: Based on patterns across multiple metrics
- **HealthAlert**: Based on system component health
- **VersionAlert**: Based on version-related issues

## Notification Channels

The AlertingSystem supports multiple notification channels:

- **Email**: Sends email notifications for alerts
- **Webhook**: Sends webhook notifications to configured endpoints
- **SMS**: Sends SMS notifications for critical alerts

## Alert Lifecycle

Alerts go through the following lifecycle:

1. **Active**: Alert is created and active
2. **Acknowledged**: Alert is acknowledged by a user
3. **Resolved**: Alert is resolved by a user

## Integration with Other Components

The AlertingSystem integrates with other components of the FlowOrchestrator system:

- **MonitoringFramework**: Provides metrics and health check data
- **StatisticsService**: Provides historical metrics data
- **ServiceManager**: Provides service health and status information
- **FlowManager**: Provides flow execution metrics and status information
- **Orchestrator**: Provides orchestration metrics and status information

## Dependencies

- **FlowOrchestrator.Abstractions**: Core interfaces and abstract classes
- **FlowOrchestrator.Common**: Common utilities and helpers
- **FlowOrchestrator.Infrastructure.Telemetry**: Telemetry infrastructure
- **FlowOrchestrator.Infrastructure.Messaging**: Messaging infrastructure
