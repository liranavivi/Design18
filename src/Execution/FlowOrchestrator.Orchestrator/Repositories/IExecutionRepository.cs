using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Orchestrator.Repositories
{
    /// <summary>
    /// Interface for execution repository.
    /// </summary>
    public interface IExecutionRepository
    {
        /// <summary>
        /// Gets an execution context by its identifier.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The execution context, or null if not found.</returns>
        Task<FlowOrchestrator.Abstractions.Common.ExecutionContext?> GetExecutionContextAsync(string executionId);

        /// <summary>
        /// Saves an execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveExecutionContextAsync(FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Updates an execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateExecutionContextAsync(FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Gets a branch execution context by its identifiers.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The branch execution context, or null if not found.</returns>
        Task<BranchExecutionContext?> GetBranchContextAsync(string executionId, string branchId);

        /// <summary>
        /// Saves a branch execution context.
        /// </summary>
        /// <param name="context">The branch execution context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveBranchContextAsync(BranchExecutionContext context);

        /// <summary>
        /// Updates a branch execution context.
        /// </summary>
        /// <param name="context">The branch execution context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateBranchContextAsync(BranchExecutionContext context);

        /// <summary>
        /// Gets all branch execution contexts for an execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The branch execution contexts.</returns>
        Task<IEnumerable<BranchExecutionContext>> GetBranchContextsForExecutionAsync(string executionId);

        /// <summary>
        /// Gets all active executions.
        /// </summary>
        /// <returns>The active execution contexts.</returns>
        Task<IEnumerable<FlowOrchestrator.Abstractions.Common.ExecutionContext>> GetActiveExecutionsAsync();

        /// <summary>
        /// Deletes an execution context and all its branch contexts.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteExecutionAsync(string executionId);
    }
}
