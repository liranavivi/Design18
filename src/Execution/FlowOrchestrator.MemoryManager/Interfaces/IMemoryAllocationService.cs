using FlowOrchestrator.Abstractions.Messaging.Messages;
using FlowOrchestrator.MemoryManager.Models;

namespace FlowOrchestrator.MemoryManager.Interfaces
{
    /// <summary>
    /// Defines the interface for memory allocation services.
    /// </summary>
    public interface IMemoryAllocationService
    {
        /// <summary>
        /// Allocates memory for a specific request.
        /// </summary>
        /// <param name="request">The memory allocation request.</param>
        /// <returns>The memory allocation result.</returns>
        Task<FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult> AllocateAsync(MemoryAllocationRequest request);

        /// <summary>
        /// Deallocates memory for a specific address.
        /// </summary>
        /// <param name="memoryAddress">The memory address to deallocate.</param>
        /// <returns>True if the memory was successfully deallocated; otherwise, false.</returns>
        Task<bool> DeallocateAsync(string memoryAddress);

        /// <summary>
        /// Checks if a memory address exists.
        /// </summary>
        /// <param name="memoryAddress">The memory address to check.</param>
        /// <returns>True if the memory address exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(string memoryAddress);

        /// <summary>
        /// Generates a memory address for a request.
        /// </summary>
        /// <param name="request">The memory allocation request.</param>
        /// <returns>The generated memory address.</returns>
        string GenerateMemoryAddress(MemoryAllocationRequest request);

        /// <summary>
        /// Parses a memory address string.
        /// </summary>
        /// <param name="address">The address string to parse.</param>
        /// <returns>A dictionary containing the address components.</returns>
        Dictionary<string, string> ParseMemoryAddress(string address);

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
    }
}
