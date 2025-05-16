using FlowOrchestrator.Observability.Monitoring.Models;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Interface for the alert manager service.
    /// </summary>
    public interface IAlertManagerService
    {
        /// <summary>
        /// Creates an alert.
        /// </summary>
        /// <param name="alert">The alert model.</param>
        /// <returns>The created alert.</returns>
        Task<AlertModel> CreateAlertAsync(AlertModel alert);

        /// <summary>
        /// Gets all alerts.
        /// </summary>
        /// <param name="includeResolved">Whether to include resolved alerts.</param>
        /// <returns>The list of alerts.</returns>
        Task<List<AlertModel>> GetAlertsAsync(bool includeResolved = false);

        /// <summary>
        /// Gets alerts by severity.
        /// </summary>
        /// <param name="severity">The alert severity.</param>
        /// <returns>The list of alerts.</returns>
        Task<List<AlertModel>> GetAlertsBySeverityAsync(AlertSeverity severity);

        /// <summary>
        /// Gets alerts by category.
        /// </summary>
        /// <param name="category">The alert category.</param>
        /// <returns>The list of alerts.</returns>
        Task<List<AlertModel>> GetAlertsByCategoryAsync(string category);

        /// <summary>
        /// Gets alerts by source.
        /// </summary>
        /// <param name="source">The alert source.</param>
        /// <returns>The list of alerts.</returns>
        Task<List<AlertModel>> GetAlertsBySourceAsync(string source);

        /// <summary>
        /// Gets an alert by ID.
        /// </summary>
        /// <param name="alertId">The alert identifier.</param>
        /// <returns>The alert model.</returns>
        Task<AlertModel?> GetAlertByIdAsync(string alertId);

        /// <summary>
        /// Updates an alert.
        /// </summary>
        /// <param name="alert">The alert model.</param>
        /// <returns>The updated alert.</returns>
        Task<AlertModel> UpdateAlertAsync(AlertModel alert);

        /// <summary>
        /// Acknowledges an alert.
        /// </summary>
        /// <param name="alertId">The alert identifier.</param>
        /// <param name="acknowledgedBy">The user who acknowledged the alert.</param>
        /// <returns>The acknowledged alert.</returns>
        Task<AlertModel?> AcknowledgeAlertAsync(string alertId, string acknowledgedBy);

        /// <summary>
        /// Resolves an alert.
        /// </summary>
        /// <param name="alertId">The alert identifier.</param>
        /// <param name="resolutionNotes">The resolution notes.</param>
        /// <returns>The resolved alert.</returns>
        Task<AlertModel?> ResolveAlertAsync(string alertId, string resolutionNotes);
    }
}
