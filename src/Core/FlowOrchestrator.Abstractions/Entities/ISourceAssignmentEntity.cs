using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Represents a source assignment entity in the FlowOrchestrator system.
    /// </summary>
    public interface ISourceAssignmentEntity : IEntity, IService
    {
        /// <summary>
        /// Gets the source entity identifier.
        /// </summary>
        string SourceEntityId { get; }

        /// <summary>
        /// Gets the importer service identifier.
        /// </summary>
        string ImporterServiceId { get; }

        /// <summary>
        /// Gets the importer service version.
        /// </summary>
        string ImporterServiceVersion { get; }

        /// <summary>
        /// Gets the assignment parameters.
        /// </summary>
        Dictionary<string, string> AssignmentParameters { get; }
    }
}
