using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Infrastructure.Data.Hazelcast.Cache;
using FlowOrchestrator.MemoryManager.Interfaces;
using FlowOrchestrator.MemoryManager.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.MemoryManager.Services
{
    /// <summary>
    /// Implementation of the memory access control service.
    /// </summary>
    public class MemoryAccessControlService : IMemoryAccessControlService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<MemoryAccessControlService> _logger;
        private const string ACCESS_CONTROL_PREFIX = "memory:acl:";

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAccessControlService"/> class.
        /// </summary>
        /// <param name="cache">The distributed cache.</param>
        /// <param name="logger">The logger.</param>
        public MemoryAccessControlService(
            IDistributedCache cache,
            ILogger<MemoryAccessControlService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateAccessAsync(string memoryAddress, FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            _logger.LogDebug("Validating access to memory address: {Address} for context: {Context}", memoryAddress, context);

            try
            {
                // Parse the memory address
                if (!MemoryAddress.TryParse(memoryAddress, out var parsedAddress))
                {
                    _logger.LogWarning("Invalid memory address format: {Address}", memoryAddress);
                    return false;
                }

                // Check if the execution context matches the memory address
                if (parsedAddress.ExecutionId == context.ExecutionId)
                {
                    // If the branch path is "main" or matches the context's branch ID, allow access
                    if (parsedAddress.BranchPath == "main" ||
                        parsedAddress.BranchPath == context.BranchId ||
                        context.BranchId == null)
                    {
                        return true;
                    }

                    // Check if the context's branch is a child of the memory address's branch
                    if (context.ParentBranchId == parsedAddress.BranchPath)
                    {
                        return true;
                    }
                }

                // Check the access control list
                var acl = await GetAccessControlListAsync(memoryAddress);
                return acl.Contains(GetContextKey(context));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating memory access");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> GrantAccessAsync(string memoryAddress, FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            _logger.LogDebug("Granting access to memory address: {Address} for context: {Context}", memoryAddress, context);

            try
            {
                var acl = await GetAccessControlListAsync(memoryAddress);
                var contextKey = GetContextKey(context);

                if (!acl.Contains(contextKey))
                {
                    acl.Add(contextKey);
                    await _cache.SetAsync($"{ACCESS_CONTROL_PREFIX}{memoryAddress}", acl);
                    _logger.LogInformation("Access granted to memory address: {Address} for context: {Context}", memoryAddress, context);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting memory access");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeAccessAsync(string memoryAddress, FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            _logger.LogDebug("Revoking access to memory address: {Address} for context: {Context}", memoryAddress, context);

            try
            {
                var acl = await GetAccessControlListAsync(memoryAddress);
                var contextKey = GetContextKey(context);

                if (acl.Contains(contextKey))
                {
                    acl.Remove(contextKey);
                    await _cache.SetAsync($"{ACCESS_CONTROL_PREFIX}{memoryAddress}", acl);
                    _logger.LogInformation("Access revoked from memory address: {Address} for context: {Context}", memoryAddress, context);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking memory access");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetAccessControlListAsync(string memoryAddress)
        {
            try
            {
                var acl = await _cache.GetAsync<List<string>>($"{ACCESS_CONTROL_PREFIX}{memoryAddress}");
                return acl ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access control list");
                return new List<string>();
            }
        }

        private string GetContextKey(FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            return $"{context.ExecutionId}:{context.BranchId ?? "main"}";
        }
    }
}
