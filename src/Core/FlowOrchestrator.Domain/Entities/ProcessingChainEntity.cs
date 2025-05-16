using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Concrete implementation of the AbstractProcessingChainEntity class.
    /// Represents a directed acyclic graph of processor services that defines data transformation logic.
    /// </summary>
    public class ProcessingChainEntity : AbstractProcessingChainEntity, IService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingChainEntity"/> class.
        /// </summary>
        public ProcessingChainEntity()
        {
            // Default constructor
            ServiceId = string.Empty;
            ServiceType = "PROCESSING_CHAIN";
            Version = "1.0.0";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingChainEntity"/> class with the specified chain ID.
        /// </summary>
        /// <param name="chainId">The processing chain identifier.</param>
        public ProcessingChainEntity(string chainId)
        {
            ChainId = chainId;
            ServiceId = chainId;
            ServiceType = "PROCESSING_CHAIN";
            Version = "1.0.0";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingChainEntity"/> class with the specified chain ID and name.
        /// </summary>
        /// <param name="chainId">The processing chain identifier.</param>
        /// <param name="name">The name of the processing chain.</param>
        public ProcessingChainEntity(string chainId, string name)
        {
            ChainId = chainId;
            Name = name;
            ServiceId = chainId;
            ServiceType = "PROCESSING_CHAIN";
            Version = "1.0.0";
        }

        /// <summary>
        /// Validates the processing chain entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            // Call the base validation
            var result = base.Validate();

            // Add any additional validation specific to this concrete implementation
            if (result.IsValid)
            {
                // Validate that the processing chain has at least one processor
                if (ProcessorServiceIds.Count == 0)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "NO_PROCESSORS", Message = "Processing chain must have at least one processor." });
                }

                // Validate that the processing chain has a valid structure
                var structureValidation = ValidateProcessingChainConnectivity();
                if (!structureValidation.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(structureValidation.Errors);
                }
            }

            return result;
        }

        /// <summary>
        /// Validates the connectivity of the processing chain structure.
        /// </summary>
        /// <returns>The validation result.</returns>
        private ValidationResult ValidateProcessingChainConnectivity()
        {
            var result = new ValidationResult { IsValid = true };

            // Build a graph representation of the processing chain
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

            // Identify entry points (nodes with no incoming connections)
            var incomingConnections = new Dictionary<string, int>();
            foreach (var node in Structure.Nodes)
            {
                incomingConnections[node.NodeId] = 0;
            }

            foreach (var connection in Structure.Connections)
            {
                if (incomingConnections.ContainsKey(connection.TargetNodeId))
                {
                    incomingConnections[connection.TargetNodeId]++;
                }
            }

            var entryPoints = Structure.Nodes
                .Where(n => incomingConnections[n.NodeId] == 0)
                .ToList();

            // Validate that there is at least one entry point
            if (entryPoints.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "NO_ENTRY_POINTS", Message = "Processing chain must have at least one entry point." });
                return result;
            }

            // Check that all nodes are reachable from at least one entry point
            var visited = new HashSet<string>();
            foreach (var entryPoint in entryPoints)
            {
                DepthFirstSearch(entryPoint.NodeId, graph, visited);
            }

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
                        Message = $"Node {node.NodeId} is not reachable from any entry point."
                    });
                }
            }

            // Identify exit points (nodes with no outgoing connections)
            var exitPoints = Structure.Nodes
                .Where(n => !graph[n.NodeId].Any())
                .ToList();

            // Validate that there is at least one exit point
            if (exitPoints.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "NO_EXIT_POINTS", Message = "Processing chain must have at least one exit point." });
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
