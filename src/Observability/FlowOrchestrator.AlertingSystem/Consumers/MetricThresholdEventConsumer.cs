using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Observability.Alerting.Models;
using FlowOrchestrator.Observability.Alerting.Services;

namespace FlowOrchestrator.Observability.Alerting.Consumers;

/// <summary>
/// Consumer for metric threshold events.
/// </summary>
public class MetricThresholdEventConsumer : IMessageConsumer<MetricThresholdEvent>
{
    private readonly ILogger<MetricThresholdEventConsumer> _logger;
    private readonly AlertHistoryService _alertHistoryService;
    private readonly NotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricThresholdEventConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="alertHistoryService">The alert history service.</param>
    /// <param name="notificationService">The notification service.</param>
    public MetricThresholdEventConsumer(
        ILogger<MetricThresholdEventConsumer> logger,
        AlertHistoryService alertHistoryService,
        NotificationService notificationService)
    {
        _logger = logger;
        _alertHistoryService = alertHistoryService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Consumes a metric threshold event message.
    /// </summary>
    /// <param name="context">The consume context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<MetricThresholdEvent> context)
    {
        var thresholdEvent = context.Message;
        
        _logger.LogInformation("Received metric threshold event: {MetricName}, {EventId}", 
            thresholdEvent.MetricName, thresholdEvent.EventId);

        try
        {
            // Create an alert
            var alert = new AlertModel
            {
                Name = $"Metric Threshold: {thresholdEvent.MetricName}",
                Description = thresholdEvent.Description,
                Severity = thresholdEvent.Severity,
                Category = "MetricThreshold",
                Source = thresholdEvent.Source,
                Timestamp = thresholdEvent.Timestamp,
                MetricName = thresholdEvent.MetricName,
                MetricValue = thresholdEvent.MetricValue,
                Threshold = thresholdEvent.Threshold,
                Operator = thresholdEvent.Operator,
                Status = AlertStatus.Active,
                Details = new Dictionary<string, object>
                {
                    { "EventId", thresholdEvent.EventId },
                    { "MetricAttributes", thresholdEvent.MetricAttributes }
                }
            };

            // Save the alert
            await _alertHistoryService.SaveAlertAsync(alert);
            
            // Send notifications
            await _notificationService.SendNotificationsAsync(alert);
            
            _logger.LogInformation("Created alert from metric threshold event: {MetricName}, {EventId}, Alert: {AlertId}", 
                thresholdEvent.MetricName, thresholdEvent.EventId, alert.AlertId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing metric threshold event: {MetricName}, {EventId}", 
                thresholdEvent.MetricName, thresholdEvent.EventId);
        }
    }
}

/// <summary>
/// Metric threshold event message.
/// </summary>
public class MetricThresholdEvent
{
    /// <summary>
    /// Gets or sets the event identifier.
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the metric name.
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric value.
    /// </summary>
    public double MetricValue { get; set; }

    /// <summary>
    /// Gets or sets the threshold.
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator.
    /// </summary>
    public ComparisonOperator Operator { get; set; } = ComparisonOperator.GreaterThan;

    /// <summary>
    /// Gets or sets the severity.
    /// </summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the metric attributes.
    /// </summary>
    public Dictionary<string, string> MetricAttributes { get; set; } = new Dictionary<string, string>();
}
