using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.MemoryManager.Interfaces
{
    /// <summary>
    /// Defines the interface for memory access control services.
    /// </summary>
    public interface IMemoryAccessControlService
    {
        /// <summary>
        /// Validates access to a memory address for a specific execution context.
        /// </summary>
        /// <param name="memoryAddress">The memory address to validate.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>True if access is allowed; otherwise, false.</returns>
        Task<bool> ValidateAccessAsync(string memoryAddress, FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Grants access to a memory address for a specific execution context.
        /// </summary>
        /// <param name="memoryAddress">The memory address.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>True if access was granted; otherwise, false.</returns>
        Task<bool> GrantAccessAsync(string memoryAddress, FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Revokes access to a memory address for a specific execution context.
        /// </summary>
        /// <param name="memoryAddress">The memory address.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>True if access was revoked; otherwise, false.</returns>
        Task<bool> RevokeAccessAsync(string memoryAddress, FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Gets the access control list for a memory address.
        /// </summary>
        /// <param name="memoryAddress">The memory address.</param>
        /// <returns>The list of execution contexts that have access to the memory address.</returns>
        Task<List<string>> GetAccessControlListAsync(string memoryAddress);
    }
}
