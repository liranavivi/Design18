using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Represents a destination assignment entity in the FlowOrchestrator system.
    /// </summary>
    public interface IDestinationAssignmentEntity : IEntity, IService
    {
        /// <summary>
        /// Gets the destination entity identifier.
        /// </summary>
        string DestinationEntityId { get; }

        /// <summary>
        /// Gets the exporter service identifier.
        /// </summary>
        string ExporterServiceId { get; }

        /// <summary>
        /// Gets the exporter service version.
        /// </summary>
        string ExporterServiceVersion { get; }

        /// <summary>
        /// Gets the assignment parameters.
        /// </summary>
        Dictionary<string, string> AssignmentParameters { get; }
    }
}
