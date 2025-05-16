using System;
using System.Threading.Tasks;

namespace FlowOrchestrator.MemoryManager.Services
{
    /// <summary>
    /// Interface for the memory manager service.
    /// </summary>
    public interface IMemoryManagerService
    {
        /// <summary>
        /// Allocates memory for a flow execution step.
        /// </summary>
        /// <param name="size">The size of the memory to allocate in bytes.</param>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <param name="memoryType">The memory type.</param>
        /// <param name="timeToLive">The time-to-live for the memory allocation.</param>
        /// <returns>The memory address.</returns>
        Task<string> AllocateMemoryAsync(
            int size,
            string executionId,
            string flowId,
            string stepId,
            string memoryType = "Default",
            TimeSpan? timeToLive = null);

        /// <summary>
        /// Releases memory for a flow execution step.
        /// </summary>
        /// <param name="memoryAddress">The memory address.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ReleaseMemoryAsync(string memoryAddress);

        /// <summary>
        /// Gets the memory usage for a flow execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The memory usage in bytes.</returns>
        Task<long> GetMemoryUsageAsync(string executionId);
    }
}
