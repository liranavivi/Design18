using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Manager.OrchestratedFlow.Services;

/// <summary>
/// Service for validating references to Workflow entities
/// </summary>
public class WorkflowValidationService : IWorkflowValidationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WorkflowValidationService> _logger;

    public WorkflowValidationService(
        HttpClient httpClient,
        ILogger<WorkflowValidationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ValidateWorkflowExistsAsync(Guid workflowId)
    {
        _logger.LogInformation("Starting workflow existence validation. WorkflowId: {WorkflowId}", workflowId);

        try
        {
            var response = await _httpClient.GetAsync($"api/workflow/{workflowId}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully validated workflow exists. WorkflowId: {WorkflowId}", workflowId);
                return true;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Workflow not found. WorkflowId: {WorkflowId}", workflowId);
                return false;
            }

            // Fail-safe: if we can't validate, assume workflow doesn't exist
            _logger.LogWarning("Failed to validate workflow existence - service returned error. WorkflowId: {WorkflowId}, StatusCode: {StatusCode}",
                workflowId, response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
            // Fail-safe: if service is unavailable, assume workflow doesn't exist
            _logger.LogError(ex, "HTTP error validating workflow existence - service may be unavailable. WorkflowId: {WorkflowId}",
                workflowId);
            return false;
        }
        catch (TaskCanceledException ex)
        {
            // Fail-safe: if request times out, assume workflow doesn't exist
            _logger.LogError(ex, "Timeout validating workflow existence. WorkflowId: {WorkflowId}", workflowId);
            return false;
        }
        catch (Exception ex)
        {
            // Fail-safe: if any other error occurs, assume workflow doesn't exist
            _logger.LogError(ex, "Unexpected error validating workflow existence. WorkflowId: {WorkflowId}", workflowId);
            return false;
        }
    }
}
