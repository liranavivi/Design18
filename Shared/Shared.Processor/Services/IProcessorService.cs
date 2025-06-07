using Shared.Processor.Models;

namespace Shared.Processor.Services;

/// <summary>
/// Interface for the core processor service functionality
/// </summary>
public interface IProcessorService
{
    /// <summary>
    /// Gets the ID of this processor instance
    /// </summary>
    /// <returns>The processor ID</returns>
    Task<Guid> GetProcessorIdAsync();

    /// <summary>
    /// Checks if a message is intended for this processor instance
    /// </summary>
    /// <param name="processorId">The processor ID from the message</param>
    /// <returns>True if the message is for this processor, false otherwise</returns>
    Task<bool> IsMessageForThisProcessorAsync(Guid processorId);

    /// <summary>
    /// Processes an activity message and returns the response
    /// </summary>
    /// <param name="message">The activity message to process</param>
    /// <returns>The activity response</returns>
    Task<ProcessorActivityResponse> ProcessActivityAsync(ProcessorActivityMessage message);

    /// <summary>
    /// Gets the current health status of the processor
    /// </summary>
    /// <returns>The health check response</returns>
    Task<ProcessorHealthStatusResponse> GetHealthStatusAsync();

    /// <summary>
    /// Gets statistics for the processor within the specified time period
    /// </summary>
    /// <param name="startTime">Start time for statistics period (null for all time)</param>
    /// <param name="endTime">End time for statistics period (null for current time)</param>
    /// <returns>The statistics response</returns>
    Task<ProcessorStatisticsResponse> GetStatisticsAsync(DateTime? startTime, DateTime? endTime);

    /// <summary>
    /// Initializes the processor service (retrieves or creates processor entity)
    /// </summary>
    /// <returns>Task representing the initialization operation</returns>
    Task InitializeAsync();

    /// <summary>
    /// Initializes the processor service with cancellation support (retrieves or creates processor entity)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop initialization</param>
    /// <returns>Task representing the initialization operation</returns>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves data from Hazelcast cache using the processor's map and key pattern
    /// </summary>
    /// <param name="orchestratedFlowEntityId">ID of the orchestrated flow entity</param>
    /// <param name="stepId">ID of the step</param>
    /// <param name="executionId">Execution ID</param>
    /// <param name="correlationId">Correlation ID for cache key isolation (defaults to Empty)</param>
    /// <returns>The cached data as a string</returns>
    Task<string?> GetCachedDataAsync(Guid orchestratedFlowEntityId, Guid stepId, Guid executionId, Guid correlationId = default);

    /// <summary>
    /// Saves data to Hazelcast cache using the processor's map and key pattern
    /// </summary>
    /// <param name="orchestratedFlowEntityId">ID of the orchestrated flow entity</param>
    /// <param name="stepId">ID of the step</param>
    /// <param name="executionId">Execution ID</param>
    /// <param name="data">Data to save</param>
    /// <param name="correlationId">Correlation ID for cache key isolation (defaults to Empty)</param>
    /// <returns>Task representing the save operation</returns>
    Task SaveCachedDataAsync(Guid orchestratedFlowEntityId, Guid stepId, Guid executionId, string data, Guid correlationId = default);

    /// <summary>
    /// Validates data against the input schema
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> ValidateInputDataAsync(string data);

    /// <summary>
    /// Validates data against the output schema
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> ValidateOutputDataAsync(string data);

    /// <summary>
    /// Gets the current schema health status including schema ID validation
    /// </summary>
    /// <returns>A tuple indicating if schemas are healthy and valid, along with error messages</returns>
    (bool InputSchemaHealthy, bool OutputSchemaHealthy, bool SchemaIdsValid, string InputSchemaError, string OutputSchemaError, string SchemaValidationError) GetSchemaHealthStatus();
}
