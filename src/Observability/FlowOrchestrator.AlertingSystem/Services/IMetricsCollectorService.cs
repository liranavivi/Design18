using FlowOrchestrator.Observability.Alerting.Models;

namespace FlowOrchestrator.Observability.Alerting.Services;

/// <summary>
/// Interface for the metrics collector service.
/// </summary>
public interface IMetricsCollectorService
{
    /// <summary>
    /// Initializes the service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Collects metrics from various sources.
    /// </summary>
    /// <returns>The list of metrics.</returns>
    Task<List<MetricData>> CollectMetricsAsync();

    /// <summary>
    /// Collects system metrics.
    /// </summary>
    /// <returns>The list of system metrics.</returns>
    Task<List<MetricData>> CollectSystemMetricsAsync();

    /// <summary>
    /// Collects flow metrics.
    /// </summary>
    /// <returns>The list of flow metrics.</returns>
    Task<List<MetricData>> CollectFlowMetricsAsync();

    /// <summary>
    /// Collects service metrics.
    /// </summary>
    /// <returns>The list of service metrics.</returns>
    Task<List<MetricData>> CollectServiceMetricsAsync();
}
