using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.MemoryManager.Models;

namespace FlowOrchestrator.MemoryManager.Interfaces
{
    /// <summary>
    /// Defines the interface for the memory manager service.
    /// </summary>
    public interface IMemoryManager : IService
    {
        /// <summary>
        /// Allocates memory for a specific execution context.
        /// </summary>
        /// <param name="request">The memory allocation request.</param>
        /// <returns>The memory allocation result.</returns>
        Task<FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult> AllocateMemoryAsync(MemoryAllocationRequest request);

        /// <summary>
        /// Deallocates memory for a specific address.
        /// </summary>
        /// <param name="memoryAddress">The memory address to deallocate.</param>
        /// <returns>True if the memory was successfully deallocated; otherwise, false.</returns>
        Task<bool> DeallocateMemoryAsync(string memoryAddress);

        /// <summary>
        /// Checks if a memory address exists.
        /// </summary>
        /// <param name="memoryAddress">The memory address to check.</param>
        /// <returns>True if the memory address exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(string memoryAddress);

        /// <summary>
        /// Gets memory usage statistics.
        /// </summary>
        /// <returns>A dictionary containing memory usage statistics.</returns>
        Task<Dictionary<string, object>> GetMemoryStatisticsAsync();

        /// <summary>
        /// Cleans up memory for a specific execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The number of memory addresses cleaned up.</returns>
        Task<int> CleanupExecutionMemoryAsync(string executionId);

        /// <summary>
        /// Cleans up memory for a specific branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The number of memory addresses cleaned up.</returns>
        Task<int> CleanupBranchMemoryAsync(string executionId, string branchId);

        /// <summary>
        /// Validates access to a memory address for a specific execution context.
        /// </summary>
        /// <param name="memoryAddress">The memory address to validate.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>True if access is allowed; otherwise, false.</returns>
        Task<bool> ValidateAccessAsync(string memoryAddress, FlowOrchestrator.Abstractions.Common.ExecutionContext context);
    }
}
