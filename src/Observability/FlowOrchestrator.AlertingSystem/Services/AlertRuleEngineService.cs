using FlowOrchestrator.Observability.Alerting.Configuration;
using FlowOrchestrator.Observability.Alerting.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FlowOrchestrator.Observability.Alerting.Services;

/// <summary>
/// Service for evaluating alert rules against metrics.
/// </summary>
public class AlertRuleEngineService : IAlertRuleEngineService
{
    private readonly ILogger<AlertRuleEngineService> _logger;
    private readonly IOptions<AlertingSystemOptions> _options;
    private readonly List<AlertDefinition> _alertDefinitions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertRuleEngineService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public AlertRuleEngineService(
        ILogger<AlertRuleEngineService> logger,
        IOptions<AlertingSystemOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing alert rule engine service");

        // Load alert definitions from configuration
        LoadAlertDefinitionsFromConfiguration();

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<List<AlertModel>> EvaluateRulesAsync(List<MetricData> metrics)
    {
        using var activity = new ActivitySource("FlowOrchestrator.AlertingSystem").StartActivity("EvaluateRules");
        
        var alerts = new List<AlertModel>();

        _logger.LogDebug("Evaluating {RuleCount} alert rules against {MetricCount} metrics", 
            _alertDefinitions.Count, metrics.Count);

        foreach (var alertDefinition in _alertDefinitions.Where(ad => ad.Enabled))
        {
            // Find matching metrics for this alert definition
            var matchingMetrics = metrics.Where(m => m.Name == alertDefinition.MetricName).ToList();

            if (!matchingMetrics.Any())
            {
                continue;
            }

            foreach (var metric in matchingMetrics)
            {
                // Check if the metric matches any filter attributes
                if (!MatchesFilterAttributes(metric, alertDefinition.FilterAttributes))
                {
                    continue;
                }

                // Evaluate the rule
                if (EvaluateRule(metric.Value, alertDefinition.Threshold, alertDefinition.Operator))
                {
                    // Create an alert
                    var alert = new AlertModel
                    {
                        Name = alertDefinition.Name,
                        Description = alertDefinition.Description,
                        Severity = alertDefinition.Severity,
                        Category = alertDefinition.Category,
                        Source = metric.Source,
                        Timestamp = DateTime.UtcNow,
                        MetricName = metric.Name,
                        MetricValue = metric.Value,
                        Threshold = alertDefinition.Threshold,
                        Operator = alertDefinition.Operator,
                        Status = AlertStatus.Active,
                        Details = new Dictionary<string, object>
                        {
                            { "AlertDefinitionId", alertDefinition.AlertDefinitionId },
                            { "MetricAttributes", metric.Attributes }
                        }
                    };

                    alerts.Add(alert);

                    _logger.LogInformation("Alert generated: {AlertName}, Metric: {MetricName}, Value: {MetricValue}, Threshold: {Threshold}",
                        alert.Name, alert.MetricName, alert.MetricValue, alert.Threshold);
                }
            }
        }

        return await Task.FromResult(alerts);
    }

    /// <inheritdoc/>
    public async Task<List<AlertDefinition>> GetAlertDefinitionsAsync()
    {
        return await Task.FromResult(_alertDefinitions.ToList());
    }

    /// <inheritdoc/>
    public async Task<AlertDefinition?> GetAlertDefinitionAsync(string alertDefinitionId)
    {
        return await Task.FromResult(_alertDefinitions.FirstOrDefault(ad => ad.AlertDefinitionId == alertDefinitionId));
    }

    /// <inheritdoc/>
    public async Task<AlertDefinition> CreateAlertDefinitionAsync(AlertDefinition alertDefinition)
    {
        alertDefinition.AlertDefinitionId = Guid.NewGuid().ToString();
        alertDefinition.CreatedTimestamp = DateTime.UtcNow;
        alertDefinition.LastModifiedTimestamp = DateTime.UtcNow;

        _alertDefinitions.Add(alertDefinition);

        _logger.LogInformation("Alert definition created: {AlertDefinitionId}, {Name}", 
            alertDefinition.AlertDefinitionId, alertDefinition.Name);

        return await Task.FromResult(alertDefinition);
    }

    /// <inheritdoc/>
    public async Task<AlertDefinition> UpdateAlertDefinitionAsync(AlertDefinition alertDefinition)
    {
        var existingDefinition = _alertDefinitions.FirstOrDefault(ad => ad.AlertDefinitionId == alertDefinition.AlertDefinitionId);
        
        if (existingDefinition == null)
        {
            throw new KeyNotFoundException($"Alert definition with ID {alertDefinition.AlertDefinitionId} not found");
        }

        // Update the existing definition
        var index = _alertDefinitions.IndexOf(existingDefinition);
        alertDefinition.LastModifiedTimestamp = DateTime.UtcNow;
        _alertDefinitions[index] = alertDefinition;

        _logger.LogInformation("Alert definition updated: {AlertDefinitionId}, {Name}", 
            alertDefinition.AlertDefinitionId, alertDefinition.Name);

        return await Task.FromResult(alertDefinition);
    }

    /// <inheritdoc/>
    public async Task DeleteAlertDefinitionAsync(string alertDefinitionId)
    {
        var existingDefinition = _alertDefinitions.FirstOrDefault(ad => ad.AlertDefinitionId == alertDefinitionId);
        
        if (existingDefinition == null)
        {
            throw new KeyNotFoundException($"Alert definition with ID {alertDefinitionId} not found");
        }

        _alertDefinitions.Remove(existingDefinition);

        _logger.LogInformation("Alert definition deleted: {AlertDefinitionId}, {Name}", 
            existingDefinition.AlertDefinitionId, existingDefinition.Name);

        await Task.CompletedTask;
    }

    private void LoadAlertDefinitionsFromConfiguration()
    {
        _alertDefinitions.Clear();

        foreach (var ruleOptions in _options.Value.AlertRules)
        {
            var alertDefinition = new AlertDefinition
            {
                AlertDefinitionId = Guid.NewGuid().ToString(),
                Name = ruleOptions.Name,
                Description = ruleOptions.Description,
                MetricName = ruleOptions.MetricName,
                Threshold = ruleOptions.Threshold,
                Operator = Enum.Parse<ComparisonOperator>(ruleOptions.Operator),
                Severity = Enum.Parse<AlertSeverity>(ruleOptions.Severity),
                Enabled = ruleOptions.Enabled,
                CreatedTimestamp = DateTime.UtcNow,
                LastModifiedTimestamp = DateTime.UtcNow
            };

            _alertDefinitions.Add(alertDefinition);

            _logger.LogInformation("Loaded alert definition from configuration: {Name}", alertDefinition.Name);
        }

        _logger.LogInformation("Loaded {Count} alert definitions from configuration", _alertDefinitions.Count);
    }

    private bool MatchesFilterAttributes(MetricData metric, Dictionary<string, string> filterAttributes)
    {
        if (filterAttributes.Count == 0)
        {
            return true;
        }

        foreach (var filter in filterAttributes)
        {
            if (!metric.Attributes.TryGetValue(filter.Key, out var value) || value != filter.Value)
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateRule(double metricValue, double threshold, ComparisonOperator op)
    {
        return op switch
        {
            ComparisonOperator.Equal => Math.Abs(metricValue - threshold) < 0.0001,
            ComparisonOperator.NotEqual => Math.Abs(metricValue - threshold) >= 0.0001,
            ComparisonOperator.GreaterThan => metricValue > threshold,
            ComparisonOperator.GreaterThanOrEqual => metricValue >= threshold,
            ComparisonOperator.LessThan => metricValue < threshold,
            ComparisonOperator.LessThanOrEqual => metricValue <= threshold,
            _ => false
        };
    }
}
