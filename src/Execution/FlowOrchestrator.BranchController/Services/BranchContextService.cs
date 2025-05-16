using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.BranchController.Models;
using FlowOrchestrator.BranchController.Messaging.Events;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FlowOrchestrator.BranchController.Services
{
    /// <summary>
    /// Service for managing branch execution contexts.
    /// </summary>
    public class BranchContextService
    {
        private readonly ILogger<BranchContextService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly TelemetryService _telemetryService;
        private readonly ConcurrentDictionary<string, BranchExecutionContext> _branchContexts;
        private readonly ConcurrentDictionary<string, Activity> _branchActivities;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchContextService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageBus">The message bus.</param>
        /// <param name="telemetryService">The telemetry service.</param>
        public BranchContextService(
            ILogger<BranchContextService> logger,
            IMessageBus messageBus,
            TelemetryService telemetryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _branchContexts = new ConcurrentDictionary<string, BranchExecutionContext>();
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

            // Store branch execution context
            _branchContexts[GetBranchKey(executionContext.ExecutionId, branchId)] = branchContext;

            // Publish branch created event
            await _messageBus.PublishAsync(new BranchCreatedEvent
            {
                ExecutionId = executionContext.ExecutionId,
                BranchId = branchId,
                ParentBranchId = parentBranchId,
                Timestamp = DateTime.UtcNow,
                Priority = branchContext.Priority,
                TimeoutMs = branchContext.TimeoutMs
            });

            _logger.LogInformation("Branch {BranchId} created for execution {ExecutionId}", branchId, executionContext.ExecutionId);

            return branchContext;
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
            var key = GetBranchKey(executionId, branchId);
            if (_branchContexts.TryGetValue(key, out var branchContext))
            {
                _logger.LogInformation("Updating branch {BranchId} status from {OldStatus} to {NewStatus}",
                    branchId, branchContext.Status, status);

                var oldStatus = branchContext.Status;
                branchContext.Status = status;

                if (status == BranchStatus.COMPLETED || status == BranchStatus.FAILED || status == BranchStatus.CANCELLED)
                {
                    branchContext.EndTimestamp = DateTime.UtcNow;

                    // Record branch completion in telemetry
                    if (_branchActivities.TryRemove(key, out var activity))
                    {
                        _telemetryService.RecordBranchExecutionEnd(executionId, branchId, status, activity);
                    }

                    // Publish appropriate event based on status
                    if (status == BranchStatus.COMPLETED)
                    {
                        await _messageBus.PublishAsync(new BranchCompletedEvent
                        {
                            ExecutionId = executionId,
                            BranchId = branchId,
                            Timestamp = DateTime.UtcNow,
                            Statistics = branchContext.Statistics,
                            Status = status.ToString()
                        });
                    }
                    else if (status == BranchStatus.FAILED && branchContext.Error != null)
                    {
                        await _messageBus.PublishAsync(new BranchFailedEvent
                        {
                            ExecutionId = executionId,
                            BranchId = branchId,
                            Timestamp = DateTime.UtcNow,
                            Error = branchContext.Error
                        });
                    }
                }

                // Publish status changed event
                await _messageBus.PublishAsync(new BranchStatusChangedEvent
                {
                    ExecutionId = executionId,
                    BranchId = branchId,
                    OldStatus = oldStatus.ToString(),
                    NewStatus = status.ToString(),
                    Timestamp = DateTime.UtcNow
                });

                return true;
            }

            _logger.LogWarning("Branch {BranchId} not found for execution {ExecutionId}", branchId, executionId);
            return false;
        }

        /// <summary>
        /// Gets a branch execution context.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The branch execution context, or null if not found.</returns>
        public BranchExecutionContext? GetBranch(string executionId, string branchId)
        {
            var key = GetBranchKey(executionId, branchId);
            _branchContexts.TryGetValue(key, out var branchContext);
            return branchContext;
        }

        /// <summary>
        /// Gets all branches for an execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The branch execution contexts.</returns>
        public IEnumerable<BranchExecutionContext> GetBranchesForExecution(string executionId)
        {
            return _branchContexts.Values.Where(b => b.ExecutionId == executionId);
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
            var key = GetBranchKey(executionId, branchId);
            if (_branchContexts.TryGetValue(key, out var branchContext))
            {
                _logger.LogInformation("Adding completed step {StepId} to branch {BranchId}", stepId, branchId);

                branchContext.CompletedSteps.Add(stepId);
                branchContext.PendingSteps.Remove(stepId);
                return true;
            }

            _logger.LogWarning("Branch {BranchId} not found for execution {ExecutionId}", branchId, executionId);
            return false;
        }

        /// <summary>
        /// Adds a pending step to a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <returns>True if the step was added, false otherwise.</returns>
        public bool AddPendingStep(string executionId, string branchId, string stepId)
        {
            var key = GetBranchKey(executionId, branchId);
            if (_branchContexts.TryGetValue(key, out var branchContext))
            {
                _logger.LogInformation("Adding pending step {StepId} to branch {BranchId}", stepId, branchId);

                if (!branchContext.PendingSteps.Contains(stepId))
                {
                    branchContext.PendingSteps.Add(stepId);
                }
                return true;
            }

            _logger.LogWarning("Branch {BranchId} not found for execution {ExecutionId}", branchId, executionId);
            return false;
        }

        /// <summary>
        /// Gets the branch key.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The branch key.</returns>
        private static string GetBranchKey(string executionId, string branchId)
        {
            return $"{executionId}:{branchId}";
        }
    }
}
