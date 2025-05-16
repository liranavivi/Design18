using FlowOrchestrator.BranchController.Models;
using FlowOrchestrator.BranchController.Messaging.Events;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.BranchController.Services
{
    /// <summary>
    /// Service for managing branch completion.
    /// </summary>
    public class BranchCompletionService
    {
        private readonly ILogger<BranchCompletionService> _logger;
        private readonly BranchContextService _branchContextService;
        private readonly BranchIsolationService _branchIsolationService;
        private readonly IMessageBus _messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchCompletionService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="branchContextService">The branch context service.</param>
        /// <param name="branchIsolationService">The branch isolation service.</param>
        /// <param name="messageBus">The message bus.</param>
        public BranchCompletionService(
            ILogger<BranchCompletionService> logger,
            BranchContextService branchContextService,
            BranchIsolationService branchIsolationService,
            IMessageBus messageBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _branchContextService = branchContextService ?? throw new ArgumentNullException(nameof(branchContextService));
            _branchIsolationService = branchIsolationService ?? throw new ArgumentNullException(nameof(branchIsolationService));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <summary>
        /// Checks if a branch is complete.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>True if the branch is complete, false otherwise.</returns>
        public bool IsBranchComplete(string executionId, string branchId)
        {
            var branchContext = _branchContextService.GetBranch(executionId, branchId);
            if (branchContext == null)
            {
                _logger.LogWarning("Cannot check completion for non-existent branch {BranchId}", branchId);
                return false;
            }

            // A branch is complete if it has no pending steps
            return branchContext.PendingSteps.Count == 0;
        }

        /// <summary>
        /// Completes a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>True if the branch was completed, false otherwise.</returns>
        public async Task<bool> CompleteBranchAsync(string executionId, string branchId)
        {
            var branchContext = _branchContextService.GetBranch(executionId, branchId);
            if (branchContext == null)
            {
                _logger.LogWarning("Cannot complete non-existent branch {BranchId}", branchId);
                return false;
            }

            if (branchContext.Status == BranchStatus.COMPLETED)
            {
                _logger.LogInformation("Branch {BranchId} is already completed", branchId);
                return true;
            }

            if (branchContext.PendingSteps.Count > 0)
            {
                _logger.LogWarning("Cannot complete branch {BranchId} with pending steps", branchId);
                return false;
            }

            // Update branch status to COMPLETED
            await _branchContextService.UpdateBranchStatusAsync(executionId, branchId, BranchStatus.COMPLETED);

            // Collect branch statistics
            branchContext.Statistics["TotalSteps"] = branchContext.CompletedSteps.Count;
            branchContext.Statistics["ExecutionTimeMs"] = (branchContext.EndTimestamp ?? DateTime.UtcNow) - branchContext.StartTimestamp;

            // Publish branch completed event
            await _messageBus.PublishAsync(new BranchCompletedEvent
            {
                ExecutionId = executionId,
                BranchId = branchId,
                Timestamp = DateTime.UtcNow,
                Statistics = branchContext.Statistics,
                Status = BranchStatus.COMPLETED.ToString()
            });

            _logger.LogInformation("Branch {BranchId} completed successfully", branchId);
            return true;
        }

        /// <summary>
        /// Fails a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="error">The error information.</param>
        /// <returns>True if the branch was failed, false otherwise.</returns>
        public async Task<bool> FailBranchAsync(string executionId, string branchId, FlowOrchestrator.Abstractions.Common.ExecutionError error)
        {
            var branchContext = _branchContextService.GetBranch(executionId, branchId);
            if (branchContext == null)
            {
                _logger.LogWarning("Cannot fail non-existent branch {BranchId}", branchId);
                return false;
            }

            if (branchContext.Status == BranchStatus.FAILED)
            {
                _logger.LogInformation("Branch {BranchId} is already failed", branchId);
                return true;
            }

            // Update branch status to FAILED
            branchContext.Error = error;
            await _branchContextService.UpdateBranchStatusAsync(executionId, branchId, BranchStatus.FAILED);

            // Publish branch failed event
            await _messageBus.PublishAsync(new BranchFailedEvent
            {
                ExecutionId = executionId,
                BranchId = branchId,
                Timestamp = DateTime.UtcNow,
                Error = error
            });

            _logger.LogInformation("Branch {BranchId} failed with error {ErrorCode}", branchId, error.ErrorCode);
            return true;
        }

        /// <summary>
        /// Checks if all branches for an execution are complete.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>True if all branches are complete, false otherwise.</returns>
        public bool AreAllBranchesComplete(string executionId)
        {
            var branches = _branchContextService.GetBranchesForExecution(executionId);
            return branches.All(b => b.Status == BranchStatus.COMPLETED || 
                                    b.Status == BranchStatus.FAILED || 
                                    b.Status == BranchStatus.CANCELLED);
        }
    }
}
