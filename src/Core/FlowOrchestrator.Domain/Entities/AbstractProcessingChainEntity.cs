using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Base abstract implementation for processing chain entities.
    /// Represents a directed acyclic graph of processor services that defines data transformation logic.
    /// </summary>
    public abstract class AbstractProcessingChainEntity : AbstractEntity, IProcessingChainEntity
    {
        /// <summary>
        /// Gets or sets the processing chain identifier.
        /// </summary>
        public string ChainId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the processing chain.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the processing chain.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the owner of the processing chain.
        /// </summary>
        public string Owner { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tags for the processing chain.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the processor service identifiers.
        /// </summary>
        public List<string> ProcessorServiceIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the processing chain configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the processing chain metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a value indicating whether the processing chain is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the processing chain structure.
        /// </summary>
        public ProcessingChainStructure Structure { get; set; } = new ProcessingChainStructure();

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service type.
        /// </summary>
        public string ServiceType { get; set; } = "PROCESSING_CHAIN";

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        public new string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the version description.
        /// </summary>
        public new string VersionDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the previous version identifier.
        /// </summary>
        public new string PreviousVersionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version status.
        /// </summary>
        public new VersionStatus VersionStatus { get; set; } = VersionStatus.DRAFT;

        /// <summary>
        /// Initializes the service with the specified configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public void Initialize(ConfigurationParameters parameters)
        {
            // No initialization needed for this entity
        }

        /// <summary>
        /// Terminates the service.
        /// </summary>
        public void Terminate()
        {
            // No termination needed for this entity
        }

        /// <summary>
        /// Gets the service state.
        /// </summary>
        /// <returns>The service state.</returns>
        public ServiceState GetState()
        {
            return ServiceState.READY;
        }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public override string GetEntityId()
        {
            return ChainId;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public override string GetEntityType()
        {
            return "ProcessingChainEntity";
        }

        /// <summary>
        /// Validates the processing chain entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required fields
            if (string.IsNullOrWhiteSpace(ChainId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "CHAIN_ID_REQUIRED", Message = "Processing chain ID is required." });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "CHAIN_NAME_REQUIRED", Message = "Processing chain name is required." });
            }

            if (ProcessorServiceIds.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "PROCESSOR_SERVICE_IDS_REQUIRED", Message = "At least one processor service ID is required." });
            }

            // Validate processing chain structure
            var structureValidation = ValidateProcessingChainStructure();
            if (!structureValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(structureValidation.Errors);
            }

            return result;
        }

        /// <summary>
        /// Validates the processing chain structure.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected virtual ValidationResult ValidateProcessingChainStructure()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate that there is at least one processor node
            if (Structure.Nodes.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "CHAIN_STRUCTURE_EMPTY", Message = "Processing chain structure must contain at least one processor node." });
                return result;
            }

            // Validate that all nodes have a valid ID
            foreach (var node in Structure.Nodes)
            {
                if (string.IsNullOrWhiteSpace(node.NodeId))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INVALID_NODE_ID", Message = $"Processor node has an invalid ID." });
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

            // Validate that the processing chain is a directed acyclic graph (DAG)
            if (!IsDirectedAcyclicGraph())
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "CYCLIC_DEPENDENCY", Message = "Processing chain must be a directed acyclic graph (DAG). Cycles detected." });
            }

            return result;
        }

        /// <summary>
        /// Checks if the processing chain structure forms a directed acyclic graph (DAG).
        /// </summary>
        /// <returns>True if the structure is a DAG, otherwise false.</returns>
        protected bool IsDirectedAcyclicGraph()
        {
            // Build adjacency list
            var adjacencyList = new Dictionary<string, List<string>>();
            foreach (var node in Structure.Nodes)
            {
                adjacencyList[node.NodeId] = new List<string>();
            }

            foreach (var connection in Structure.Connections)
            {
                adjacencyList[connection.SourceNodeId].Add(connection.TargetNodeId);
            }

            // Check for cycles using DFS
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();

            foreach (var node in Structure.Nodes)
            {
                if (HasCycle(node.NodeId, adjacencyList, visited, recursionStack))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasCycle(string nodeId, Dictionary<string, List<string>> adjacencyList, HashSet<string> visited, HashSet<string> recursionStack)
        {
            // If node is not visited yet, mark it as visited and add to recursion stack
            if (!visited.Contains(nodeId))
            {
                visited.Add(nodeId);
                recursionStack.Add(nodeId);

                // Visit all adjacent nodes
                foreach (var adjacentNode in adjacencyList[nodeId])
                {
                    // If adjacent node is not visited, check for cycles recursively
                    if (!visited.Contains(adjacentNode) && HasCycle(adjacentNode, adjacencyList, visited, recursionStack))
                    {
                        return true;
                    }
                    // If adjacent node is in recursion stack, there is a cycle
                    else if (recursionStack.Contains(adjacentNode))
                    {
                        return true;
                    }
                }
            }

            // Remove node from recursion stack
            recursionStack.Remove(nodeId);
            return false;
        }

        /// <summary>
        /// Copies the properties of this entity to another entity.
        /// </summary>
        /// <param name="target">The target entity.</param>
        protected override void CopyPropertiesTo(AbstractEntity target)
        {
            base.CopyPropertiesTo(target);

            if (target is AbstractProcessingChainEntity chainEntity)
            {
                chainEntity.ChainId = ChainId;
                chainEntity.Name = Name;
                chainEntity.Description = Description;
                chainEntity.Owner = Owner;
                chainEntity.Tags = new List<string>(Tags);
                chainEntity.ProcessorServiceIds = new List<string>(ProcessorServiceIds);
                chainEntity.Configuration = new Dictionary<string, object>(Configuration);
                chainEntity.Metadata = new Dictionary<string, object>(Metadata);
                chainEntity.IsEnabled = IsEnabled;

                // Deep copy of the processing chain structure
                chainEntity.Structure = new ProcessingChainStructure
                {
                    Nodes = Structure.Nodes.Select(n => new ProcessorNode
                    {
                        NodeId = n.NodeId,
                        ProcessorId = n.ProcessorId,
                        Configuration = new Dictionary<string, object>(n.Configuration),
                        Metadata = new Dictionary<string, object>(n.Metadata)
                    }).ToList(),

                    Connections = Structure.Connections.Select(c => new ProcessorConnection
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

    /// <summary>
    /// Represents the structure of a processing chain.
    /// </summary>
    public class ProcessingChainStructure
    {
        /// <summary>
        /// Gets or sets the processor nodes in the processing chain.
        /// </summary>
        public List<ProcessorNode> Nodes { get; set; } = new List<ProcessorNode>();

        /// <summary>
        /// Gets or sets the connections between processor nodes in the processing chain.
        /// </summary>
        public List<ProcessorConnection> Connections { get; set; } = new List<ProcessorConnection>();
    }

    /// <summary>
    /// Represents a processor node in a processing chain.
    /// </summary>
    public class ProcessorNode
    {
        /// <summary>
        /// Gets or sets the node identifier.
        /// </summary>
        public string NodeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processor identifier.
        /// </summary>
        public string ProcessorId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processor node configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the processor node metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a connection between processor nodes in a processing chain.
    /// </summary>
    public class ProcessorConnection
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
}
