namespace FlowOrchestrator.Observability.Monitoring.Models
{
    /// <summary>
    /// Alert model for the monitoring framework.
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
        /// Gets or sets the alert source.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the alert category.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the alert was created.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the alert status.
        /// </summary>
        public AlertStatus Status { get; set; } = AlertStatus.Active;

        /// <summary>
        /// Gets or sets the timestamp when the alert was resolved.
        /// </summary>
        public DateTime? ResolvedTimestamp { get; set; }

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
        Resolved,

        /// <summary>
        /// Closed alert.
        /// </summary>
        Closed
    }
}
