using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Jobs
{
    /// <summary>
    /// Job for executing a task scheduler.
    /// </summary>
    [DisallowConcurrentExecution]
    public class TaskSchedulerJob : IJob
    {
        private readonly ILogger<TaskSchedulerJob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TaskSchedulerJob(ILogger<TaskSchedulerJob> logger)
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
                _logger.LogInformation("Executing task scheduler job at {time}", DateTime.UtcNow);

                // Get the task scheduler entity ID from the job data map
                var dataMap = context.JobDetail.JobDataMap;
                var taskSchedulerEntityId = dataMap.GetString("TaskSchedulerEntityId");

                if (string.IsNullOrEmpty(taskSchedulerEntityId))
                {
                    _logger.LogError("Task scheduler entity ID is missing in job data map");
                    return;
                }

                // In a real implementation, this would trigger the task scheduler execution through a message bus
                // For now, we'll just log the execution
                _logger.LogInformation("Triggered task scheduler execution for task scheduler entity ID {TaskSchedulerEntityId}",
                    taskSchedulerEntityId);

                // Simulate some work
                await Task.Delay(100);

                _logger.LogInformation("Task scheduler execution completed for task scheduler entity ID {TaskSchedulerEntityId}",
                    taskSchedulerEntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing task scheduler job");
                throw;
            }
        }
    }
}
