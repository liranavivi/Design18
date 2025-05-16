using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Versioning.Models
{
    /// <summary>
    /// Represents the type of a component in the FlowOrchestrator system.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ComponentType
    {
        /// <summary>
        /// Importer service component.
        /// </summary>
        IMPORTER_SERVICE,

        /// <summary>
        /// Processor service component.
        /// </summary>
        PROCESSOR_SERVICE,

        /// <summary>
        /// Exporter service component.
        /// </summary>
        EXPORTER_SERVICE,

        /// <summary>
        /// Source entity component.
        /// </summary>
        SOURCE_ENTITY,

        /// <summary>
        /// Destination entity component.
        /// </summary>
        DESTINATION_ENTITY,

        /// <summary>
        /// Source assignment entity component.
        /// </summary>
        SOURCE_ASSIGNMENT_ENTITY,

        /// <summary>
        /// Destination assignment entity component.
        /// </summary>
        DESTINATION_ASSIGNMENT_ENTITY,

        /// <summary>
        /// Flow entity component.
        /// </summary>
        FLOW_ENTITY,

        /// <summary>
        /// Processing chain entity component.
        /// </summary>
        PROCESSING_CHAIN_ENTITY,

        /// <summary>
        /// Task scheduler entity component.
        /// </summary>
        TASK_SCHEDULER_ENTITY,

        /// <summary>
        /// Scheduled flow entity component.
        /// </summary>
        SCHEDULED_FLOW_ENTITY,

        /// <summary>
        /// Protocol component.
        /// </summary>
        PROTOCOL,

        /// <summary>
        /// Protocol handler component.
        /// </summary>
        PROTOCOL_HANDLER,

        /// <summary>
        /// Statistics provider component.
        /// </summary>
        STATISTICS_PROVIDER,

        /// <summary>
        /// Other component type.
        /// </summary>
        OTHER
    }
}
