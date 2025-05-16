using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Domain.Entities;
using Quartz.Impl.Matchers;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Interfaces;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Jobs;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Models;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using System.Text.Json;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Services
{
    /// <summary>
    /// Service for managing Quartz schedulers.
    /// </summary>
    public class QuartzSchedulerService : IQuartzSchedulerService
    {
        private readonly ILogger<QuartzSchedulerService> _logger;
        private readonly QuartzConfiguration _configuration;
        private readonly IQuartzJobFactory _jobFactory;
        private IScheduler? _scheduler;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuartzSchedulerService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="jobFactory">The job factory.</param>
        public QuartzSchedulerService(
            ILogger<QuartzSchedulerService> logger,
            IOptions<QuartzConfiguration> configuration,
            IQuartzJobFactory jobFactory)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _jobFactory = jobFactory;
        }

        /// <summary>
        /// Initializes the scheduler.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing Quartz scheduler");

                // Create scheduler properties
                var properties = new NameValueCollection
                {
                    ["quartz.scheduler.instanceName"] = _configuration.SchedulerName,
                    ["quartz.scheduler.instanceId"] = _configuration.InstanceId,
                    ["quartz.threadPool.threadCount"] = _configuration.ThreadCount.ToString(),
                    ["quartz.jobStore.type"] = _configuration.JobStoreType
                    // Removed serializer type as it's causing issues
                };

                // Create scheduler factory
                var factory = new StdSchedulerFactory(properties);

                // Get scheduler
                _scheduler = await factory.GetScheduler();

                // Set job factory
                _scheduler.JobFactory = _jobFactory;

                _logger.LogInformation("Quartz scheduler initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Quartz scheduler");
                throw;
            }
        }

        /// <summary>
        /// Starts the scheduler.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartAsync()
        {
            try
            {
                if (_scheduler == null)
                {
                    await InitializeAsync();
                }

                if (_scheduler != null && !_scheduler.IsStarted)
                {
                    _logger.LogInformation("Starting Quartz scheduler");
                    await _scheduler.Start();
                    _logger.LogInformation("Quartz scheduler started");
                }
                else
                {
                    _logger.LogWarning("Quartz scheduler is already started or not initialized");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Quartz scheduler");
                throw;
            }
        }

        /// <summary>
        /// Stops the scheduler.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StopAsync()
        {
            try
            {
                if (_scheduler != null && _scheduler.IsStarted)
                {
                    _logger.LogInformation("Stopping Quartz scheduler");
                    await _scheduler.Standby();
                    _logger.LogInformation("Quartz scheduler stopped");
                }
                else
                {
                    _logger.LogWarning("Quartz scheduler is already stopped or not initialized");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Quartz scheduler");
                throw;
            }
        }

        /// <summary>
        /// Schedules a job based on a task scheduler entity.
        /// </summary>
        /// <param name="taskSchedulerEntity">The task scheduler entity.</param>
        /// <returns>A task representing the asynchronous operation with the job key.</returns>
        public async Task<JobKey> ScheduleJobAsync(ITaskSchedulerEntity taskSchedulerEntity)
        {
            try
            {
                if (_scheduler == null)
                {
                    await InitializeAsync();
                    await StartAsync();
                }

                if (_scheduler == null)
                {
                    throw new InvalidOperationException("Scheduler is not initialized");
                }

                _logger.LogInformation("Scheduling job for task scheduler entity {TaskSchedulerEntityId}", taskSchedulerEntity.ServiceId);

                // Create job key
                var jobKey = new JobKey($"TaskScheduler_{taskSchedulerEntity.ServiceId}", "TaskSchedulers");

                // Create job detail
                var jobDetail = JobBuilder.Create<TaskSchedulerJob>()
                    .WithIdentity(jobKey)
                    .WithDescription($"Task scheduler job for {taskSchedulerEntity.SchedulerName}")
                    .UsingJobData("TaskSchedulerEntityId", taskSchedulerEntity.ServiceId)
                    .StoreDurably()
                    .Build();

                // Create trigger
                var triggerKey = new TriggerKey($"TaskSchedulerTrigger_{taskSchedulerEntity.ServiceId}", "TaskSchedulers");
                var scheduleType = Enum.Parse<ScheduleType>(taskSchedulerEntity.SchedulerType, true);

                // Use a simple trigger for testing
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)
                        .RepeatForever())
                    .WithDescription($"Trigger for task scheduler {taskSchedulerEntity.SchedulerName}")
                    .ForJob(jobKey)
                    .Build();

                // Schedule job
                if (await _scheduler.CheckExists(jobKey))
                {
                    _logger.LogInformation("Job already exists, rescheduling");
                    await _scheduler.DeleteJob(jobKey);
                }

                await _scheduler.ScheduleJob(jobDetail, trigger);

                _logger.LogInformation("Job scheduled for task scheduler entity {TaskSchedulerEntityId}", taskSchedulerEntity.ServiceId);

                return jobKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling job for task scheduler entity {TaskSchedulerEntityId}", taskSchedulerEntity.ServiceId);
                throw;
            }
        }

        /// <summary>
        /// Schedules a job based on a task scheduler entity and flow entity.
        /// </summary>
        /// <param name="taskSchedulerEntity">The task scheduler entity.</param>
        /// <param name="scheduledFlowEntity">The scheduled flow entity.</param>
        /// <returns>A task representing the asynchronous operation with the job key.</returns>
        public async Task<JobKey> ScheduleJobAsync(ITaskSchedulerEntity taskSchedulerEntity, IScheduledFlowEntity scheduledFlowEntity)
        {
            try
            {
                if (_scheduler == null)
                {
                    await InitializeAsync();
                    await StartAsync();
                }

                if (_scheduler == null)
                {
                    throw new InvalidOperationException("Scheduler is not initialized");
                }

                _logger.LogInformation("Scheduling job for scheduled flow entity {ScheduledFlowEntityId}", scheduledFlowEntity.ServiceId);

                // Create job key
                var jobKey = new JobKey($"Flow_{scheduledFlowEntity.ServiceId}", "Flows");

                // Create job detail
                var jobDetail = JobBuilder.Create<FlowExecutionJob>()
                    .WithIdentity(jobKey)
                    .WithDescription($"Flow execution job for {scheduledFlowEntity.ServiceId}")
                    .UsingJobData("FlowEntityId", scheduledFlowEntity.FlowEntityId)
                    .UsingJobData("TaskSchedulerEntityId", scheduledFlowEntity.TaskSchedulerEntityId)
                    .UsingJobData("FlowParameters", JsonSerializer.Serialize(scheduledFlowEntity.FlowParameters))
                    .StoreDurably()
                    .Build();

                // Create trigger
                var triggerKey = new TriggerKey($"FlowTrigger_{scheduledFlowEntity.ServiceId}", "Flows");
                var scheduleType = Enum.Parse<ScheduleType>(taskSchedulerEntity.SchedulerType, true);

                // Use a simple trigger for testing
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(60)
                        .RepeatForever())
                    .WithDescription($"Trigger for flow {scheduledFlowEntity.ServiceId}")
                    .ForJob(jobKey)
                    .Build();

                // Schedule job
                if (await _scheduler.CheckExists(jobKey))
                {
                    _logger.LogInformation("Job already exists, rescheduling");
                    await _scheduler.DeleteJob(jobKey);
                }

                await _scheduler.ScheduleJob(jobDetail, trigger);

                _logger.LogInformation("Job scheduled for scheduled flow entity {ScheduledFlowEntityId}", scheduledFlowEntity.ServiceId);

                return jobKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling job for scheduled flow entity {ScheduledFlowEntityId}", scheduledFlowEntity.ServiceId);
                throw;
            }
        }

        /// <summary>
        /// Unschedules a job.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>A task representing the asynchronous operation with a value indicating whether the job was unscheduled.</returns>
        public async Task<bool> UnscheduleJobAsync(JobKey jobKey)
        {
            try
            {
                if (_scheduler == null)
                {
                    throw new InvalidOperationException("Scheduler is not initialized");
                }

                _logger.LogInformation("Unscheduling job {JobKey}", jobKey);

                if (await _scheduler.CheckExists(jobKey))
                {
                    await _scheduler.DeleteJob(jobKey);
                    _logger.LogInformation("Job {JobKey} unscheduled", jobKey);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Job {JobKey} does not exist", jobKey);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unscheduling job {JobKey}", jobKey);
                throw;
            }
        }

        /// <summary>
        /// Pauses a job.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PauseJobAsync(JobKey jobKey)
        {
            try
            {
                if (_scheduler == null)
                {
                    throw new InvalidOperationException("Scheduler is not initialized");
                }

                _logger.LogInformation("Pausing job {JobKey}", jobKey);

                if (await _scheduler.CheckExists(jobKey))
                {
                    await _scheduler.PauseJob(jobKey);
                    _logger.LogInformation("Job {JobKey} paused", jobKey);
                }
                else
                {
                    _logger.LogWarning("Job {JobKey} does not exist", jobKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing job {JobKey}", jobKey);
                throw;
            }
        }

        /// <summary>
        /// Resumes a job.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ResumeJobAsync(JobKey jobKey)
        {
            try
            {
                if (_scheduler == null)
                {
                    throw new InvalidOperationException("Scheduler is not initialized");
                }

                _logger.LogInformation("Resuming job {JobKey}", jobKey);

                if (await _scheduler.CheckExists(jobKey))
                {
                    await _scheduler.ResumeJob(jobKey);
                    _logger.LogInformation("Job {JobKey} resumed", jobKey);
                }
                else
                {
                    _logger.LogWarning("Job {JobKey} does not exist", jobKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming job {JobKey}", jobKey);
                throw;
            }
        }

        /// <summary>
        /// Triggers a job immediately.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task TriggerJobAsync(JobKey jobKey)
        {
            try
            {
                if (_scheduler == null)
                {
                    throw new InvalidOperationException("Scheduler is not initialized");
                }

                _logger.LogInformation("Triggering job {JobKey}", jobKey);

                if (await _scheduler.CheckExists(jobKey))
                {
                    await _scheduler.TriggerJob(jobKey);
                    _logger.LogInformation("Job {JobKey} triggered", jobKey);
                }
                else
                {
                    _logger.LogWarning("Job {JobKey} does not exist", jobKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering job {JobKey}", jobKey);
                throw;
            }
        }

        /// <summary>
        /// Gets the scheduler status.
        /// </summary>
        /// <returns>The scheduler status.</returns>
        public async Task<SchedulerStatus> GetSchedulerStatusAsync()
        {
            try
            {
                if (_scheduler == null)
                {
                    return SchedulerStatus.NotInitialized;
                }

                if (_scheduler.IsShutdown)
                {
                    return SchedulerStatus.Stopped;
                }

                if (_scheduler.InStandbyMode)
                {
                    return SchedulerStatus.Paused;
                }

                if (_scheduler.IsStarted)
                {
                    return SchedulerStatus.Running;
                }

                return SchedulerStatus.Initialized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scheduler status");
                return SchedulerStatus.Error;
            }
        }

        /// <summary>
        /// Gets the job status.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>The job status.</returns>
        public async Task<JobStatus> GetJobStatusAsync(JobKey jobKey)
        {
            try
            {
                if (_scheduler == null)
                {
                    throw new InvalidOperationException("Scheduler is not initialized");
                }

                if (!await _scheduler.CheckExists(jobKey))
                {
                    return JobStatus.NotFound;
                }

                var jobDetail = await _scheduler.GetJobDetail(jobKey);
                var triggers = await _scheduler.GetTriggersOfJob(jobKey);
                var currentlyExecuting = await _scheduler.GetCurrentlyExecutingJobs();

                if (currentlyExecuting.Any(j => j.JobDetail.Key.Equals(jobKey)))
                {
                    return JobStatus.Running;
                }

                if (triggers.All(t => _scheduler.GetTriggerState(t.Key).Result == TriggerState.Paused))
                {
                    return JobStatus.Paused;
                }

                if (triggers.Any())
                {
                    return JobStatus.Scheduled;
                }

                return JobStatus.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job status for {JobKey}", jobKey);
                return JobStatus.Error;
            }
        }

        /// <summary>
        /// Gets all scheduled jobs.
        /// </summary>
        /// <returns>A collection of job keys.</returns>
        public async Task<IReadOnlyCollection<JobKey>> GetAllJobsAsync()
        {
            try
            {
                if (_scheduler == null)
                {
                    throw new InvalidOperationException("Scheduler is not initialized");
                }

                var jobGroups = await _scheduler.GetJobGroupNames();
                var jobKeys = new List<JobKey>();

                foreach (var group in jobGroups)
                {
                    jobKeys.AddRange(await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group)));
                }

                return jobKeys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all jobs");
                throw;
            }
        }
    }
}
