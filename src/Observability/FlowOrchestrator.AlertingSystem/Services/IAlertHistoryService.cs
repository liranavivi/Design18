using FlowOrchestrator.Observability.Alerting.Models;

namespace FlowOrchestrator.Observability.Alerting.Services;

/// <summary>
/// Interface for the alert history service.
/// </summary>
public interface IAlertHistoryService
{
    /// <summary>
    /// Initializes the service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Saves an alert to the history.
    /// </summary>
    /// <param name="alert">The alert.</param>
    /// <returns>The saved alert.</returns>
    Task<AlertModel> SaveAlertAsync(AlertModel alert);

    /// <summary>
    /// Gets all alerts.
    /// </summary>
    /// <param name="includeResolved">Whether to include resolved alerts.</param>
    /// <returns>The list of alerts.</returns>
    Task<List<AlertModel>> GetAlertsAsync(bool includeResolved = false);

    /// <summary>
    /// Gets an alert by ID.
    /// </summary>
    /// <param name="alertId">The alert ID.</param>
    /// <returns>The alert.</returns>
    Task<AlertModel?> GetAlertAsync(string alertId);

    /// <summary>
    /// Gets alerts by severity.
    /// </summary>
    /// <param name="severity">The severity.</param>
    /// <param name="includeResolved">Whether to include resolved alerts.</param>
    /// <returns>The list of alerts.</returns>
    Task<List<AlertModel>> GetAlertsBySeverityAsync(AlertSeverity severity, bool includeResolved = false);

    /// <summary>
    /// Gets alerts by category.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="includeResolved">Whether to include resolved alerts.</param>
    /// <returns>The list of alerts.</returns>
    Task<List<AlertModel>> GetAlertsByCategoryAsync(string category, bool includeResolved = false);

    /// <summary>
    /// Gets alerts by source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="includeResolved">Whether to include resolved alerts.</param>
    /// <returns>The list of alerts.</returns>
    Task<List<AlertModel>> GetAlertsBySourceAsync(string source, bool includeResolved = false);

    /// <summary>
    /// Acknowledges an alert.
    /// </summary>
    /// <param name="alertId">The alert ID.</param>
    /// <param name="acknowledgedBy">The user who acknowledged the alert.</param>
    /// <returns>The acknowledged alert.</returns>
    Task<AlertModel> AcknowledgeAlertAsync(string alertId, string acknowledgedBy);

    /// <summary>
    /// Resolves an alert.
    /// </summary>
    /// <param name="alertId">The alert ID.</param>
    /// <param name="resolvedBy">The user who resolved the alert.</param>
    /// <param name="resolutionNotes">The resolution notes.</param>
    /// <returns>The resolved alert.</returns>
    Task<AlertModel> ResolveAlertAsync(string alertId, string resolvedBy, string resolutionNotes);

    /// <summary>
    /// Cleans up old alerts.
    /// </summary>
    /// <param name="retentionDays">The retention period in days.</param>
    /// <returns>The number of alerts removed.</returns>
    Task<int> CleanupOldAlertsAsync(int retentionDays);
}
