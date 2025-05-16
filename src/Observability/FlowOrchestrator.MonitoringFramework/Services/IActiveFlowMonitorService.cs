using FlowOrchestrator.Observability.Monitoring.Models;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Interface for the active flow monitor service.
    /// </summary>
    public interface IActiveFlowMonitorService
    {
        /// <summary>
        /// Gets all active flows.
        /// </summary>
        /// <returns>The list of active flow statuses.</returns>
        Task<List<ActiveFlowStatus>> GetActiveFlowsAsync();

        /// <summary>
        /// Gets an active flow by ID.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The active flow status.</returns>
        Task<ActiveFlowStatus?> GetActiveFlowAsync(string flowId, string executionId);

        /// <summary>
        /// Gets active flows by status.
        /// </summary>
        /// <param name="status">The execution status.</param>
        /// <returns>The list of active flow statuses.</returns>
        Task<List<ActiveFlowStatus>> GetActiveFlowsByStatusAsync(string status);

        /// <summary>
        /// Gets flow execution history.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="limit">The maximum number of executions to retrieve.</param>
        /// <returns>The list of active flow statuses.</returns>
        Task<List<ActiveFlowStatus>> GetFlowExecutionHistoryAsync(string flowId, int limit = 10);

        /// <summary>
        /// Gets flow execution statistics.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <returns>The dictionary of statistics.</returns>
        Task<Dictionary<string, object>> GetFlowExecutionStatisticsAsync(string flowId);

        /// <summary>
        /// Registers a flow execution for monitoring.
        /// </summary>
        /// <param name="flowStatus">The active flow status.</param>
        void RegisterFlowExecution(ActiveFlowStatus flowStatus);

        /// <summary>
        /// Updates a flow execution status.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="status">The execution status.</param>
        /// <param name="currentStep">The current step.</param>
        void UpdateFlowExecutionStatus(string flowId, string executionId, string status, string currentStep);
    }
}
