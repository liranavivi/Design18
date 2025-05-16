using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Defines the interface for flow entities in the FlowOrchestrator system.
    /// </summary>
    public interface IFlowEntity : IEntity, IService
    {
        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        string FlowId { get; set; }

        /// <summary>
        /// Gets or sets the name of the flow.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the flow.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the owner of the flow.
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// Gets or sets the tags for the flow.
        /// </summary>
        List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the importer service identifier.
        /// </summary>
        string ImporterServiceId { get; set; }

        /// <summary>
        /// Gets or sets the processor service identifiers.
        /// </summary>
        List<string> ProcessorServiceIds { get; set; }

        /// <summary>
        /// Gets or sets the exporter service identifiers.
        /// </summary>
        List<string> ExporterServiceIds { get; set; }

        /// <summary>
        /// Gets or sets the flow configuration.
        /// </summary>
        Dictionary<string, object> Configuration { get; set; }

        /// <summary>
        /// Gets or sets the flow metadata.
        /// </summary>
        Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the flow is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the flow structure.
        /// </summary>
        FlowStructure Structure { get; set; }
    }

    /// <summary>
    /// Represents the structure of a flow.
    /// </summary>
    public class FlowStructure
    {
        /// <summary>
        /// Gets or sets the nodes in the flow.
        /// </summary>
        public List<FlowNode> Nodes { get; set; } = new List<FlowNode>();

        /// <summary>
        /// Gets or sets the connections between nodes in the flow.
        /// </summary>
        public List<FlowConnection> Connections { get; set; } = new List<FlowConnection>();
    }

    /// <summary>
    /// Represents a node in a flow.
    /// </summary>
    public class FlowNode
    {
        /// <summary>
        /// Gets or sets the node identifier.
        /// </summary>
        public string NodeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the node type.
        /// </summary>
        public FlowNodeType NodeType { get; set; }

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the node configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the node metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a connection between nodes in a flow.
    /// </summary>
    public class FlowConnection
    {
        /// <summary>
        /// Gets or sets the connection identifier.
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source node identifier.
        /// </summary>
        public string SourceNodeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target node identifier.
        /// </summary>
        public string TargetNodeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch identifier.
        /// </summary>
        public string? BranchId { get; set; }

        /// <summary>
        /// Gets or sets the connection configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the connection metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents the type of a node in a flow.
    /// </summary>
    public enum FlowNodeType
    {
        /// <summary>
        /// Importer node.
        /// </summary>
        IMPORTER,

        /// <summary>
        /// Processor node.
        /// </summary>
        PROCESSOR,

        /// <summary>
        /// Exporter node.
        /// </summary>
        EXPORTER,

        /// <summary>
        /// Branch node.
        /// </summary>
        BRANCH,

        /// <summary>
        /// Merge node.
        /// </summary>
        MERGE
    }
}
