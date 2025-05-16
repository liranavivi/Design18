namespace FlowOrchestrator.Observability.Alerting.Models;

/// <summary>
/// Alert model for the alerting system.
/// </summary>
public class AlertModel
{
    /// <summary>
    /// Gets or sets the alert identifier.
    /// </summary>
    public string AlertId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the alert name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alert description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alert severity.
    /// </summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Info;

    /// <summary>
    /// Gets or sets the alert category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alert source.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the metric name.
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

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
    /// Gets or sets the status.
    /// </summary>
    public AlertStatus Status { get; set; } = AlertStatus.Active;

    /// <summary>
    /// Gets or sets the acknowledged timestamp.
    /// </summary>
    public DateTime? AcknowledgedTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the acknowledged by.
    /// </summary>
    public string AcknowledgedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolved timestamp.
    /// </summary>
    public DateTime? ResolvedTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the resolved by.
    /// </summary>
    public string ResolvedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolution notes.
    /// </summary>
    public string ResolutionNotes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alert details.
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Alert severity levels.
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Informational alert.
    /// </summary>
    Info,

    /// <summary>
    /// Warning alert.
    /// </summary>
    Warning,

    /// <summary>
    /// Error alert.
    /// </summary>
    Error,

    /// <summary>
    /// Critical alert.
    /// </summary>
    Critical
}

/// <summary>
/// Alert status.
/// </summary>
public enum AlertStatus
{
    /// <summary>
    /// Active alert.
    /// </summary>
    Active,

    /// <summary>
    /// Acknowledged alert.
    /// </summary>
    Acknowledged,

    /// <summary>
    /// Resolved alert.
    /// </summary>
    Resolved
}

/// <summary>
/// Comparison operators for alert rules.
/// </summary>
public enum ComparisonOperator
{
    /// <summary>
    /// Equal to.
    /// </summary>
    Equal,

    /// <summary>
    /// Not equal to.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Greater than.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Greater than or equal to.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Less than.
    /// </summary>
    LessThan,

    /// <summary>
    /// Less than or equal to.
    /// </summary>
    LessThanOrEqual
}
