using FlowOrchestrator.Observability.Monitoring.Models;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Implementation of the alert manager service.
    /// </summary>
    public class AlertManagerService : IAlertManagerService
    {
        private readonly ILogger<AlertManagerService> _logger;
        private readonly IOptions<MonitoringOptions> _options;
        private readonly List<AlertModel> _alerts = new();
        private readonly object _lock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertManagerService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The monitoring options.</param>
        public AlertManagerService(
            ILogger<AlertManagerService> logger,
            IOptions<MonitoringOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        /// <inheritdoc/>
        public async Task<AlertModel?> AcknowledgeAlertAsync(string alertId, string acknowledgedBy)
        {
            lock (_lock)
            {
                var alert = _alerts.FirstOrDefault(a => a.AlertId == alertId);
                
                if (alert != null)
                {
                    alert.Status = AlertStatus.Acknowledged;
                    alert.Details["AcknowledgedBy"] = acknowledgedBy;
                    alert.Details["AcknowledgedAt"] = DateTime.UtcNow;
                    
                    _logger.LogInformation("Alert {AlertId} acknowledged by {AcknowledgedBy}", alertId, acknowledgedBy);
                    return alert;
                }
                
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<AlertModel> CreateAlertAsync(AlertModel alert)
        {
            lock (_lock)
            {
                // Check if a similar alert already exists
                var existingAlert = _alerts.FirstOrDefault(a => 
                    a.Name == alert.Name && 
                    a.Source == alert.Source && 
                    a.Status == AlertStatus.Active);
                
                if (existingAlert != null)
                {
                    // Update the existing alert
                    existingAlert.Description = alert.Description;
                    existingAlert.Severity = alert.Severity;
                    existingAlert.Timestamp = DateTime.UtcNow;
                    
                    foreach (var detail in alert.Details)
                    {
                        existingAlert.Details[detail.Key] = detail.Value;
                    }
                    
                    existingAlert.Details["OccurrenceCount"] = existingAlert.Details.TryGetValue("OccurrenceCount", out var count) 
                        ? (int)count + 1 
                        : 1;
                    
                    _logger.LogInformation("Updated existing alert {AlertId}", existingAlert.AlertId);
                    return existingAlert;
                }
                
                // Add occurrence count
                alert.Details["OccurrenceCount"] = 1;
                
                _alerts.Add(alert);
                _logger.LogInformation("Created new alert {AlertId}: {AlertName}", alert.AlertId, alert.Name);
                
                return alert;
            }
        }

        /// <inheritdoc/>
        public async Task<AlertModel?> GetAlertByIdAsync(string alertId)
        {
            lock (_lock)
            {
                return _alerts.FirstOrDefault(a => a.AlertId == alertId);
            }
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> GetAlertsAsync(bool includeResolved = false)
        {
            lock (_lock)
            {
                if (includeResolved)
                {
                    return _alerts.ToList();
                }
                
                return _alerts.Where(a => a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Closed).ToList();
            }
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> GetAlertsByCategoryAsync(string category)
        {
            lock (_lock)
            {
                return _alerts.Where(a => a.Category == category).ToList();
            }
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> GetAlertsBySeverityAsync(AlertSeverity severity)
        {
            lock (_lock)
            {
                return _alerts.Where(a => a.Severity == severity).ToList();
            }
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> GetAlertsBySourceAsync(string source)
        {
            lock (_lock)
            {
                return _alerts.Where(a => a.Source == source).ToList();
            }
        }

        /// <inheritdoc/>
        public async Task<AlertModel?> ResolveAlertAsync(string alertId, string resolutionNotes)
        {
            lock (_lock)
            {
                var alert = _alerts.FirstOrDefault(a => a.AlertId == alertId);
                
                if (alert != null)
                {
                    alert.Status = AlertStatus.Resolved;
                    alert.ResolutionNotes = resolutionNotes;
                    alert.ResolvedTimestamp = DateTime.UtcNow;
                    
                    _logger.LogInformation("Alert {AlertId} resolved: {ResolutionNotes}", alertId, resolutionNotes);
                    return alert;
                }
                
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<AlertModel> UpdateAlertAsync(AlertModel alert)
        {
            lock (_lock)
            {
                var existingAlert = _alerts.FirstOrDefault(a => a.AlertId == alert.AlertId);
                
                if (existingAlert != null)
                {
                    existingAlert.Name = alert.Name;
                    existingAlert.Description = alert.Description;
                    existingAlert.Severity = alert.Severity;
                    existingAlert.Category = alert.Category;
                    existingAlert.Status = alert.Status;
                    existingAlert.ResolutionNotes = alert.ResolutionNotes;
                    existingAlert.ResolvedTimestamp = alert.ResolvedTimestamp;
                    existingAlert.Details = alert.Details;
                    
                    _logger.LogInformation("Updated alert {AlertId}", alert.AlertId);
                    return existingAlert;
                }
                
                _alerts.Add(alert);
                _logger.LogInformation("Added alert {AlertId}", alert.AlertId);
                return alert;
            }
        }
    }
}
