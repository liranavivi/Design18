using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Represents a scheduled flow entity in the FlowOrchestrator system.
    /// </summary>
    public interface IScheduledFlowEntity : IEntity, IService
    {
        /// <summary>
        /// Gets the flow entity identifier.
        /// </summary>
        string FlowEntityId { get; }

        /// <summary>
        /// Gets the task scheduler entity identifier.
        /// </summary>
        string TaskSchedulerEntityId { get; }

        /// <summary>
        /// Gets the flow parameters.
        /// </summary>
        Dictionary<string, string> FlowParameters { get; }

        /// <summary>
        /// Gets a value indicating whether the scheduled flow is enabled.
        /// </summary>
        bool? Enabled { get; }
    }
}
