using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Models;
using Quartz;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Interfaces
{
    /// <summary>
    /// Defines the interface for the Quartz scheduler service.
    /// </summary>
    public interface IQuartzSchedulerService
    {
        /// <summary>
        /// Initializes the scheduler.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InitializeAsync();

        /// <summary>
        /// Starts the scheduler.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StartAsync();

        /// <summary>
        /// Stops the scheduler.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StopAsync();

        /// <summary>
        /// Schedules a job based on a task scheduler entity.
        /// </summary>
        /// <param name="taskSchedulerEntity">The task scheduler entity.</param>
        /// <returns>A task representing the asynchronous operation with the job key.</returns>
        Task<JobKey> ScheduleJobAsync(ITaskSchedulerEntity taskSchedulerEntity);

        /// <summary>
        /// Schedules a job based on a task scheduler entity and flow entity.
        /// </summary>
        /// <param name="taskSchedulerEntity">The task scheduler entity.</param>
        /// <param name="scheduledFlowEntity">The scheduled flow entity.</param>
        /// <returns>A task representing the asynchronous operation with the job key.</returns>
        Task<JobKey> ScheduleJobAsync(ITaskSchedulerEntity taskSchedulerEntity, IScheduledFlowEntity scheduledFlowEntity);

        /// <summary>
        /// Unschedules a job.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>A task representing the asynchronous operation with a value indicating whether the job was unscheduled.</returns>
        Task<bool> UnscheduleJobAsync(JobKey jobKey);

        /// <summary>
        /// Pauses a job.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PauseJobAsync(JobKey jobKey);

        /// <summary>
        /// Resumes a job.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ResumeJobAsync(JobKey jobKey);

        /// <summary>
        /// Triggers a job immediately.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task TriggerJobAsync(JobKey jobKey);

        /// <summary>
        /// Gets the scheduler status.
        /// </summary>
        /// <returns>The scheduler status.</returns>
        Task<SchedulerStatus> GetSchedulerStatusAsync();

        /// <summary>
        /// Gets the job status.
        /// </summary>
        /// <param name="jobKey">The job key.</param>
        /// <returns>The job status.</returns>
        Task<JobStatus> GetJobStatusAsync(JobKey jobKey);

        /// <summary>
        /// Gets all scheduled jobs.
        /// </summary>
        /// <returns>A collection of job keys.</returns>
        Task<IReadOnlyCollection<JobKey>> GetAllJobsAsync();
    }
}
