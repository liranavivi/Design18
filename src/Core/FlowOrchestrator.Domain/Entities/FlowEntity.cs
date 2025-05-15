using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Concrete implementation of the AbstractFlowEntity class.
    /// Represents a complete end-to-end data pipeline connecting sources to destinations.
    /// </summary>
    public class FlowEntity : AbstractFlowEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowEntity"/> class.
        /// </summary>
        public FlowEntity()
        {
            // Default constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowEntity"/> class with the specified flow ID.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        public FlowEntity(string flowId)
        {
            FlowId = flowId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowEntity"/> class with the specified flow ID and name.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="name">The name of the flow.</param>
        public FlowEntity(string flowId, string name)
        {
            FlowId = flowId;
            Name = name;
        }

        /// <summary>
        /// Validates the flow entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            // Call the base validation
            var result = base.Validate();

            // Add any additional validation specific to this concrete implementation
            if (result.IsValid)
            {
                // Validate that the flow has at least one processor
                if (ProcessorServiceIds.Count == 0)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "NO_PROCESSORS", Message = "Flow must have at least one processor." });
                }

                // Validate that the flow has a valid structure
                var structureValidation = ValidateFlowStructureConnectivity();
                if (!structureValidation.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(structureValidation.Errors);
                }
            }

            return result;
        }

        /// <summary>
        /// Validates the connectivity of the flow structure.
        /// </summary>
        /// <returns>The validation result.</returns>
        private ValidationResult ValidateFlowStructureConnectivity()
        {
            var result = new ValidationResult { IsValid = true };

            // Get the importer node
            var importerNode = Structure.Nodes.FirstOrDefault(n => n.NodeType == Abstractions.Entities.FlowNodeType.IMPORTER);
            if (importerNode == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "NO_IMPORTER_NODE", Message = "Flow must have an importer node." });
                return result;
            }

            // Get the exporter nodes
            var exporterNodes = Structure.Nodes.Where(n => n.NodeType == Abstractions.Entities.FlowNodeType.EXPORTER).ToList();
            if (exporterNodes.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "NO_EXPORTER_NODES", Message = "Flow must have at least one exporter node." });
                return result;
            }

            // Build a graph representation of the flow
            var graph = new Dictionary<string, List<string>>();
            foreach (var node in Structure.Nodes)
            {
                graph[node.NodeId] = new List<string>();
            }

            foreach (var connection in Structure.Connections)
            {
                if (graph.ContainsKey(connection.SourceNodeId))
                {
                    graph[connection.SourceNodeId].Add(connection.TargetNodeId);
                }
            }

            // Check that all nodes are reachable from the importer
            var visited = new HashSet<string>();
            DepthFirstSearch(importerNode.NodeId, graph, visited);

            // Check if all nodes are reachable
            var unreachableNodes = Structure.Nodes
                .Where(n => !visited.Contains(n.NodeId))
                .ToList();

            if (unreachableNodes.Any())
            {
                result.IsValid = false;
                foreach (var node in unreachableNodes)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "UNREACHABLE_NODE",
                        Message = $"Node {node.NodeId} is not reachable from the importer."
                    });
                }
            }

            // Check that all exporter nodes are reachable
            var unreachableExporters = exporterNodes
                .Where(n => !visited.Contains(n.NodeId))
                .ToList();

            if (unreachableExporters.Any())
            {
                result.IsValid = false;
                foreach (var node in unreachableExporters)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "UNREACHABLE_EXPORTER",
                        Message = $"Exporter node {node.NodeId} is not reachable from the importer."
                    });
                }
            }

            // Check for nodes with no outgoing connections (except exporters)
            var deadEndNodes = Structure.Nodes
                .Where(n => n.NodeType != Abstractions.Entities.FlowNodeType.EXPORTER)
                .Where(n => !graph[n.NodeId].Any())
                .ToList();

            if (deadEndNodes.Any())
            {
                result.IsValid = false;
                foreach (var node in deadEndNodes)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "DEAD_END_NODE",
                        Message = $"Node {node.NodeId} has no outgoing connections."
                    });
                }
            }

            return result;
        }

        private void DepthFirstSearch(string nodeId, Dictionary<string, List<string>> graph, HashSet<string> visited)
        {
            visited.Add(nodeId);
            foreach (var adjacentNode in graph[nodeId])
            {
                if (!visited.Contains(adjacentNode))
                {
                    DepthFirstSearch(adjacentNode, graph, visited);
                }
            }
        }
    }
}
