using FlowOrchestrator.Observability.Monitoring.Models;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Implementation of the active flow monitor service.
    /// </summary>
    public class ActiveFlowMonitorService : IActiveFlowMonitorService
    {
        private readonly ILogger<ActiveFlowMonitorService> _logger;
        private readonly IOptions<MonitoringOptions> _options;
        private readonly Dictionary<string, Dictionary<string, ActiveFlowStatus>> _activeFlows = new();
        private readonly Dictionary<string, List<ActiveFlowStatus>> _flowHistory = new();
        private readonly object _lock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveFlowMonitorService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The monitoring options.</param>
        public ActiveFlowMonitorService(
            ILogger<ActiveFlowMonitorService> logger,
            IOptions<MonitoringOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        /// <inheritdoc/>
        public async Task<List<ActiveFlowStatus>> GetActiveFlowsAsync()
        {
            lock (_lock)
            {
                var activeFlows = new List<ActiveFlowStatus>();

                foreach (var flowExecutions in _activeFlows.Values)
                {
                    activeFlows.AddRange(flowExecutions.Values);
                }

                return activeFlows;
            }
        }

        /// <inheritdoc/>
        public async Task<ActiveFlowStatus?> GetActiveFlowAsync(string flowId, string executionId)
        {
            lock (_lock)
            {
                if (_activeFlows.TryGetValue(flowId, out var flowExecutions) &&
                    flowExecutions.TryGetValue(executionId, out var flowStatus))
                {
                    return flowStatus;
                }

                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ActiveFlowStatus>> GetActiveFlowsByStatusAsync(string status)
        {
            lock (_lock)
            {
                var activeFlows = new List<ActiveFlowStatus>();

                foreach (var flowExecutions in _activeFlows.Values)
                {
                    activeFlows.AddRange(flowExecutions.Values.Where(f => f.ExecutionStatus == status));
                }

                return activeFlows;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ActiveFlowStatus>> GetFlowExecutionHistoryAsync(string flowId, int limit = 10)
        {
            lock (_lock)
            {
                if (_flowHistory.TryGetValue(flowId, out var history))
                {
                    return history.OrderByDescending(h => h.StartTimestamp).Take(limit).ToList();
                }

                return new List<ActiveFlowStatus>();
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> GetFlowExecutionStatisticsAsync(string flowId)
        {
            var statistics = new Dictionary<string, object>();

            lock (_lock)
            {
                if (_flowHistory.TryGetValue(flowId, out var history))
                {
                    // Calculate statistics
                    statistics["TotalExecutions"] = history.Count;
                    statistics["SuccessfulExecutions"] = history.Count(h => h.ExecutionStatus == "Completed");
                    statistics["FailedExecutions"] = history.Count(h => h.ExecutionStatus == "Failed");

                    if (history.Any())
                    {
                        statistics["AverageExecutionTime"] = history
                            .Where(h => h.ExecutionStatus == "Completed")
                            .Select(h => h.ExecutionDuration.TotalMilliseconds)
                            .DefaultIfEmpty(0)
                            .Average();

                        statistics["MaxExecutionTime"] = history
                            .Where(h => h.ExecutionStatus == "Completed")
                            .Select(h => h.ExecutionDuration.TotalMilliseconds)
                            .DefaultIfEmpty(0)
                            .Max();

                        statistics["MinExecutionTime"] = history
                            .Where(h => h.ExecutionStatus == "Completed")
                            .Select(h => h.ExecutionDuration.TotalMilliseconds)
                            .DefaultIfEmpty(0)
                            .Min();

                        var lastExecution = history
                            .OrderByDescending(h => h.StartTimestamp)
                            .FirstOrDefault();

                        if (lastExecution != null)
                        {
                            statistics["LastExecutionTime"] = lastExecution.StartTimestamp;
                            statistics["LastExecutionStatus"] = lastExecution.ExecutionStatus ?? "Unknown";
                        }
                        else
                        {
                            statistics["LastExecutionTime"] = DateTime.MinValue;
                            statistics["LastExecutionStatus"] = "Unknown";
                        }
                    }
                }

                // Add active executions
                if (_activeFlows.TryGetValue(flowId, out var activeExecutions))
                {
                    statistics["ActiveExecutions"] = activeExecutions.Count;
                }
                else
                {
                    statistics["ActiveExecutions"] = 0;
                }
            }

            return statistics;
        }

        /// <inheritdoc/>
        public void RegisterFlowExecution(ActiveFlowStatus flowStatus)
        {
            lock (_lock)
            {
                try
                {
                    // Add to active flows
                    if (!_activeFlows.TryGetValue(flowStatus.FlowId, out var flowExecutions))
                    {
                        flowExecutions = new Dictionary<string, ActiveFlowStatus>();
                        _activeFlows[flowStatus.FlowId] = flowExecutions;
                    }

                    flowExecutions[flowStatus.ExecutionId] = flowStatus;

                    // Add to flow history
                    if (!_flowHistory.TryGetValue(flowStatus.FlowId, out var history))
                    {
                        history = new List<ActiveFlowStatus>();
                        _flowHistory[flowStatus.FlowId] = history;
                    }

                    history.Add(flowStatus);

                    // Limit history size
                    if (history.Count > 100)
                    {
                        history.RemoveAt(0);
                    }

                    _logger.LogInformation("Registered flow execution {ExecutionId} for flow {FlowId}",
                        flowStatus.ExecutionId, flowStatus.FlowId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering flow execution {ExecutionId} for flow {FlowId}",
                        flowStatus.ExecutionId, flowStatus.FlowId);
                }
            }
        }

        /// <inheritdoc/>
        public void UpdateFlowExecutionStatus(string flowId, string executionId, string status, string currentStep)
        {
            lock (_lock)
            {
                try
                {
                    if (_activeFlows.TryGetValue(flowId, out var flowExecutions) &&
                        flowExecutions.TryGetValue(executionId, out var flowStatus))
                    {
                        flowStatus.ExecutionStatus = status;
                        flowStatus.CurrentStep = currentStep;
                        flowStatus.ExecutionDuration = DateTime.UtcNow - flowStatus.StartTimestamp;

                        // If the flow is completed or failed, update the history and remove from active flows
                        if (status == "Completed" || status == "Failed")
                        {
                            flowExecutions.Remove(executionId);

                            if (flowExecutions.Count == 0)
                            {
                                _activeFlows.Remove(flowId);
                            }
                        }

                        _logger.LogInformation("Updated flow execution {ExecutionId} for flow {FlowId} to status {Status}",
                            executionId, flowId, status);
                    }
                    else
                    {
                        _logger.LogWarning("Flow execution {ExecutionId} for flow {FlowId} not found",
                            executionId, flowId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating flow execution {ExecutionId} for flow {FlowId}",
                        executionId, flowId);
                }
            }
        }
    }
}
