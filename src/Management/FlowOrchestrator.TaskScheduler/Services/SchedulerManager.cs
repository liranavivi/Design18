using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Interfaces;
using FlowOrchestrator.Management.Scheduling.Adapters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FlowOrchestrator.Management.Scheduling.Services
{
    /// <summary>
    /// Manager for the scheduler.
    /// </summary>
    public class SchedulerManager
    {
        private readonly ILogger<SchedulerManager> _logger;
        private readonly IQuartzSchedulerService _quartzSchedulerService;
        private readonly TaskSchedulerService _taskSchedulerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="quartzSchedulerService">The Quartz scheduler service.</param>
        /// <param name="taskSchedulerService">The task scheduler service.</param>
        public SchedulerManager(
            ILogger<SchedulerManager> logger,
            IQuartzSchedulerService quartzSchedulerService,
            TaskSchedulerService taskSchedulerService)
        {
            _logger = logger;
            _quartzSchedulerService = quartzSchedulerService;
            _taskSchedulerService = taskSchedulerService;
        }

        /// <summary>
        /// Initializes the scheduler.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing scheduler");

                // Start the scheduler
                await _quartzSchedulerService.StartAsync();

                // Load scheduled flows
                await LoadScheduledFlowsAsync();

                _logger.LogInformation("Scheduler initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing scheduler");
                throw;
            }
        }

        /// <summary>
        /// Shuts down the scheduler.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ShutdownAsync()
        {
            try
            {
                _logger.LogInformation("Shutting down scheduler");

                // Shutdown the scheduler
                await _quartzSchedulerService.StopAsync();

                _logger.LogInformation("Scheduler shut down successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down scheduler");
                throw;
            }
        }

        /// <summary>
        /// Loads scheduled flows from the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadScheduledFlowsAsync()
        {
            try
            {
                _logger.LogInformation("Loading scheduled flows");

                // In a real implementation, we would retrieve scheduled flows from a repository
                // For now, we'll create dummy entities for demonstration purposes
                // Create task scheduler entities
                var taskScheduler1 = new TaskSchedulerEntity
                {
                    SchedulerId = "task-scheduler-001",
                    Name = "Daily Task Scheduler",
                    ScheduleType = ScheduleType.CRON,
                    ScheduleExpression = "0 0/5 * * * ?", // Every 5 minutes
                    IsEnabled = true
                };

                var taskScheduler2 = new TaskSchedulerEntity
                {
                    SchedulerId = "task-scheduler-002",
                    Name = "Hourly Task Scheduler",
                    ScheduleType = ScheduleType.CRON,
                    ScheduleExpression = "0 0 * * * ?", // Every hour
                    IsEnabled = true
                };

                // Create scheduled flow entities
                var scheduledFlow1 = new ScheduledFlowEntity
                {
                    ScheduledFlowId = "scheduled-flow-001",
                    Name = "Daily Import Flow",
                    FlowEntityId = "flow-001",
                    TaskSchedulerEntityId = "task-scheduler-001",
                    ExecutionParameters = new Dictionary<string, object>
                    {
                        { "param1", "value1" },
                        { "param2", "value2" }
                    },
                    IsEnabled = true
                };

                var scheduledFlow2 = new ScheduledFlowEntity
                {
                    ScheduledFlowId = "scheduled-flow-002",
                    Name = "Hourly Export Flow",
                    FlowEntityId = "flow-002",
                    TaskSchedulerEntityId = "task-scheduler-002",
                    ExecutionParameters = new Dictionary<string, object>
                    {
                        { "param1", "value1" },
                        { "param2", "value2" }
                    },
                    IsEnabled = true
                };

                // Create adapters for the entities
                var taskSchedulerAdapter1 = new TaskSchedulerEntityAdapter(taskScheduler1);
                var taskSchedulerAdapter2 = new TaskSchedulerEntityAdapter(taskScheduler2);
                var scheduledFlowAdapter1 = new ScheduledFlowEntityAdapter(scheduledFlow1);
                var scheduledFlowAdapter2 = new ScheduledFlowEntityAdapter(scheduledFlow2);

                // Create a list of tuples with the adapters
                var scheduledFlows = new List<(ITaskSchedulerEntity, IScheduledFlowEntity)>
                {
                    (taskSchedulerAdapter1, scheduledFlowAdapter1),
                    (taskSchedulerAdapter2, scheduledFlowAdapter2)
                };

                // Schedule each flow
                foreach (var (taskSchedulerEntity, scheduledFlowEntity) in scheduledFlows)
                {
                    if (taskSchedulerEntity.Enabled == true && scheduledFlowEntity.Enabled == true)
                    {
                        _logger.LogInformation("Scheduling flow {ScheduledFlowEntityId} with task scheduler {TaskSchedulerEntityId}",
                            scheduledFlowEntity.ServiceId, taskSchedulerEntity.ServiceId);

                        try
                        {
                            // Schedule the job
                            var jobKey = await _quartzSchedulerService.ScheduleJobAsync(taskSchedulerEntity, scheduledFlowEntity);

                            _logger.LogInformation("Flow {ScheduledFlowEntityId} scheduled successfully with job key {JobKey}",
                                scheduledFlowEntity.ServiceId, jobKey);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error scheduling flow {ScheduledFlowEntityId}", scheduledFlowEntity.ServiceId);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Skipping disabled flow {ScheduledFlowEntityId} or task scheduler {TaskSchedulerEntityId}",
                            scheduledFlowEntity.ServiceId, taskSchedulerEntity.ServiceId);
                    }
                }

                _logger.LogInformation("Scheduled flows loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scheduled flows");
                throw;
            }
        }
    }
}
