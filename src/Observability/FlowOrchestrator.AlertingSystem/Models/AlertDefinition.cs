using FlowOrchestrator.Observability.Alerting.Models;

namespace FlowOrchestrator.Observability.Alerting.Models;

/// <summary>
/// Alert definition for the alerting system.
/// </summary>
public class AlertDefinition
{
    /// <summary>
    /// Gets or sets the alert definition identifier.
    /// </summary>
    public string AlertDefinitionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric name.
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the threshold.
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator.
    /// </summary>
    public ComparisonOperator Operator { get; set; } = ComparisonOperator.GreaterThan;

    /// <summary>
    /// Gets or sets the alert severity.
    /// </summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    /// <summary>
    /// Gets or sets the alert category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the alert definition is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the filter attributes.
    /// </summary>
    public Dictionary<string, string> FilterAttributes { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the notification settings.
    /// </summary>
    public NotificationSettings NotificationSettings { get; set; } = new NotificationSettings();

    /// <summary>
    /// Gets or sets the created timestamp.
    /// </summary>
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last modified timestamp.
    /// </summary>
    public DateTime LastModifiedTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modified by.
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;
}

/// <summary>
/// Notification settings for an alert definition.
/// </summary>
public class NotificationSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether to send email notifications.
    /// </summary>
    public bool SendEmail { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to send webhook notifications.
    /// </summary>
    public bool SendWebhook { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to send SMS notifications.
    /// </summary>
    public bool SendSms { get; set; } = false;

    /// <summary>
    /// Gets or sets the email recipients.
    /// </summary>
    public List<string> EmailRecipients { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the webhook endpoints.
    /// </summary>
    public List<string> WebhookEndpoints { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the SMS recipients.
    /// </summary>
    public List<string> SmsRecipients { get; set; } = new List<string>();
}
