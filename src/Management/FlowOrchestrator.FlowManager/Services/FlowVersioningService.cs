using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FlowOrchestrator.Management.Flows.Services
{
    /// <summary>
    /// Service for managing flow entity versions.
    /// </summary>
    public class FlowVersioningService
    {
        private readonly ILogger<FlowVersioningService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowVersioningService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public FlowVersioningService(ILogger<FlowVersioningService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates a new version of a flow entity.
        /// </summary>
        /// <param name="flow">The flow entity to version.</param>
        /// <param name="versionDescription">The version description.</param>
        /// <returns>The new flow entity version.</returns>
        public IFlowEntity CreateNewVersion(IFlowEntity flow, string versionDescription)
        {
            if (flow == null)
            {
                throw new ArgumentNullException(nameof(flow));
            }

            if (string.IsNullOrWhiteSpace(versionDescription))
            {
                throw new ArgumentException("Version description cannot be null or empty", nameof(versionDescription));
            }

            try
            {
                // Create a new flow entity with the same properties
                var newFlow = new FlowEntity
                {
                    FlowId = flow.FlowId,
                    Name = flow.Name,
                    Description = flow.Description,
                    Owner = flow.Owner,
                    Tags = new List<string>(flow.Tags),
                    ImporterServiceId = flow.ImporterServiceId,
                    ProcessorServiceIds = new List<string>(flow.ProcessorServiceIds),
                    ExporterServiceIds = new List<string>(flow.ExporterServiceIds),
                    IsEnabled = flow.IsEnabled,
                    ServiceId = flow.ServiceId,
                    ServiceType = flow.ServiceType,
                    Metadata = new Dictionary<string, object>(flow.Metadata)
                };

                // Copy configuration
                newFlow.Configuration = new Dictionary<string, object>(flow.Configuration);

                // Copy structure
                newFlow.Structure = CloneFlowStructure(flow.Structure) ?? new FlowStructure();

                // Update version-specific properties
                string currentVersion = ((IEntity)flow).Version;
                var parts = currentVersion.Split('.');

                if (parts.Length != 3 || !int.TryParse(parts[2], out int patch))
                {
                    throw new InvalidOperationException($"Invalid version format: {currentVersion}");
                }

                // Increment patch version
                patch++;
                var newVersion = $"{parts[0]}.{parts[1]}.{patch}";
                newFlow.Version = newVersion;
                newFlow.VersionDescription = versionDescription;
                newFlow.PreviousVersionId = flow.ServiceId;
                newFlow.VersionStatus = VersionStatus.DRAFT;

                _logger.LogInformation("Created new version {NewVersion} of flow {FlowId}", newVersion, flow.FlowId);

                return newFlow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version of flow {FlowId}", flow.FlowId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new version of a processing chain entity.
        /// </summary>
        /// <param name="chain">The processing chain entity to version.</param>
        /// <param name="versionDescription">The version description.</param>
        /// <returns>The new processing chain entity version.</returns>
        public IProcessingChainEntity CreateNewVersion(IProcessingChainEntity chain, string versionDescription)
        {
            if (chain == null)
            {
                throw new ArgumentNullException(nameof(chain));
            }

            if (string.IsNullOrWhiteSpace(versionDescription))
            {
                throw new ArgumentException("Version description cannot be null or empty", nameof(versionDescription));
            }

            try
            {
                // Create a new processing chain entity with the same properties
                var newChain = new ProcessingChainEntity
                {
                    ChainId = chain.ChainId,
                    Name = chain.Name,
                    Description = chain.Description,
                    ProcessorServiceIds = new List<string>(chain.ProcessorServiceIds),
                    ServiceId = chain.ServiceId,
                    ServiceType = chain.ServiceType,
                    Metadata = new Dictionary<string, object>(chain.Metadata)
                };

                // Copy configuration
                newChain.Configuration = new Dictionary<string, object>(chain.Configuration);

                // Update version-specific properties
                string currentVersion = ((IEntity)chain).Version;
                var parts = currentVersion.Split('.');

                if (parts.Length != 3 || !int.TryParse(parts[2], out int patch))
                {
                    throw new InvalidOperationException($"Invalid version format: {currentVersion}");
                }

                // Increment patch version
                patch++;
                var newVersion = $"{parts[0]}.{parts[1]}.{patch}";
                newChain.Version = newVersion;
                newChain.VersionDescription = versionDescription;
                newChain.PreviousVersionId = chain.ServiceId;
                newChain.VersionStatus = VersionStatus.DRAFT;

                _logger.LogInformation("Created new version {NewVersion} of processing chain {ChainId}", newVersion, chain.ChainId);

                return newChain;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version of processing chain {ChainId}", chain.ChainId);
                throw;
            }
        }

        /// <summary>
        /// Clones a flow structure.
        /// </summary>
        /// <param name="structure">The flow structure to clone.</param>
        /// <returns>The cloned flow structure.</returns>
        private FlowStructure? CloneFlowStructure(FlowStructure? structure)
        {
            if (structure == null)
            {
                return null;
            }

            var newStructure = new FlowStructure
            {
                Nodes = new List<FlowNode>(),
                Connections = new List<FlowConnection>()
            };

            // Clone nodes
            foreach (var node in structure.Nodes)
            {
                var newNode = new FlowNode
                {
                    NodeId = node.NodeId,
                    NodeType = node.NodeType,
                    ServiceId = node.ServiceId,
                    Configuration = new Dictionary<string, object>(node.Configuration),
                    Metadata = new Dictionary<string, object>(node.Metadata)
                };

                newStructure.Nodes.Add(newNode);
            }

            // Clone connections
            foreach (var connection in structure.Connections)
            {
                var newConnection = new FlowConnection
                {
                    ConnectionId = connection.ConnectionId,
                    SourceNodeId = connection.SourceNodeId,
                    TargetNodeId = connection.TargetNodeId,
                    Configuration = new Dictionary<string, object>(connection.Configuration),
                    Metadata = new Dictionary<string, object>(connection.Metadata)
                };

                newStructure.Connections.Add(newConnection);
            }

            return newStructure;
        }
    }
}
