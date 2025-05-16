namespace FlowOrchestrator.Observability.Alerting.Configuration;

/// <summary>
/// Options for the alerting system.
/// </summary>
public class AlertingSystemOptions
{
    /// <summary>
    /// Gets or sets the service identifier.
    /// </summary>
    public string ServiceId { get; set; } = "FlowOrchestrator.AlertingSystem";

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the health check interval in seconds.
    /// </summary>
    public int HealthCheckIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the alert check interval in seconds.
    /// </summary>
    public int AlertCheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the data retention period in days.
    /// </summary>
    public int DataRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the notification channels.
    /// </summary>
    public NotificationChannelsOptions NotificationChannels { get; set; } = new NotificationChannelsOptions();

    /// <summary>
    /// Gets or sets the alert rules.
    /// </summary>
    public List<AlertRuleOptions> AlertRules { get; set; } = new List<AlertRuleOptions>();

    /// <summary>
    /// Gets or sets the OpenTelemetry options.
    /// </summary>
    public OpenTelemetryOptions OpenTelemetry { get; set; } = new OpenTelemetryOptions();
}

/// <summary>
/// Options for notification channels.
/// </summary>
public class NotificationChannelsOptions
{
    /// <summary>
    /// Gets or sets the email notification options.
    /// </summary>
    public EmailNotificationOptions Email { get; set; } = new EmailNotificationOptions();

    /// <summary>
    /// Gets or sets the webhook notification options.
    /// </summary>
    public WebhookNotificationOptions Webhook { get; set; } = new WebhookNotificationOptions();

    /// <summary>
    /// Gets or sets the SMS notification options.
    /// </summary>
    public SmsNotificationOptions Sms { get; set; } = new SmsNotificationOptions();
}

/// <summary>
/// Options for email notifications.
/// </summary>
public class EmailNotificationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether email notifications are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the SMTP server.
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP port.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Gets or sets the SMTP username.
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP password.
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the from address.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default recipients.
    /// </summary>
    public List<string> DefaultRecipients { get; set; } = new List<string>();
}

/// <summary>
/// Options for webhook notifications.
/// </summary>
public class WebhookNotificationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether webhook notifications are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the webhook endpoints.
    /// </summary>
    public List<string> Endpoints { get; set; } = new List<string>();
}

/// <summary>
/// Options for SMS notifications.
/// </summary>
public class SmsNotificationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether SMS notifications are enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the SMS provider.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account SID.
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authentication token.
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the from number.
    /// </summary>
    public string FromNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default recipients.
    /// </summary>
    public List<string> DefaultRecipients { get; set; } = new List<string>();
}

/// <summary>
/// Options for alert rules.
/// </summary>
public class AlertRuleOptions
{
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
    /// Gets or sets the operator.
    /// </summary>
    public string Operator { get; set; } = "GreaterThan";

    /// <summary>
    /// Gets or sets the severity.
    /// </summary>
    public string Severity { get; set; } = "Warning";

    /// <summary>
    /// Gets or sets a value indicating whether the rule is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Options for OpenTelemetry.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string ServiceName { get; set; } = "FlowOrchestrator.AlertingSystem";

    /// <summary>
    /// Gets or sets the service namespace.
    /// </summary>
    public string ServiceNamespace { get; set; } = "FlowOrchestrator.Observability";

    /// <summary>
    /// Gets or sets the service version.
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets a value indicating whether to enable the console exporter.
    /// </summary>
    public bool EnableConsoleExporter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the OTLP exporter.
    /// </summary>
    public bool EnableOtlpExporter { get; set; } = false;

    /// <summary>
    /// Gets or sets the OTLP endpoint.
    /// </summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Gets or sets the tracing sampling rate.
    /// </summary>
    public double TracingSamplingRate { get; set; } = 1.0;
}
