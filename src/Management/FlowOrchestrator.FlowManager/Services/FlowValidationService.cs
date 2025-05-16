using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Common.Validation;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Management.Flows.Services
{
    /// <summary>
    /// Service for validating flow entities.
    /// </summary>
    public class FlowValidationService
    {
        private readonly ILogger<FlowValidationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowValidationService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public FlowValidationService(ILogger<FlowValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates a flow entity.
        /// </summary>
        /// <param name="flow">The flow entity to validate.</param>
        /// <returns>A validation result indicating whether the flow entity is valid.</returns>
        public ValidationResult ValidateFlow(IFlowEntity flow)
        {
            if (flow == null)
            {
                return ValidationResult.Error("Flow entity cannot be null");
            }

            var errors = new List<string>();

            // Validate flow ID
            if (string.IsNullOrWhiteSpace(flow.FlowId))
            {
                errors.Add("Flow ID cannot be null or empty");
            }

            // Validate flow name
            if (string.IsNullOrWhiteSpace(flow.Name))
            {
                errors.Add("Flow name cannot be null or empty");
            }

            // Validate importer service ID
            if (string.IsNullOrWhiteSpace(flow.ImporterServiceId))
            {
                errors.Add("Importer service ID cannot be null or empty");
            }

            // Validate exporter service IDs
            if (flow.ExporterServiceIds == null || flow.ExporterServiceIds.Count == 0)
            {
                errors.Add("Flow must have at least one exporter service ID");
            }

            // Validate flow structure
            var structureValidation = ValidateFlowStructure(flow);
            if (!structureValidation.IsValid)
            {
                errors.AddRange(structureValidation.Errors);
            }

            return errors.Count > 0
                ? ValidationResult.Error("Flow validation failed", errors.ToArray())
                : ValidationResult.Success("Flow validation successful");
        }

        /// <summary>
        /// Validates the structure of a flow entity.
        /// </summary>
        /// <param name="flow">The flow entity to validate.</param>
        /// <returns>A validation result indicating whether the flow structure is valid.</returns>
        private ValidationResult ValidateFlowStructure(IFlowEntity flow)
        {
            if (flow.Structure == null)
            {
                return ValidationResult.Error("Flow structure cannot be null");
            }

            var errors = new List<string>();

            // Validate that there is exactly one importer node
            var importerNodes = flow.Structure.Nodes.Where(n => n.NodeType == FlowNodeType.IMPORTER).ToList();
            if (importerNodes.Count != 1)
            {
                errors.Add($"Flow must have exactly one importer node, but found {importerNodes.Count}");
            }

            // Validate that there is at least one exporter node
            var exporterNodes = flow.Structure.Nodes.Where(n => n.NodeType == FlowNodeType.EXPORTER).ToList();
            if (exporterNodes.Count == 0)
            {
                errors.Add("Flow must have at least one exporter node");
            }

            // Validate that all nodes are connected
            var connectedNodeIds = new HashSet<string>();
            foreach (var connection in flow.Structure.Connections)
            {
                connectedNodeIds.Add(connection.SourceNodeId);
                connectedNodeIds.Add(connection.TargetNodeId);
            }

            var disconnectedNodes = flow.Structure.Nodes
                .Where(n => !connectedNodeIds.Contains(n.NodeId))
                .ToList();

            if (disconnectedNodes.Count > 0)
            {
                errors.Add($"Flow has {disconnectedNodes.Count} disconnected nodes");
                foreach (var node in disconnectedNodes)
                {
                    errors.Add($"Node '{node.NodeId}' of type '{node.NodeType}' is disconnected");
                }
            }

            // Validate that there are no cycles in the flow graph
            if (HasCycles(flow.Structure))
            {
                errors.Add("Flow structure contains cycles, which are not allowed");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Flow structure validation failed", errors.ToArray())
                : ValidationResult.Success("Flow structure validation successful");
        }

        /// <summary>
        /// Determines whether the flow structure contains cycles.
        /// </summary>
        /// <param name="structure">The flow structure to check.</param>
        /// <returns>True if the flow structure contains cycles; otherwise, false.</returns>
        private bool HasCycles(FlowStructure structure)
        {
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();

            // Start DFS from each node to detect cycles
            foreach (var node in structure.Nodes)
            {
                if (IsCyclicUtil(structure, node.NodeId, visited, recursionStack))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Utility method for cycle detection using DFS.
        /// </summary>
        /// <param name="structure">The flow structure to check.</param>
        /// <param name="nodeId">The current node ID.</param>
        /// <param name="visited">The set of visited nodes.</param>
        /// <param name="recursionStack">The recursion stack.</param>
        /// <returns>True if a cycle is detected; otherwise, false.</returns>
        private bool IsCyclicUtil(FlowStructure structure, string nodeId, HashSet<string> visited, HashSet<string> recursionStack)
        {
            // Mark the current node as visited and add to recursion stack
            if (!visited.Contains(nodeId))
            {
                visited.Add(nodeId);
                recursionStack.Add(nodeId);

                // Find all adjacent nodes
                var adjacentNodes = structure.Connections
                    .Where(c => c.SourceNodeId == nodeId)
                    .Select(c => c.TargetNodeId);

                foreach (var adjacentNode in adjacentNodes)
                {
                    // If the adjacent node is not visited, recursively check it
                    if (!visited.Contains(adjacentNode) && IsCyclicUtil(structure, adjacentNode, visited, recursionStack))
                    {
                        return true;
                    }
                    // If the adjacent node is in the recursion stack, there is a cycle
                    else if (recursionStack.Contains(adjacentNode))
                    {
                        return true;
                    }
                }
            }

            // Remove the node from the recursion stack
            recursionStack.Remove(nodeId);
            return false;
        }
    }
}
