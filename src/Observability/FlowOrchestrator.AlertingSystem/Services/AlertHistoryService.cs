using FlowOrchestrator.Observability.Alerting.Configuration;
using FlowOrchestrator.Observability.Alerting.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FlowOrchestrator.Observability.Alerting.Services;

/// <summary>
/// Service for tracking alert history.
/// </summary>
public class AlertHistoryService : IAlertHistoryService
{
    private readonly ILogger<AlertHistoryService> _logger;
    private readonly IOptions<AlertingSystemOptions> _options;
    private readonly List<AlertModel> _alerts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertHistoryService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public AlertHistoryService(
        ILogger<AlertHistoryService> logger,
        IOptions<AlertingSystemOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing alert history service");
        
        // Schedule cleanup of old alerts
        _ = ScheduleCleanupAsync();
        
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<AlertModel> SaveAlertAsync(AlertModel alert)
    {
        using var activity = new ActivitySource("FlowOrchestrator.AlertingSystem").StartActivity("SaveAlert");
        
        // Check if this is a duplicate alert (same name, source, and metric within the last hour)
        var existingAlert = _alerts.FirstOrDefault(a => 
            a.Name == alert.Name && 
            a.Source == alert.Source && 
            a.MetricName == alert.MetricName &&
            a.Status != AlertStatus.Resolved &&
            (DateTime.UtcNow - a.Timestamp).TotalHours < 1);

        if (existingAlert != null)
        {
            _logger.LogDebug("Duplicate alert detected: {AlertId}, {Name}. Updating existing alert.", 
                existingAlert.AlertId, existingAlert.Name);
            
            // Update the existing alert
            existingAlert.MetricValue = alert.MetricValue;
            existingAlert.Timestamp = DateTime.UtcNow;
            existingAlert.Details["UpdateCount"] = existingAlert.Details.TryGetValue("UpdateCount", out var count) 
                ? (int)count + 1 
                : 1;
            
            return await Task.FromResult(existingAlert);
        }

        // Add the new alert
        _alerts.Add(alert);
        
        _logger.LogInformation("Alert saved: {AlertId}, {Name}, {Severity}", 
            alert.AlertId, alert.Name, alert.Severity);
        
        return await Task.FromResult(alert);
    }

    /// <inheritdoc/>
    public async Task<List<AlertModel>> GetAlertsAsync(bool includeResolved = false)
    {
        if (includeResolved)
        {
            return await Task.FromResult(_alerts.OrderByDescending(a => a.Timestamp).ToList());
        }
        
        return await Task.FromResult(_alerts
            .Where(a => a.Status != AlertStatus.Resolved)
            .OrderByDescending(a => a.Timestamp)
            .ToList());
    }

    /// <inheritdoc/>
    public async Task<AlertModel?> GetAlertAsync(string alertId)
    {
        return await Task.FromResult(_alerts.FirstOrDefault(a => a.AlertId == alertId));
    }

    /// <inheritdoc/>
    public async Task<List<AlertModel>> GetAlertsBySeverityAsync(AlertSeverity severity, bool includeResolved = false)
    {
        if (includeResolved)
        {
            return await Task.FromResult(_alerts
                .Where(a => a.Severity == severity)
                .OrderByDescending(a => a.Timestamp)
                .ToList());
        }
        
        return await Task.FromResult(_alerts
            .Where(a => a.Severity == severity && a.Status != AlertStatus.Resolved)
            .OrderByDescending(a => a.Timestamp)
            .ToList());
    }

    /// <inheritdoc/>
    public async Task<List<AlertModel>> GetAlertsByCategoryAsync(string category, bool includeResolved = false)
    {
        if (includeResolved)
        {
            return await Task.FromResult(_alerts
                .Where(a => a.Category == category)
                .OrderByDescending(a => a.Timestamp)
                .ToList());
        }
        
        return await Task.FromResult(_alerts
            .Where(a => a.Category == category && a.Status != AlertStatus.Resolved)
            .OrderByDescending(a => a.Timestamp)
            .ToList());
    }

    /// <inheritdoc/>
    public async Task<List<AlertModel>> GetAlertsBySourceAsync(string source, bool includeResolved = false)
    {
        if (includeResolved)
        {
            return await Task.FromResult(_alerts
                .Where(a => a.Source == source)
                .OrderByDescending(a => a.Timestamp)
                .ToList());
        }
        
        return await Task.FromResult(_alerts
            .Where(a => a.Source == source && a.Status != AlertStatus.Resolved)
            .OrderByDescending(a => a.Timestamp)
            .ToList());
    }

    /// <inheritdoc/>
    public async Task<AlertModel> AcknowledgeAlertAsync(string alertId, string acknowledgedBy)
    {
        var alert = _alerts.FirstOrDefault(a => a.AlertId == alertId);
        
        if (alert == null)
        {
            throw new KeyNotFoundException($"Alert with ID {alertId} not found");
        }
        
        if (alert.Status == AlertStatus.Resolved)
        {
            throw new InvalidOperationException($"Alert with ID {alertId} is already resolved");
        }
        
        alert.Status = AlertStatus.Acknowledged;
        alert.AcknowledgedTimestamp = DateTime.UtcNow;
        alert.AcknowledgedBy = acknowledgedBy;
        
        _logger.LogInformation("Alert acknowledged: {AlertId}, {Name}, by {AcknowledgedBy}", 
            alert.AlertId, alert.Name, acknowledgedBy);
        
        return await Task.FromResult(alert);
    }

    /// <inheritdoc/>
    public async Task<AlertModel> ResolveAlertAsync(string alertId, string resolvedBy, string resolutionNotes)
    {
        var alert = _alerts.FirstOrDefault(a => a.AlertId == alertId);
        
        if (alert == null)
        {
            throw new KeyNotFoundException($"Alert with ID {alertId} not found");
        }
        
        if (alert.Status == AlertStatus.Resolved)
        {
            throw new InvalidOperationException($"Alert with ID {alertId} is already resolved");
        }
        
        alert.Status = AlertStatus.Resolved;
        alert.ResolvedTimestamp = DateTime.UtcNow;
        alert.ResolvedBy = resolvedBy;
        alert.ResolutionNotes = resolutionNotes;
        
        _logger.LogInformation("Alert resolved: {AlertId}, {Name}, by {ResolvedBy}", 
            alert.AlertId, alert.Name, resolvedBy);
        
        return await Task.FromResult(alert);
    }

    /// <inheritdoc/>
    public async Task<int> CleanupOldAlertsAsync(int retentionDays)
    {
        using var activity = new ActivitySource("FlowOrchestrator.AlertingSystem").StartActivity("CleanupOldAlerts");
        
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var oldAlerts = _alerts.Where(a => a.Timestamp < cutoffDate).ToList();
        
        foreach (var alert in oldAlerts)
        {
            _alerts.Remove(alert);
        }
        
        _logger.LogInformation("Cleaned up {Count} old alerts older than {RetentionDays} days", 
            oldAlerts.Count, retentionDays);
        
        return await Task.FromResult(oldAlerts.Count);
    }

    private async Task ScheduleCleanupAsync()
    {
        while (true)
        {
            try
            {
                // Wait for 24 hours
                await Task.Delay(TimeSpan.FromHours(24));
                
                // Clean up old alerts
                await CleanupOldAlertsAsync(_options.Value.DataRetentionDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old alerts");
            }
        }
    }
}
