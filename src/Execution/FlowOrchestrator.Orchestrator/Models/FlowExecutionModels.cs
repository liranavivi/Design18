using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using System.Collections.Concurrent;

namespace FlowOrchestrator.Orchestrator.Models
{
    /// <summary>
    /// Represents a request to start a flow execution.
    /// </summary>
    public class StartFlowRequest
    {
        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow version.
        /// </summary>
        public string? FlowVersion { get; set; }

        /// <summary>
        /// Gets or sets the importer service identifier.
        /// </summary>
        public string? ImporterServiceId { get; set; }

        /// <summary>
        /// Gets or sets the processor service identifiers.
        /// </summary>
        public List<string>? ProcessorServiceIds { get; set; }

        /// <summary>
        /// Gets or sets the exporter service identifiers.
        /// </summary>
        public List<string>? ExporterServiceIds { get; set; }

        /// <summary>
        /// Gets or sets the execution parameters.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }
    }

    /// <summary>
    /// Represents the response for an execution status request.
    /// </summary>
    public class ExecutionStatusResponse
    {
        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution status.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the execution started.
        /// </summary>
        public DateTime StartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the execution ended.
        /// </summary>
        public DateTime? EndTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the branch status information.
        /// </summary>
        public List<BranchStatusInfo> Branches { get; set; } = new List<BranchStatusInfo>();
    }

    /// <summary>
    /// Represents branch status information.
    /// </summary>
    public class BranchStatusInfo
    {
        /// <summary>
        /// Gets or sets the branch identifier.
        /// </summary>
        public string BranchId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of completed steps.
        /// </summary>
        public int CompletedSteps { get; set; }

        /// <summary>
        /// Gets or sets the number of pending steps.
        /// </summary>
        public int PendingSteps { get; set; }
    }

    /// <summary>
    /// Mock implementation of IFlowEntity for testing purposes.
    /// </summary>
    public class MockFlowEntity : IFlowEntity
    {
        /// <inheritdoc/>
        public string ServiceId { get; set; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public string ServiceType { get; set; } = "FLOW_ENTITY";

        /// <inheritdoc/>
        public string Version { get; set; } = "1.0.0";

        /// <inheritdoc/>
        public ServiceState State { get; set; } = ServiceState.READY;

        /// <inheritdoc/>
        public string FlowId { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string Name { get; set; } = "Mock Flow";

        /// <inheritdoc/>
        public string Description { get; set; } = "Mock flow entity for testing";

        /// <inheritdoc/>
        public string Owner { get; set; } = "System";

        /// <inheritdoc/>
        public List<string> Tags { get; set; } = new List<string>();

        /// <inheritdoc/>
        public string ImporterServiceId { get; set; } = string.Empty;

        /// <inheritdoc/>
        public List<string> ProcessorServiceIds { get; set; } = new List<string>();

        /// <inheritdoc/>
        public List<string> ExporterServiceIds { get; set; } = new List<string>();

        /// <inheritdoc/>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc/>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc/>
        public FlowStructure Structure { get; set; } = new FlowStructure();

        /// <inheritdoc/>
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public DateTime LastModifiedTimestamp { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public string VersionDescription { get; set; } = "Initial version";

        /// <inheritdoc/>
        public string PreviousVersionId { get; set; } = string.Empty;

        /// <inheritdoc/>
        public VersionStatus VersionStatus { get; set; } = VersionStatus.ACTIVE;

        /// <inheritdoc/>
        public void Initialize(ConfigurationParameters parameters)
        {
            // No-op for mock
        }

        /// <inheritdoc/>
        public void Terminate()
        {
            // No-op for mock
        }

        /// <inheritdoc/>
        public ValidationResult ValidateConfiguration(ConfigurationParameters parameters)
        {
            return new ValidationResult();
        }

        /// <inheritdoc/>
        public string GetEntityId()
        {
            return FlowId;
        }

        /// <inheritdoc/>
        public string GetEntityType()
        {
            return "FLOW";
        }

        /// <inheritdoc/>
        public ValidationResult Validate()
        {
            return new ValidationResult();
        }

        /// <inheritdoc/>
        public ServiceState GetState()
        {
            return State;
        }
    }
}
