using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.Json;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Jobs
{
    /// <summary>
    /// Job for executing a flow.
    /// </summary>
    [DisallowConcurrentExecution]
    public class FlowExecutionJob : IJob
    {
        private readonly ILogger<FlowExecutionJob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowExecutionJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FlowExecutionJob(ILogger<FlowExecutionJob> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes the job.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Executing flow job at {time}", DateTime.UtcNow);

                // Get the flow entity ID and task scheduler entity ID from the job data map
                var dataMap = context.JobDetail.JobDataMap;
                var flowEntityId = dataMap.GetString("FlowEntityId");
                var taskSchedulerEntityId = dataMap.GetString("TaskSchedulerEntityId");
                var flowParametersJson = dataMap.GetString("FlowParameters");

                if (string.IsNullOrEmpty(flowEntityId) || string.IsNullOrEmpty(taskSchedulerEntityId))
                {
                    _logger.LogError("Flow entity ID or task scheduler entity ID is missing in job data map");
                    return;
                }

                // Parse flow parameters
                Dictionary<string, string>? flowParameters = null;
                if (!string.IsNullOrEmpty(flowParametersJson))
                {
                    flowParameters = JsonSerializer.Deserialize<Dictionary<string, string>>(flowParametersJson);
                }

                // In a real implementation, this would trigger the flow execution through a message bus
                // For now, we'll just log the execution
                _logger.LogInformation("Triggered flow execution for flow entity ID {FlowEntityId} with task scheduler entity ID {TaskSchedulerEntityId}",
                    flowEntityId, taskSchedulerEntityId);

                if (flowParameters != null && flowParameters.Count > 0)
                {
                    _logger.LogInformation("Flow parameters: {FlowParameters}", JsonSerializer.Serialize(flowParameters));
                }

                // Simulate some work
                await Task.Delay(100);

                _logger.LogInformation("Flow execution completed for flow entity ID {FlowEntityId}", flowEntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing flow job");
                throw;
            }
        }
    }
}
