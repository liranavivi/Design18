using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Orchestrator.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FlowOrchestrator.Orchestrator.Services
{
    /// <summary>
    /// Service for managing branch execution.
    /// </summary>
    public class BranchManagementService
    {
        private readonly ILogger<BranchManagementService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly MemoryAddressService _memoryAddressService;
        private readonly IExecutionRepository _executionRepository;
        private readonly TelemetryService _telemetryService;
        private readonly ConcurrentDictionary<string, Activity> _branchActivities;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchManagementService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageBus">The message bus.</param>
        /// <param name="memoryAddressService">The memory address service.</param>
        /// <param name="executionRepository">The execution repository.</param>
        /// <param name="telemetryService">The telemetry service.</param>
        public BranchManagementService(
            ILogger<BranchManagementService> logger,
            IMessageBus messageBus,
            MemoryAddressService memoryAddressService,
            IExecutionRepository executionRepository,
            TelemetryService telemetryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _memoryAddressService = memoryAddressService ?? throw new ArgumentNullException(nameof(memoryAddressService));
            _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _branchActivities = new ConcurrentDictionary<string, Activity>();
        }

        /// <summary>
        /// Creates a new branch execution context.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="parentBranchId">The parent branch identifier.</param>
        /// <returns>The branch execution context.</returns>
        public async Task<BranchExecutionContext> CreateBranchAsync(FlowOrchestrator.Abstractions.Common.ExecutionContext executionContext, string branchId, string? parentBranchId = null)
        {
            _logger.LogInformation("Creating branch {BranchId} for execution {ExecutionId}", branchId, executionContext.ExecutionId);

            var branchContext = new BranchExecutionContext
            {
                ExecutionId = executionContext.ExecutionId,
                BranchId = branchId,
                ParentBranchId = parentBranchId,
                Status = BranchStatus.NEW,
                StartTimestamp = DateTime.UtcNow,
                Parameters = new Dictionary<string, object>(executionContext.Parameters)
            };

            // Start branch telemetry tracking
            var parentActivity = _branchActivities.TryGetValue(GetBranchKey(executionContext.ExecutionId, parentBranchId ?? "main"), out var activity)
                ? activity
                : null;

            var branchActivity = _telemetryService.RecordBranchExecutionStart(executionContext.ExecutionId, branchId, parentActivity);
            if (branchActivity != null)
            {
                _branchActivities[GetBranchKey(executionContext.ExecutionId, branchId)] = branchActivity;
            }

            // Store branch execution context in repository
            await _executionRepository.SaveBranchContextAsync(branchContext);

            _logger.LogInformation("Branch {BranchId} created for execution {ExecutionId}", branchId, executionContext.ExecutionId);

            return branchContext;
        }

        /// <summary>
        /// Creates a new branch execution context.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="parentBranchId">The parent branch identifier.</param>
        /// <returns>The branch execution context.</returns>
        public BranchExecutionContext CreateBranch(FlowOrchestrator.Abstractions.Common.ExecutionContext executionContext, string branchId, string? parentBranchId = null)
        {
            return CreateBranchAsync(executionContext, branchId, parentBranchId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a branch execution context.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The branch execution context, or null if not found.</returns>
        public async Task<BranchExecutionContext?> GetBranchAsync(string executionId, string branchId)
        {
            return await _executionRepository.GetBranchContextAsync(executionId, branchId);
        }

        /// <summary>
        /// Gets a branch execution context.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The branch execution context, or null if not found.</returns>
        public BranchExecutionContext? GetBranch(string executionId, string branchId)
        {
            return GetBranchAsync(executionId, branchId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Updates the status of a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="status">The new status.</param>
        /// <returns>True if the branch was updated, false otherwise.</returns>
        public async Task<bool> UpdateBranchStatusAsync(string executionId, string branchId, BranchStatus status)
        {
            var branchContext = await _executionRepository.GetBranchContextAsync(executionId, branchId);
            if (branchContext != null)
            {
                _logger.LogInformation("Updating branch {BranchId} status from {OldStatus} to {NewStatus}",
                    branchId, branchContext.Status, status);

                var oldStatus = branchContext.Status;
                branchContext.Status = status;

                if (status == BranchStatus.COMPLETED || status == BranchStatus.FAILED || status == BranchStatus.CANCELLED)
                {
                    branchContext.EndTimestamp = DateTime.UtcNow;

                    // Record branch completion in telemetry
                    if (_branchActivities.TryRemove(GetBranchKey(executionId, branchId), out var activity))
                    {
                        _telemetryService.RecordBranchExecutionEnd(executionId, branchId, status, activity);
                    }
                }

                await _executionRepository.UpdateBranchContextAsync(branchContext);
                return true;
            }

            _logger.LogWarning("Branch {BranchId} not found for execution {ExecutionId}", branchId, executionId);
            return false;
        }

        /// <summary>
        /// Updates the status of a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="status">The new status.</param>
        /// <returns>True if the branch was updated, false otherwise.</returns>
        public bool UpdateBranchStatus(string executionId, string branchId, BranchStatus status)
        {
            return UpdateBranchStatusAsync(executionId, branchId, status).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a completed step to a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <returns>True if the step was added, false otherwise.</returns>
        public async Task<bool> AddCompletedStepAsync(string executionId, string branchId, string stepId)
        {
            var branchContext = await _executionRepository.GetBranchContextAsync(executionId, branchId);
            if (branchContext != null)
            {
                _logger.LogInformation("Adding completed step {StepId} to branch {BranchId}", stepId, branchId);

                branchContext.CompletedSteps.Add(stepId);
                branchContext.PendingSteps.Remove(stepId);

                await _executionRepository.UpdateBranchContextAsync(branchContext);
                return true;
            }

            _logger.LogWarning("Branch {BranchId} not found for execution {ExecutionId}", branchId, executionId);
            return false;
        }

        /// <summary>
        /// Adds a completed step to a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <returns>True if the step was added, false otherwise.</returns>
        public bool AddCompletedStep(string executionId, string branchId, string stepId)
        {
            return AddCompletedStepAsync(executionId, branchId, stepId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all branches for an execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The branch execution contexts.</returns>
        public async Task<IEnumerable<BranchExecutionContext>> GetBranchesForExecutionAsync(string executionId)
        {
            return await _executionRepository.GetBranchContextsForExecutionAsync(executionId);
        }

        /// <summary>
        /// Gets all branches for an execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The branch execution contexts.</returns>
        public IEnumerable<BranchExecutionContext> GetBranchesForExecution(string executionId)
        {
            return GetBranchesForExecutionAsync(executionId).GetAwaiter().GetResult();
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
