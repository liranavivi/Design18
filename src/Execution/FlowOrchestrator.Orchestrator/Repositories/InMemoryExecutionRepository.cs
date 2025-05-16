using FlowOrchestrator.Abstractions.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FlowOrchestrator.Orchestrator.Repositories
{
    /// <summary>
    /// In-memory implementation of IExecutionRepository.
    /// </summary>
    public class InMemoryExecutionRepository : IExecutionRepository
    {
        private readonly ILogger<InMemoryExecutionRepository> _logger;
        private readonly ConcurrentDictionary<string, FlowOrchestrator.Abstractions.Common.ExecutionContext> _executionContexts;
        private readonly ConcurrentDictionary<string, BranchExecutionContext> _branchContexts;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryExecutionRepository"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public InMemoryExecutionRepository(ILogger<InMemoryExecutionRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executionContexts = new ConcurrentDictionary<string, FlowOrchestrator.Abstractions.Common.ExecutionContext>();
            _branchContexts = new ConcurrentDictionary<string, BranchExecutionContext>();
        }

        /// <inheritdoc/>
        public Task<FlowOrchestrator.Abstractions.Common.ExecutionContext?> GetExecutionContextAsync(string executionId)
        {
            _logger.LogDebug("Getting execution context for execution {ExecutionId}", executionId);
            _executionContexts.TryGetValue(executionId, out var context);
            return Task.FromResult(context as FlowOrchestrator.Abstractions.Common.ExecutionContext);
        }

        /// <inheritdoc/>
        public Task SaveExecutionContextAsync(FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            _logger.LogDebug("Saving execution context for execution {ExecutionId}", context.ExecutionId);
            _executionContexts[context.ExecutionId] = context;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdateExecutionContextAsync(FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            _logger.LogDebug("Updating execution context for execution {ExecutionId}", context.ExecutionId);
            _executionContexts[context.ExecutionId] = context;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<BranchExecutionContext?> GetBranchContextAsync(string executionId, string branchId)
        {
            var key = GetBranchKey(executionId, branchId);
            _logger.LogDebug("Getting branch context for branch {BranchId} of execution {ExecutionId}", branchId, executionId);
            _branchContexts.TryGetValue(key, out var context);
            return Task.FromResult(context as BranchExecutionContext);
        }

        /// <inheritdoc/>
        public Task SaveBranchContextAsync(BranchExecutionContext context)
        {
            var key = GetBranchKey(context.ExecutionId, context.BranchId);
            _logger.LogDebug("Saving branch context for branch {BranchId} of execution {ExecutionId}", context.BranchId, context.ExecutionId);
            _branchContexts[key] = context;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdateBranchContextAsync(BranchExecutionContext context)
        {
            var key = GetBranchKey(context.ExecutionId, context.BranchId);
            _logger.LogDebug("Updating branch context for branch {BranchId} of execution {ExecutionId}", context.BranchId, context.ExecutionId);
            _branchContexts[key] = context;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<BranchExecutionContext>> GetBranchContextsForExecutionAsync(string executionId)
        {
            _logger.LogDebug("Getting branch contexts for execution {ExecutionId}", executionId);
            var branches = _branchContexts.Values.Where(b => b.ExecutionId == executionId);
            return Task.FromResult(branches);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FlowOrchestrator.Abstractions.Common.ExecutionContext>> GetActiveExecutionsAsync()
        {
            _logger.LogDebug("Getting active executions");
            var activeExecutions = _executionContexts.Values.Where(e => e.Status == ExecutionStatus.RUNNING);
            return Task.FromResult(activeExecutions);
        }

        /// <inheritdoc/>
        public Task DeleteExecutionAsync(string executionId)
        {
            _logger.LogDebug("Deleting execution {ExecutionId}", executionId);

            // Remove execution context
            _executionContexts.TryRemove(executionId, out _);

            // Remove branch contexts
            var branchKeys = _branchContexts.Keys.Where(k => k.StartsWith($"{executionId}:")).ToList();
            foreach (var key in branchKeys)
            {
                _branchContexts.TryRemove(key, out _);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the key for a branch in the dictionary.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The branch key.</returns>
        private string GetBranchKey(string executionId, string branchId)
        {
            return $"{executionId}:{branchId}";
        }
    }
}
