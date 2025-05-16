using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Common.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;

// Use ValidationResult from FlowOrchestrator.Common.Validation to avoid ambiguity
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Management.Flows.Services
{
    /// <summary>
    /// Manager service for flow entities.
    /// </summary>
    public class FlowEntityManager : BaseFlowManagerService<IFlowEntity>
    {
        private readonly FlowValidationService _flowValidationService;
        private readonly FlowVersioningService _flowVersioningService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowEntityManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        /// <param name="flowValidationService">The flow validation service.</param>
        /// <param name="flowVersioningService">The flow versioning service.</param>
        public FlowEntityManager(
            ILogger<FlowEntityManager> logger,
            IPublishEndpoint publishEndpoint,
            FlowValidationService flowValidationService,
            FlowVersioningService flowVersioningService)
            : base(logger, publishEndpoint)
        {
            _flowValidationService = flowValidationService;
            _flowVersioningService = flowVersioningService;
        }

        /// <summary>
        /// Validates flow-specific aspects.
        /// </summary>
        /// <param name="service">The flow entity to validate.</param>
        /// <returns>A validation result indicating whether the flow entity is valid.</returns>
        protected override ValidationResult ValidateServiceSpecifics(IFlowEntity service)
        {
            return _flowValidationService.ValidateFlow(service);
        }

        /// <summary>
        /// Gets the expected service type.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "FLOW";
        }

        /// <summary>
        /// Determines whether the specified service type is valid.
        /// </summary>
        /// <param name="serviceType">The service type to check.</param>
        /// <returns>True if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType == "FLOW";
        }

        /// <summary>
        /// Creates a new version of a flow entity.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="versionDescription">The version description.</param>
        /// <returns>The new flow entity version.</returns>
        public IFlowEntity CreateNewVersion(string flowId, string versionDescription)
        {
            var flow = GetService(flowId);
            if (flow == null)
            {
                throw new ArgumentException($"Flow with ID '{flowId}' not found", nameof(flowId));
            }

            return _flowVersioningService.CreateNewVersion(flow, versionDescription);
        }

        /// <summary>
        /// Validates a flow entity.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <returns>A validation result indicating whether the flow entity is valid.</returns>
        public ValidationResult ValidateFlow(string flowId)
        {
            var flow = GetService(flowId);
            if (flow == null)
            {
                return ValidationResult.Error($"Flow with ID '{flowId}' not found");
            }

            return _flowValidationService.ValidateFlow(flow);
        }

        /// <summary>
        /// Gets all flow entities.
        /// </summary>
        /// <returns>The collection of flow entities.</returns>
        public IEnumerable<IFlowEntity> GetAllFlows()
        {
            return GetAllServices();
        }

        /// <summary>
        /// Gets a flow entity by its identifier.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <returns>The flow entity if found; otherwise, null.</returns>
        public IFlowEntity? GetFlow(string flowId)
        {
            return GetService(flowId);
        }

        /// <summary>
        /// Gets all versions of a flow entity.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <returns>The collection of flow entity versions.</returns>
        public IEnumerable<IFlowEntity> GetFlowVersions(string flowId)
        {
            return GetAllServiceVersions(flowId);
        }
    }
}
