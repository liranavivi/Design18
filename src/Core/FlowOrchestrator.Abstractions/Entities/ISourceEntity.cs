using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Represents a source entity in the FlowOrchestrator system.
    /// </summary>
    public interface ISourceEntity : IEntity, IService
    {
        /// <summary>
        /// Gets the source type.
        /// </summary>
        string SourceType { get; }

        /// <summary>
        /// Gets the connection parameters.
        /// </summary>
        Dictionary<string, string> ConnectionParameters { get; }
    }
}
