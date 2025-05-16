using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.MemoryManager.Interfaces;
using FlowOrchestrator.MemoryManager.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.MemoryManager.Services
{
    /// <summary>
    /// Implementation of the memory manager service.
    /// </summary>
    public class MemoryManagerService : IMemoryManager
    {
        private readonly IMemoryAllocationService _allocationService;
        private readonly IMemoryAccessControlService _accessControlService;
        private readonly ILogger<MemoryManagerService> _logger;
        private ServiceState _state = ServiceState.UNINITIALIZED;
        private ConfigurationParameters _configurationParameters = new ConfigurationParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryManagerService"/> class.
        /// </summary>
        /// <param name="allocationService">The memory allocation service.</param>
        /// <param name="accessControlService">The memory access control service.</param>
        /// <param name="logger">The logger.</param>
        public MemoryManagerService(
            IMemoryAllocationService allocationService,
            IMemoryAccessControlService accessControlService,
            ILogger<MemoryManagerService> logger)
        {
            _allocationService = allocationService;
            _accessControlService = accessControlService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public string ServiceId => _configurationParameters.GetParameterOrDefault<string>("ServiceId", "MEMORY-MANAGER-SERVICE");

        /// <inheritdoc/>
        public string Version => "1.0.0";

        /// <inheritdoc/>
        public string ServiceType => "MemoryManager";

        /// <inheritdoc/>
        public void Initialize(ConfigurationParameters parameters)
        {
            _logger.LogInformation("Initializing MemoryManagerService with parameters: {Parameters}", parameters);
            _state = ServiceState.INITIALIZING;

            try
            {
                _configurationParameters = parameters;
                // Perform any additional initialization here

                _state = ServiceState.READY;
                _logger.LogInformation("MemoryManagerService initialized successfully");
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                _logger.LogError(ex, "Error initializing MemoryManagerService");
                throw;
            }
        }

        /// <inheritdoc/>
        public void Terminate()
        {
            _logger.LogInformation("Terminating MemoryManagerService");
            _state = ServiceState.TERMINATING;

            try
            {
                // Perform any cleanup here

                _state = ServiceState.TERMINATED;
                _logger.LogInformation("MemoryManagerService terminated successfully");
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                _logger.LogError(ex, "Error terminating MemoryManagerService");
                throw;
            }
        }

        /// <inheritdoc/>
        public ServiceState GetState()
        {
            return _state;
        }

        /// <inheritdoc/>
        public async Task<FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult> AllocateMemoryAsync(MemoryAllocationRequest request)
        {
            _logger.LogDebug("Allocating memory for request: {Request}", request);

            if (_state != ServiceState.READY)
            {
                _logger.LogWarning("Cannot allocate memory: service is not ready. Current state: {State}", _state);
                return new FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult
                {
                    Success = false,
                    ErrorMessage = $"Service is not ready. Current state: {_state}"
                };
            }

            try
            {
                var result = await _allocationService.AllocateAsync(request);

                if (result.Success && request.Context != null)
                {
                    // Grant access to the execution context
                    await _accessControlService.GrantAccessAsync(result.MemoryAddress, request.Context);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating memory");
                return new FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeallocateMemoryAsync(string memoryAddress)
        {
            _logger.LogDebug("Deallocating memory for address: {Address}", memoryAddress);

            if (_state != ServiceState.READY)
            {
                _logger.LogWarning("Cannot deallocate memory: service is not ready. Current state: {State}", _state);
                return false;
            }

            try
            {
                return await _allocationService.DeallocateAsync(memoryAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deallocating memory");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string memoryAddress)
        {
            _logger.LogDebug("Checking if memory address exists: {Address}", memoryAddress);

            if (_state != ServiceState.READY)
            {
                _logger.LogWarning("Cannot check memory existence: service is not ready. Current state: {State}", _state);
                return false;
            }

            try
            {
                return await _allocationService.ExistsAsync(memoryAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking memory existence");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> GetMemoryStatisticsAsync()
        {
            _logger.LogDebug("Getting memory statistics");

            if (_state != ServiceState.READY)
            {
                _logger.LogWarning("Cannot get memory statistics: service is not ready. Current state: {State}", _state);
                return new Dictionary<string, object>
                {
                    { "Error", $"Service is not ready. Current state: {_state}" }
                };
            }

            try
            {
                return await _allocationService.GetMemoryStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memory statistics");
                return new Dictionary<string, object>
                {
                    { "Error", ex.Message }
                };
            }
        }

        /// <inheritdoc/>
        public async Task<int> CleanupExecutionMemoryAsync(string executionId)
        {
            _logger.LogInformation("Cleaning up memory for execution: {ExecutionId}", executionId);

            if (_state != ServiceState.READY)
            {
                _logger.LogWarning("Cannot cleanup execution memory: service is not ready. Current state: {State}", _state);
                return 0;
            }

            try
            {
                return await _allocationService.CleanupExecutionMemoryAsync(executionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up execution memory");
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<int> CleanupBranchMemoryAsync(string executionId, string branchId)
        {
            _logger.LogInformation("Cleaning up memory for branch: {BranchId} of execution: {ExecutionId}", branchId, executionId);

            if (_state != ServiceState.READY)
            {
                _logger.LogWarning("Cannot cleanup branch memory: service is not ready. Current state: {State}", _state);
                return 0;
            }

            try
            {
                return await _allocationService.CleanupBranchMemoryAsync(executionId, branchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up branch memory");
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateAccessAsync(string memoryAddress, FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            _logger.LogDebug("Validating access to memory address: {Address} for context: {Context}", memoryAddress, context);

            if (_state != ServiceState.READY)
            {
                _logger.LogWarning("Cannot validate memory access: service is not ready. Current state: {State}", _state);
                return false;
            }

            try
            {
                return await _accessControlService.ValidateAccessAsync(memoryAddress, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating memory access");
                return false;
            }
        }
    }
}
