using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Observability.Alerting.Models;
using FlowOrchestrator.Observability.Alerting.Services;

namespace FlowOrchestrator.Observability.Alerting.Consumers;

/// <summary>
/// Consumer for system events.
/// </summary>
public class SystemEventConsumer : IMessageConsumer<SystemEvent>
{
    private readonly ILogger<SystemEventConsumer> _logger;
    private readonly AlertRuleEngineService _alertRuleEngineService;
    private readonly AlertHistoryService _alertHistoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemEventConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="alertRuleEngineService">The alert rule engine service.</param>
    /// <param name="alertHistoryService">The alert history service.</param>
    public SystemEventConsumer(
        ILogger<SystemEventConsumer> logger,
        AlertRuleEngineService alertRuleEngineService,
        AlertHistoryService alertHistoryService)
    {
        _logger = logger;
        _alertRuleEngineService = alertRuleEngineService;
        _alertHistoryService = alertHistoryService;
    }

    /// <summary>
    /// Consumes a system event message.
    /// </summary>
    /// <param name="context">The consume context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<SystemEvent> context)
    {
        var systemEvent = context.Message;
        
        _logger.LogInformation("Received system event: {EventType}, {EventId}", 
            systemEvent.EventType, systemEvent.EventId);

        try
        {
            // Check if this event should generate an alert
            if (systemEvent.Severity == EventSeverity.Error || systemEvent.Severity == EventSeverity.Critical)
            {
                // Create an alert
                var alert = new AlertModel
                {
                    Name = $"System Event: {systemEvent.EventType}",
                    Description = systemEvent.Description,
                    Severity = MapEventSeverityToAlertSeverity(systemEvent.Severity),
                    Category = "SystemEvent",
                    Source = systemEvent.Source,
                    Timestamp = systemEvent.Timestamp,
                    MetricName = "system.event",
                    MetricValue = 1.0,
                    Threshold = 0.0,
                    Operator = ComparisonOperator.GreaterThan,
                    Status = AlertStatus.Active,
                    Details = new Dictionary<string, object>
                    {
                        { "EventId", systemEvent.EventId },
                        { "EventType", systemEvent.EventType },
                        { "EventData", systemEvent.EventData }
                    }
                };

                // Save the alert
                await _alertHistoryService.SaveAlertAsync(alert);
                
                _logger.LogInformation("Created alert from system event: {EventType}, {EventId}, Alert: {AlertId}", 
                    systemEvent.EventType, systemEvent.EventId, alert.AlertId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing system event: {EventType}, {EventId}", 
                systemEvent.EventType, systemEvent.EventId);
        }
    }

    private AlertSeverity MapEventSeverityToAlertSeverity(EventSeverity eventSeverity)
    {
        return eventSeverity switch
        {
            EventSeverity.Info => AlertSeverity.Info,
            EventSeverity.Warning => AlertSeverity.Warning,
            EventSeverity.Error => AlertSeverity.Error,
            EventSeverity.Critical => AlertSeverity.Critical,
            _ => AlertSeverity.Info
        };
    }
}

/// <summary>
/// System event message.
/// </summary>
public class SystemEvent
{
    /// <summary>
    /// Gets or sets the event identifier.
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the severity.
    /// </summary>
    public EventSeverity Severity { get; set; } = EventSeverity.Info;

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the event data.
    /// </summary>
    public Dictionary<string, object> EventData { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Event severity levels.
/// </summary>
public enum EventSeverity
{
    /// <summary>
    /// Informational event.
    /// </summary>
    Info,

    /// <summary>
    /// Warning event.
    /// </summary>
    Warning,

    /// <summary>
    /// Error event.
    /// </summary>
    Error,

    /// <summary>
    /// Critical event.
    /// </summary>
    Critical
}
