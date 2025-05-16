namespace FlowOrchestrator.Observability.Statistics.Models
{
    /// <summary>
    /// Represents an alert definition.
    /// </summary>
    public class AlertDefinition
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
        /// Gets or sets the metric name.
        /// </summary>
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the threshold value.
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
        /// Gets or sets a value indicating whether the alert is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the filter attributes.
        /// </summary>
        public Dictionary<string, string> FilterAttributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the notification channels.
        /// </summary>
        public List<string> NotificationChannels { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the cooldown period in minutes.
        /// </summary>
        public int CooldownMinutes { get; set; } = 15;
    }

    /// <summary>
    /// Represents a comparison operator for alerts.
    /// </summary>
    public enum ComparisonOperator
    {
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
        LessThanOrEqual,

        /// <summary>
        /// Equal to.
        /// </summary>
        Equal,

        /// <summary>
        /// Not equal to.
        /// </summary>
        NotEqual
    }

    /// <summary>
    /// Represents the severity of an alert.
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// Informational severity.
        /// </summary>
        Info,

        /// <summary>
        /// Warning severity.
        /// </summary>
        Warning,

        /// <summary>
        /// Error severity.
        /// </summary>
        Error,

        /// <summary>
        /// Critical severity.
        /// </summary>
        Critical
    }
}
