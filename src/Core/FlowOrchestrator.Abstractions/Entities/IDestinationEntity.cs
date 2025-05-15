using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Represents a destination entity in the FlowOrchestrator system.
    /// </summary>
    public interface IDestinationEntity : IEntity, IService
    {
        /// <summary>
        /// Gets the destination type.
        /// </summary>
        string DestinationType { get; }

        /// <summary>
        /// Gets the connection parameters.
        /// </summary>
        Dictionary<string, string> ConnectionParameters { get; }
    }
}
