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
    /// Manager service for processing chain entities.
    /// </summary>
    public class ProcessingChainManager : BaseFlowManagerService<IProcessingChainEntity>
    {
        private readonly FlowValidationService _flowValidationService;
        private readonly FlowVersioningService _flowVersioningService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingChainManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        /// <param name="flowValidationService">The flow validation service.</param>
        /// <param name="flowVersioningService">The flow versioning service.</param>
        public ProcessingChainManager(
            ILogger<ProcessingChainManager> logger,
            IPublishEndpoint publishEndpoint,
            FlowValidationService flowValidationService,
            FlowVersioningService flowVersioningService)
            : base(logger, publishEndpoint)
        {
            _flowValidationService = flowValidationService;
            _flowVersioningService = flowVersioningService;
        }

        /// <summary>
        /// Validates processing chain-specific aspects.
        /// </summary>
        /// <param name="service">The processing chain entity to validate.</param>
        /// <returns>A validation result indicating whether the processing chain entity is valid.</returns>
        protected override ValidationResult ValidateServiceSpecifics(IProcessingChainEntity service)
        {
            if (service == null)
            {
                return ValidationResult.Error("Processing chain entity cannot be null");
            }

            var errors = new List<string>();

            // Validate processing chain ID
            if (string.IsNullOrWhiteSpace(service.ChainId))
            {
                errors.Add("Processing chain ID cannot be null or empty");
            }

            // Validate processor service IDs
            if (service.ProcessorServiceIds == null || service.ProcessorServiceIds.Count == 0)
            {
                errors.Add("Processing chain must have at least one processor service ID");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Processing chain validation failed", errors.ToArray())
                : ValidationResult.Success("Processing chain validation successful");
        }

        /// <summary>
        /// Gets the expected service type.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "PROCESSING_CHAIN";
        }

        /// <summary>
        /// Determines whether the specified service type is valid.
        /// </summary>
        /// <param name="serviceType">The service type to check.</param>
        /// <returns>True if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType == "PROCESSING_CHAIN";
        }

        /// <summary>
        /// Creates a new version of a processing chain entity.
        /// </summary>
        /// <param name="chainId">The chain identifier.</param>
        /// <param name="versionDescription">The version description.</param>
        /// <returns>The new processing chain entity version.</returns>
        public IProcessingChainEntity CreateNewVersion(string chainId, string versionDescription)
        {
            var chain = GetService(chainId);
            if (chain == null)
            {
                throw new ArgumentException($"Processing chain with ID '{chainId}' not found", nameof(chainId));
            }

            return _flowVersioningService.CreateNewVersion(chain, versionDescription);
        }

        /// <summary>
        /// Gets all processing chain entities.
        /// </summary>
        /// <returns>The collection of processing chain entities.</returns>
        public IEnumerable<IProcessingChainEntity> GetAllProcessingChains()
        {
            return GetAllServices();
        }

        /// <summary>
        /// Gets a processing chain entity by its identifier.
        /// </summary>
        /// <param name="chainId">The chain identifier.</param>
        /// <returns>The processing chain entity if found; otherwise, null.</returns>
        public IProcessingChainEntity? GetProcessingChain(string chainId)
        {
            return GetService(chainId);
        }

        /// <summary>
        /// Gets all versions of a processing chain entity.
        /// </summary>
        /// <param name="chainId">The chain identifier.</param>
        /// <returns>The collection of processing chain entity versions.</returns>
        public IEnumerable<IProcessingChainEntity> GetProcessingChainVersions(string chainId)
        {
            return GetAllServiceVersions(chainId);
        }
    }
}
