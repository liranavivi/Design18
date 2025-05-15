using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Represents a task scheduler entity in the FlowOrchestrator system.
    /// </summary>
    public interface ITaskSchedulerEntity : IEntity, IService
    {
        /// <summary>
        /// Gets the scheduler name.
        /// </summary>
        string SchedulerName { get; }

        /// <summary>
        /// Gets the scheduler type.
        /// </summary>
        string SchedulerType { get; }

        /// <summary>
        /// Gets the schedule expression.
        /// </summary>
        string ScheduleExpression { get; }

        /// <summary>
        /// Gets the schedule parameters.
        /// </summary>
        Dictionary<string, string> ScheduleParameters { get; }

        /// <summary>
        /// Gets a value indicating whether the scheduler is enabled.
        /// </summary>
        bool? Enabled { get; }
    }
}
