using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Base abstract implementation of the IFlowEntity interface.
    /// Provides common functionality for flow entities in the system.
    /// </summary>
    public abstract class AbstractFlowEntity : AbstractEntity, IFlowEntity
    {
        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the flow.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the flow.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the owner of the flow.
        /// </summary>
        public string Owner { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tags for the flow.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the importer service identifier.
        /// </summary>
        public string ImporterServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processor service identifiers.
        /// </summary>
        public List<string> ProcessorServiceIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the exporter service identifiers.
        /// </summary>
        public List<string> ExporterServiceIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the flow configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the flow metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a value indicating whether the flow is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the flow structure.
        /// </summary>
        public FlowStructure Structure { get; set; } = new FlowStructure();

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public override string GetEntityId()
        {
            return FlowId;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public override string GetEntityType()
        {
            return "FlowEntity";
        }

        /// <summary>
        /// Validates the flow entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required fields
            if (string.IsNullOrWhiteSpace(FlowId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "FLOW_ID_REQUIRED", Message = "Flow ID is required." });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "FLOW_NAME_REQUIRED", Message = "Flow name is required." });
            }

            if (string.IsNullOrWhiteSpace(ImporterServiceId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "IMPORTER_SERVICE_ID_REQUIRED", Message = "Importer service ID is required." });
            }

            if (ExporterServiceIds.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "EXPORTER_SERVICE_IDS_REQUIRED", Message = "At least one exporter service ID is required." });
            }

            // Validate flow structure
            var structureValidation = ValidateFlowStructure();
            if (!structureValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(structureValidation.Errors);
            }

            return result;
        }

        /// <summary>
        /// Validates the flow structure.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected virtual ValidationResult ValidateFlowStructure()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate that there is at least one node
            if (Structure.Nodes.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "FLOW_STRUCTURE_EMPTY", Message = "Flow structure must contain at least one node." });
                return result;
            }

            // Validate that there is exactly one importer node
            var importerNodes = Structure.Nodes.Where(n => n.NodeType == FlowNodeType.IMPORTER).ToList();
            if (importerNodes.Count != 1)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "INVALID_IMPORTER_COUNT", Message = $"Flow must have exactly one importer node, but found {importerNodes.Count}." });
            }

            // Validate that there is at least one exporter node
            var exporterNodes = Structure.Nodes.Where(n => n.NodeType == FlowNodeType.EXPORTER).ToList();
            if (exporterNodes.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "NO_EXPORTER_NODES", Message = "Flow must have at least one exporter node." });
            }

            // Validate that all nodes have a valid ID
            foreach (var node in Structure.Nodes)
            {
                if (string.IsNullOrWhiteSpace(node.NodeId))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INVALID_NODE_ID", Message = $"Node of type {node.NodeType} has an invalid ID." });
                }
            }

            // Validate that all connections have valid source and target nodes
            foreach (var connection in Structure.Connections)
            {
                if (string.IsNullOrWhiteSpace(connection.SourceNodeId))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INVALID_CONNECTION_SOURCE", Message = $"Connection {connection.ConnectionId} has an invalid source node ID." });
                }

                if (string.IsNullOrWhiteSpace(connection.TargetNodeId))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INVALID_CONNECTION_TARGET", Message = $"Connection {connection.ConnectionId} has an invalid target node ID." });
                }

                // Validate that source and target nodes exist
                var sourceNode = Structure.Nodes.FirstOrDefault(n => n.NodeId == connection.SourceNodeId);
                var targetNode = Structure.Nodes.FirstOrDefault(n => n.NodeId == connection.TargetNodeId);

                if (sourceNode == null)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "SOURCE_NODE_NOT_FOUND", Message = $"Source node {connection.SourceNodeId} for connection {connection.ConnectionId} not found." });
                }

                if (targetNode == null)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "TARGET_NODE_NOT_FOUND", Message = $"Target node {connection.TargetNodeId} for connection {connection.ConnectionId} not found." });
                }
            }

            return result;
        }

        /// <summary>
        /// Copies the properties of this entity to another entity.
        /// </summary>
        /// <param name="target">The target entity.</param>
        protected override void CopyPropertiesTo(AbstractEntity target)
        {
            base.CopyPropertiesTo(target);

            if (target is AbstractFlowEntity flowEntity)
            {
                flowEntity.FlowId = FlowId;
                flowEntity.Name = Name;
                flowEntity.Description = Description;
                flowEntity.Owner = Owner;
                flowEntity.Tags = new List<string>(Tags);
                flowEntity.ImporterServiceId = ImporterServiceId;
                flowEntity.ProcessorServiceIds = new List<string>(ProcessorServiceIds);
                flowEntity.ExporterServiceIds = new List<string>(ExporterServiceIds);
                flowEntity.Configuration = new Dictionary<string, object>(Configuration);
                flowEntity.Metadata = new Dictionary<string, object>(Metadata);
                flowEntity.IsEnabled = IsEnabled;
                
                // Deep copy of the flow structure
                flowEntity.Structure = new FlowStructure
                {
                    Nodes = Structure.Nodes.Select(n => new FlowNode
                    {
                        NodeId = n.NodeId,
                        NodeType = n.NodeType,
                        ServiceId = n.ServiceId,
                        Configuration = new Dictionary<string, object>(n.Configuration),
                        Metadata = new Dictionary<string, object>(n.Metadata)
                    }).ToList(),
                    
                    Connections = Structure.Connections.Select(c => new FlowConnection
                    {
                        ConnectionId = c.ConnectionId,
                        SourceNodeId = c.SourceNodeId,
                        TargetNodeId = c.TargetNodeId,
                        BranchId = c.BranchId,
                        Configuration = new Dictionary<string, object>(c.Configuration),
                        Metadata = new Dictionary<string, object>(c.Metadata)
                    }).ToList()
                };
            }
        }
    }
}
