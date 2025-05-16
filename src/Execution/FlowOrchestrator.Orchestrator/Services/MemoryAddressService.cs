using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Orchestrator.Services
{
    /// <summary>
    /// Service for generating and managing memory addresses.
    /// </summary>
    public class MemoryAddressService
    {
        private readonly ILogger<MemoryAddressService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAddressService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MemoryAddressService(ILogger<MemoryAddressService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a memory address for a step in a flow execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="stepType">The step type.</param>
        /// <param name="branchPath">The branch path.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <param name="dataType">The data type.</param>
        /// <param name="additionalInfo">Additional information to include in the address.</param>
        /// <returns>The memory address.</returns>
        public string GenerateMemoryAddress(
            string executionId,
            string flowId,
            string stepType,
            string branchPath,
            string stepId,
            string dataType,
            string? additionalInfo = null)
        {
            // Format: {executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}:{additionalInfo}
            var address = $"{executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}";

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                address += $":{additionalInfo}";
            }

            _logger.LogDebug("Generated memory address: {Address}", address);
            return address;
        }

        /// <summary>
        /// Generates an input memory address for a processor.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="processorId">The processor identifier.</param>
        /// <returns>The memory address.</returns>
        public string GenerateProcessorInputAddress(FlowOrchestrator.Abstractions.Common.ExecutionContext executionContext, string processorId)
        {
            return GenerateMemoryAddress(
                executionContext.ExecutionId,
                executionContext.FlowId,
                "PROCESS",
                executionContext.BranchId ?? "main",
                processorId,
                "Input");
        }

        /// <summary>
        /// Generates an output memory address for a processor.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="processorId">The processor identifier.</param>
        /// <returns>The memory address.</returns>
        public string GenerateProcessorOutputAddress(FlowOrchestrator.Abstractions.Common.ExecutionContext executionContext, string processorId)
        {
            return GenerateMemoryAddress(
                executionContext.ExecutionId,
                executionContext.FlowId,
                "PROCESS",
                executionContext.BranchId ?? "main",
                processorId,
                "Output");
        }

        /// <summary>
        /// Generates an input memory address for an exporter.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="exporterId">The exporter identifier.</param>
        /// <returns>The memory address.</returns>
        public string GenerateExporterInputAddress(FlowOrchestrator.Abstractions.Common.ExecutionContext executionContext, string exporterId)
        {
            return GenerateMemoryAddress(
                executionContext.ExecutionId,
                executionContext.FlowId,
                "EXPORT",
                executionContext.BranchId ?? "main",
                exporterId,
                "Input");
        }

        /// <summary>
        /// Generates an output memory address for an importer.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="importerId">The importer identifier.</param>
        /// <returns>The memory address.</returns>
        public string GenerateImporterOutputAddress(FlowOrchestrator.Abstractions.Common.ExecutionContext executionContext, string importerId)
        {
            return GenerateMemoryAddress(
                executionContext.ExecutionId,
                executionContext.FlowId,
                "IMPORT",
                executionContext.BranchId ?? "main",
                importerId,
                "Output");
        }

        /// <summary>
        /// Parses a memory address into its components.
        /// </summary>
        /// <param name="address">The memory address.</param>
        /// <returns>A dictionary containing the address components.</returns>
        public Dictionary<string, string> ParseMemoryAddress(string address)
        {
            var components = address.Split(':');
            var result = new Dictionary<string, string>();

            if (components.Length >= 6)
            {
                result["executionId"] = components[0];
                result["flowId"] = components[1];
                result["stepType"] = components[2];
                result["branchPath"] = components[3];
                result["stepId"] = components[4];
                result["dataType"] = components[5];

                if (components.Length > 6)
                {
                    result["additionalInfo"] = components[6];
                }
            }
            else
            {
                _logger.LogWarning("Invalid memory address format: {Address}", address);
            }

            return result;
        }
    }
}
