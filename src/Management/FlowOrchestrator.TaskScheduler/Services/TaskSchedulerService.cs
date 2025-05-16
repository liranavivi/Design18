using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Interfaces;
using FlowOrchestrator.Management.Scheduling.Adapters;
using FlowOrchestrator.Management.Scheduling.Messaging.Commands;
using FlowOrchestrator.Management.Scheduling.Messaging.Results;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ExecutionStatus = FlowOrchestrator.Abstractions.Common.ExecutionStatus;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.Json;

namespace FlowOrchestrator.Management.Scheduling.Services
{
    /// <summary>
    /// Service for managing task schedulers and scheduled flows.
    /// </summary>
    public class TaskSchedulerService
    {
        private readonly ILogger<TaskSchedulerService> _logger;
        private readonly IQuartzSchedulerService _quartzSchedulerService;
        private readonly IMessageBus _messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="quartzSchedulerService">The Quartz scheduler service.</param>
        /// <param name="messageBus">The message bus.</param>
        public TaskSchedulerService(
            ILogger<TaskSchedulerService> logger,
            IQuartzSchedulerService quartzSchedulerService,
            IMessageBus messageBus)
        {
            _logger = logger;
            _quartzSchedulerService = quartzSchedulerService;
            _messageBus = messageBus;
        }

