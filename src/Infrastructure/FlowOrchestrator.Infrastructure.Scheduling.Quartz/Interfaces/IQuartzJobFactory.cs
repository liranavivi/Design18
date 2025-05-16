using Quartz;
using Quartz.Spi;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Interfaces
{
    /// <summary>
    /// Defines the interface for the Quartz job factory.
    /// </summary>
    public interface IQuartzJobFactory : IJobFactory
    {
        /// <summary>
        /// Creates a new instance of a job.
        /// </summary>
        /// <param name="bundle">The TriggerFiredBundle from which to create the job.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <returns>The new job instance.</returns>
        new IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler);

        /// <summary>
        /// Destroys a job instance.
        /// </summary>
        /// <param name="job">The job to destroy.</param>
        new void ReturnJob(IJob job);
    }
}
