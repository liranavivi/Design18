using FlowOrchestrator.Abstractions.Messaging.Messages;
using FlowOrchestrator.Infrastructure.Data.Hazelcast.Cache;
using FlowOrchestrator.MemoryManager.Interfaces;
using FlowOrchestrator.MemoryManager.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.MemoryManager.Services
{
    /// <summary>
    /// Implementation of the memory allocation service.
    /// </summary>
    public class MemoryAllocationService : IMemoryAllocationService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<MemoryAllocationService> _logger;
        private const string MEMORY_METADATA_PREFIX = "memory:metadata:";
        private const string MEMORY_INDEX_PREFIX = "memory:index:";

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAllocationService"/> class.
        /// </summary>
        /// <param name="cache">The distributed cache.</param>
        /// <param name="logger">The logger.</param>
        public MemoryAllocationService(
            IDistributedCache cache,
            ILogger<MemoryAllocationService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult> AllocateAsync(MemoryAllocationRequest request)
        {
            _logger.LogDebug("Allocating memory for request: {Request}", request);

            try
            {
                // Generate memory address
                var memoryAddress = GenerateMemoryAddress(request);

                // Check if the address already exists
                if (await ExistsAsync(memoryAddress))
                {
                    _logger.LogWarning("Memory address already exists: {Address}", memoryAddress);
                    return new FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult
                    {
                        Success = false,
                        MemoryAddress = memoryAddress,
                        ErrorMessage = "Memory address already exists"
                    };
                }

                // Create allocation result
                var result = new FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult
                {
                    Success = true,
                    MemoryAddress = memoryAddress,
                    ExecutionId = request.ExecutionId,
                    FlowId = request.FlowId,
                    StepId = request.StepId
                };

                // Note: TimeToLiveSeconds and EstimatedSizeBytes are stored in the cache metadata
                // but not in the MemoryAllocationResult class

                // Create metadata for cache
                var metadata = new MemoryMetadata
                {
                    Success = true,
                    MemoryAddress = memoryAddress,
                    AllocationTimestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "ExecutionId", request.ExecutionId },
                        { "FlowId", request.FlowId },
                        { "StepType", request.StepType },
                        { "BranchPath", request.BranchPath },
                        { "StepId", request.StepId },
                        { "DataType", request.DataType },
                        { "AllocationTimestamp", DateTime.UtcNow }
                    }
                };

                // Set expiration if specified
                if (request.TimeToLiveSeconds.HasValue)
                {
                    metadata.ExpirationTimestamp = DateTime.UtcNow.AddSeconds(request.TimeToLiveSeconds.Value);
                    metadata.Metadata["ExpirationTimestamp"] = metadata.ExpirationTimestamp;
                    metadata.Metadata["TimeToLiveSeconds"] = request.TimeToLiveSeconds.Value;
                }

                // Add estimated size if specified
                if (request.EstimatedSizeBytes.HasValue)
                {
                    metadata.Metadata["EstimatedSizeBytes"] = request.EstimatedSizeBytes.Value;
                }

                // Store metadata in cache
                await _cache.SetAsync(
                    $"{MEMORY_METADATA_PREFIX}{memoryAddress}",
                    metadata,
                    request.TimeToLiveSeconds);

                // Add to execution index
                await AddToExecutionIndexAsync(request.ExecutionId, memoryAddress);

                // Add to branch index if branch path is specified
                if (!string.IsNullOrEmpty(request.BranchPath) && request.BranchPath != "main")
                {
                    await AddToBranchIndexAsync(request.ExecutionId, request.BranchPath, memoryAddress);
                }

                _logger.LogInformation("Memory allocated successfully: {Address}", memoryAddress);
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
        public async Task<bool> DeallocateAsync(string memoryAddress)
        {
            _logger.LogDebug("Deallocating memory for address: {Address}", memoryAddress);

            try
            {
                // Get metadata to find execution and branch
                var metadata = await _cache.GetAsync<MemoryMetadata>($"{MEMORY_METADATA_PREFIX}{memoryAddress}");

                if (metadata == null)
                {
                    _logger.LogWarning("Memory address not found: {Address}", memoryAddress);
                    return false;
                }

                // Remove from cache
                var removed = await _cache.RemoveAsync($"{MEMORY_METADATA_PREFIX}{memoryAddress}");

                if (!removed)
                {
                    _logger.LogWarning("Failed to remove memory metadata: {Address}", memoryAddress);
                    return false;
                }

                // Remove from execution index
                if (metadata.Metadata.TryGetValue("ExecutionId", out var executionId) && executionId != null)
                {
                    string? executionIdStr = executionId.ToString();
                    if (!string.IsNullOrEmpty(executionIdStr))
                    {
                        await RemoveFromExecutionIndexAsync(executionIdStr, memoryAddress);
                    }
                }

                // Remove from branch index
                if (metadata.Metadata.TryGetValue("BranchPath", out var branchPath) && branchPath != null &&
                    metadata.Metadata.TryGetValue("ExecutionId", out var execId) && execId != null)
                {
                    string? branchPathStr = branchPath.ToString();
                    string? execIdStr = execId.ToString();

                    if (!string.IsNullOrEmpty(execIdStr) && !string.IsNullOrEmpty(branchPathStr) && branchPathStr != "main")
                    {
                        await RemoveFromBranchIndexAsync(execIdStr, branchPathStr, memoryAddress);
                    }
                }

                _logger.LogInformation("Memory deallocated successfully: {Address}", memoryAddress);
                return true;
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
            return await _cache.ExistsAsync($"{MEMORY_METADATA_PREFIX}{memoryAddress}");
        }

        /// <inheritdoc/>
        public string GenerateMemoryAddress(MemoryAllocationRequest request)
        {
            // Format: {executionId}:{flowId}:{stepType}:{branchPath}:{stepId}:{dataType}:{additionalInfo}
            var address = $"{request.ExecutionId}:{request.FlowId}:{request.StepType}:{request.BranchPath}:{request.StepId}:{request.DataType}";

            if (!string.IsNullOrEmpty(request.AdditionalInfo))
            {
                address += $":{request.AdditionalInfo}";
            }

            _logger.LogDebug("Generated memory address: {Address}", address);
            return address;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> GetMemoryStatisticsAsync()
        {
            try
            {
                // This is a simplified implementation
                // In a real-world scenario, you would gather more detailed statistics
                var stats = new Dictionary<string, object>
                {
                    { "ServiceName", "MemoryAllocationService" },
                    { "Timestamp", DateTime.UtcNow }
                };

                // Add more statistics here
                // In a real implementation, we would gather statistics from the cache
                var allKeys = await _cache.GetAllKeysAsync();
                stats["TotalKeys"] = allKeys.Count();

                // Add a small delay to simulate async work
                await Task.Delay(1);

                return stats;
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

            try
            {
                // Get all memory addresses for the execution
                var addresses = await GetExecutionMemoryAddressesAsync(executionId);

                if (addresses == null || !addresses.Any())
                {
                    _logger.LogInformation("No memory addresses found for execution: {ExecutionId}", executionId);
                    return 0;
                }

                // Deallocate each address
                int count = 0;
                foreach (var address in addresses)
                {
                    if (await DeallocateAsync(address))
                    {
                        count++;
                    }
                }

                // Remove the execution index
                await _cache.RemoveAsync($"{MEMORY_INDEX_PREFIX}execution:{executionId}");

                _logger.LogInformation("Cleaned up {Count} memory addresses for execution: {ExecutionId}", count, executionId);
                return count;
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

            try
            {
                // Get all memory addresses for the branch
                var addresses = await GetBranchMemoryAddressesAsync(executionId, branchId);

                if (addresses == null || !addresses.Any())
                {
                    _logger.LogInformation("No memory addresses found for branch: {BranchId} of execution: {ExecutionId}", branchId, executionId);
                    return 0;
                }

                // Deallocate each address
                int count = 0;
                foreach (var address in addresses)
                {
                    if (await DeallocateAsync(address))
                    {
                        count++;
                    }
                }

                // Remove the branch index
                await _cache.RemoveAsync($"{MEMORY_INDEX_PREFIX}branch:{executionId}:{branchId}");

                _logger.LogInformation("Cleaned up {Count} memory addresses for branch: {BranchId} of execution: {ExecutionId}", count, branchId, executionId);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up branch memory");
                return 0;
            }
        }

        private async Task AddToExecutionIndexAsync(string executionId, string memoryAddress)
        {
            var key = $"{MEMORY_INDEX_PREFIX}execution:{executionId}";
            var addresses = await _cache.GetAsync<List<string>>(key) ?? new List<string>();

            if (!addresses.Contains(memoryAddress))
            {
                addresses.Add(memoryAddress);
                await _cache.SetAsync(key, addresses);
            }
        }

        private async Task RemoveFromExecutionIndexAsync(string executionId, string memoryAddress)
        {
            var key = $"{MEMORY_INDEX_PREFIX}execution:{executionId}";
            var addresses = await _cache.GetAsync<List<string>>(key);

            if (addresses != null && addresses.Contains(memoryAddress))
            {
                addresses.Remove(memoryAddress);
                await _cache.SetAsync(key, addresses);
            }
        }

        private async Task AddToBranchIndexAsync(string executionId, string branchId, string memoryAddress)
        {
            var key = $"{MEMORY_INDEX_PREFIX}branch:{executionId}:{branchId}";
            var addresses = await _cache.GetAsync<List<string>>(key) ?? new List<string>();

            if (!addresses.Contains(memoryAddress))
            {
                addresses.Add(memoryAddress);
                await _cache.SetAsync(key, addresses);
            }
        }

        private async Task RemoveFromBranchIndexAsync(string executionId, string branchId, string memoryAddress)
        {
            var key = $"{MEMORY_INDEX_PREFIX}branch:{executionId}:{branchId}";
            var addresses = await _cache.GetAsync<List<string>>(key);

            if (addresses != null && addresses.Contains(memoryAddress))
            {
                addresses.Remove(memoryAddress);
                await _cache.SetAsync(key, addresses);
            }
        }

        private async Task<List<string>> GetExecutionMemoryAddressesAsync(string executionId)
        {
            var key = $"{MEMORY_INDEX_PREFIX}execution:{executionId}";
            var result = await _cache.GetAsync<List<string>>(key);
            return result ?? new List<string>();
        }

        private async Task<List<string>> GetBranchMemoryAddressesAsync(string executionId, string branchId)
        {
            var key = $"{MEMORY_INDEX_PREFIX}branch:{executionId}:{branchId}";
            var result = await _cache.GetAsync<List<string>>(key);
            return result ?? new List<string>();
        }
    }
}
