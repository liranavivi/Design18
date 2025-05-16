namespace FlowOrchestrator.Observability.Alerting.Models;

/// <summary>
/// Metric data for the alerting system.
/// </summary>
public class MetricData
{
    /// <summary>
    /// Gets or sets the metric name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attributes.
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
}