        /// <summary>
        /// Schedules a flow for execution.
        /// </summary>
        /// <param name="command">The schedule flow command.</param>
        /// <returns>The flow scheduling result.</returns>
        public async Task<FlowSchedulingResult> ScheduleFlowAsync(ScheduleFlowCommand command)
        {
            try
            {
                _logger.LogInformation("Scheduling flow {ScheduledFlowEntityId} with task scheduler {TaskSchedulerEntityId}",
                    command.ScheduledFlowEntityId, command.TaskSchedulerEntityId);

                // In a real implementation, we would retrieve the task scheduler entity and scheduled flow entity from a repository
                // For now, we'll create dummy entities for demonstration purposes
                var taskSchedulerEntity = new TaskSchedulerEntity
                {
                    SchedulerId = command.TaskSchedulerEntityId,
                    Name = $"Scheduler for {command.ScheduledFlowEntityId}",
                    ScheduleType = ScheduleType.CRON,
                    ScheduleExpression = "0 0/5 * * * ?", // Every 5 minutes
                    IsEnabled = true
                };

                var scheduledFlowEntity = new ScheduledFlowEntity
                {
                    ScheduledFlowId = command.ScheduledFlowEntityId,
                    Name = $"Scheduled Flow {command.ScheduledFlowEntityId}",
                    FlowEntityId = command.FlowEntityId,
                    TaskSchedulerEntityId = command.TaskSchedulerEntityId,
                    ExecutionParameters = new Dictionary<string, object>(command.FlowParameters.ToDictionary(kv => kv.Key, kv => (object)kv.Value)),
                    IsEnabled = true
                };

                // Create adapters for the entities
                var taskSchedulerAdapter = new TaskSchedulerEntityAdapter(taskSchedulerEntity);
                var scheduledFlowAdapter = new ScheduledFlowEntityAdapter(scheduledFlowEntity);

                // Schedule the job
                var jobKey = await _quartzSchedulerService.ScheduleJobAsync(taskSchedulerAdapter, scheduledFlowAdapter);

                // Get the trigger key
                var triggerKey = new TriggerKey($"FlowTrigger_{scheduledFlowEntity.ScheduledFlowId}", "Flows");

                // Get the job status
                var status = await _quartzSchedulerService.GetJobStatusAsync(jobKey);

                // For now, we'll just set a default next execution time
                var nextExecutionTime = DateTime.UtcNow.AddMinutes(5);

                _logger.LogInformation("Flow {ScheduledFlowEntityId} scheduled successfully with job key {JobKey}, next execution at {NextExecutionTime}",
                    command.ScheduledFlowEntityId, jobKey, nextExecutionTime);

                return new FlowSchedulingResult(
                    command,
                    true,
                    jobKey.ToString(),
                    triggerKey.ToString(),
                    nextExecutionTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling flow {ScheduledFlowEntityId}", command.ScheduledFlowEntityId);

                var errorDetails = new Dictionary<string, object>
                {
                    { "ExceptionType", ex.GetType().Name },
                    { "ExceptionMessage", ex.Message },
                    { "StackTrace", ex.StackTrace ?? string.Empty }
                };

                return new FlowSchedulingResult(
                    command,
                    $"Error scheduling flow: {ex.Message}",
                    errorDetails);
            }
        }

        /// <summary>
        /// Triggers a flow execution immediately.
        /// </summary>
        /// <param name="command">The trigger flow command.</param>
        /// <returns>The flow execution result.</returns>
        public async Task<FlowExecutionResult> TriggerFlowAsync(TriggerFlowCommand command)
        {
            try
            {
                _logger.LogInformation("Triggering flow {ScheduledFlowEntityId} with task scheduler {TaskSchedulerEntityId}",
                    command.ScheduledFlowEntityId, command.TaskSchedulerEntityId);

                // Create a job key for the flow
                var jobKey = new JobKey($"Flow_{command.ScheduledFlowEntityId}", "Flows");

                // Check if the job exists
                var allJobs = await _quartzSchedulerService.GetAllJobsAsync();
                var jobExists = allJobs.Any(j => j.Equals(jobKey));

                if (!jobExists)
                {
                    _logger.LogWarning("Job for flow {ScheduledFlowEntityId} does not exist, creating it", command.ScheduledFlowEntityId);

                    // In a real implementation, we would retrieve the task scheduler entity and scheduled flow entity from a repository
                    // For now, we'll create dummy entities for demonstration purposes
                    var taskSchedulerEntity = new TaskSchedulerEntity
                    {
                        SchedulerId = command.TaskSchedulerEntityId,
                        Name = $"Scheduler for {command.ScheduledFlowEntityId}",
                        ScheduleType = ScheduleType.CRON,
                        ScheduleExpression = "0 0/5 * * * ?", // Every 5 minutes
                        IsEnabled = true
                    };

                    var scheduledFlowEntity = new ScheduledFlowEntity
                    {
                        ScheduledFlowId = command.ScheduledFlowEntityId,
                        Name = $"Scheduled Flow {command.ScheduledFlowEntityId}",
                        FlowEntityId = command.FlowEntityId,
                        TaskSchedulerEntityId = command.TaskSchedulerEntityId,
                        ExecutionParameters = new Dictionary<string, object>(command.FlowParameters.ToDictionary(kv => kv.Key, kv => (object)kv.Value)),
                        IsEnabled = true
                    };

                    // Create adapters for the entities
                    var taskSchedulerAdapter = new TaskSchedulerEntityAdapter(taskSchedulerEntity);
                    var scheduledFlowAdapter = new ScheduledFlowEntityAdapter(scheduledFlowEntity);

                    // Schedule the job
                    jobKey = await _quartzSchedulerService.ScheduleJobAsync(taskSchedulerAdapter, scheduledFlowAdapter);
                }

                // Trigger the job
                await _quartzSchedulerService.TriggerJobAsync(jobKey);

                _logger.LogInformation("Flow {ScheduledFlowEntityId} triggered successfully", command.ScheduledFlowEntityId);

                // In a real implementation, we would wait for the flow execution to complete and return the result
                // For now, we'll just return a success result
                return new FlowExecutionResult(
                    command,
                    true,
                    ExecutionStatus.COMPLETED,
                    command.Context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering flow {ScheduledFlowEntityId}", command.ScheduledFlowEntityId);

                var errorDetails = new Dictionary<string, object>
                {
                    { "ExceptionType", ex.GetType().Name },
                    { "ExceptionMessage", ex.Message },
                    { "StackTrace", ex.StackTrace ?? string.Empty }
                };

                return new FlowExecutionResult(
                    command,
                    $"Error triggering flow: {ex.Message}",
                    errorDetails,
                    command.Context);
            }
        }
    }
}
