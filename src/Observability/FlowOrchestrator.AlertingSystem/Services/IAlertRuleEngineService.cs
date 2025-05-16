using FlowOrchestrator.Observability.Alerting.Models;

namespace FlowOrchestrator.Observability.Alerting.Services;

/// <summary>
/// Interface for the alert rule engine service.
/// </summary>
public interface IAlertRuleEngineService
{
    /// <summary>
    /// Initializes the service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Evaluates alert rules against metrics.
    /// </summary>
    /// <param name="metrics">The metrics to evaluate.</param>
    /// <returns>The list of alerts.</returns>
    Task<List<AlertModel>> EvaluateRulesAsync(List<MetricData> metrics);

    /// <summary>
    /// Gets all alert definitions.
    /// </summary>
    /// <returns>The list of alert definitions.</returns>
    Task<List<AlertDefinition>> GetAlertDefinitionsAsync();

    /// <summary>
    /// Gets an alert definition by ID.
    /// </summary>
    /// <param name="alertDefinitionId">The alert definition ID.</param>
    /// <returns>The alert definition.</returns>
    Task<AlertDefinition?> GetAlertDefinitionAsync(string alertDefinitionId);

    /// <summary>
    /// Creates an alert definition.
    /// </summary>
    /// <param name="alertDefinition">The alert definition.</param>
    /// <returns>The created alert definition.</returns>
    Task<AlertDefinition> CreateAlertDefinitionAsync(AlertDefinition alertDefinition);

    /// <summary>
    /// Updates an alert definition.
    /// </summary>
    /// <param name="alertDefinition">The alert definition.</param>
    /// <returns>The updated alert definition.</returns>
    Task<AlertDefinition> UpdateAlertDefinitionAsync(AlertDefinition alertDefinition);

    /// <summary>
    /// Deletes an alert definition.
    /// </summary>
    /// <param name="alertDefinitionId">The alert definition ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAlertDefinitionAsync(string alertDefinitionId);
}
