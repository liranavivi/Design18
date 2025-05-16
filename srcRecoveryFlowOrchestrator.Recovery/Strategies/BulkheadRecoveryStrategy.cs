using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Recovery.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FlowOrchestrator.Recovery.Strategies
{
    /// <summary>
    /// A recovery strategy that implements the bulkhead pattern.
    /// </summary>
    public class BulkheadRecoveryStrategy : AbstractRecoveryStrategy
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _serviceSemaphores = new();
        private readonly ConcurrentDictionary<string, int> _serviceExecutionCounts = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkheadRecoveryStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public BulkheadRecoveryStrategy(ILogger<BulkheadRecoveryStrategy> logger) : base(logger)
        {
        }

        /// <summary>
        /// Gets the name of the strategy.
        /// </summary>
        public override string Name => "BulkheadStrategy";

        /// <summary>
        /// Gets the description of the strategy.
        /// </summary>
        public override string Description => "Isolates failures to prevent system-wide impact";

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateConfiguration(Dictionary<string, object> parameters)
        {
            var errors = new List<string>();

            // Validate MaxConcurrentExecutions
            if (parameters.TryGetValue("MaxConcurrentExecutions", out var maxConcurrentExecutionsObj))
            {
                if (maxConcurrentExecutionsObj is not int maxConcurrentExecutions || maxConcurrentExecutions <= 0)
                {
                    errors.Add("MaxConcurrentExecutions must be a positive integer");
                }
            }

            // Validate BulkheadType
            if (parameters.TryGetValue("BulkheadType", out var bulkheadTypeObj))
            {
                if (bulkheadTypeObj is not string bulkheadType || 
                    !Enum.TryParse<BulkheadType>(bulkheadType, true, out _))
                {
                    errors.Add("BulkheadType must be one of: Service, Branch, Resource");
                }
            }

            return errors.Count > 0 
                ? ValidationResult.Invalid(errors) 
                : ValidationResult.Valid();
        }

        /// <summary>
        /// Determines whether this strategy is applicable to the specified error context.
        /// </summary>
        /// <param name="errorContext">The error context.</param>
        /// <returns><c>true</c> if this strategy is applicable; otherwise, <c>false</c>.</returns>
        public override bool IsApplicable(ErrorContext errorContext)
        {
            // Bulkhead is applicable when we need to isolate failures
            // For now, we'll apply it to resource-related errors
            return errorContext.Classification switch
            {
                FlowOrchestrator.Common.Exceptions.ErrorClassification.RESOURCE_ERROR => true,
                FlowOrchestrator.Common.Exceptions.ErrorClassification.RESOURCE_UNAVAILABLE => true,
                FlowOrchestrator.Common.Exceptions.ErrorClassification.RESOURCE_QUOTA_EXCEEDED => true,
                _ => false
            };
        }

        /// <summary>
        /// Executes the recovery strategy.
        /// </summary>
        /// <param name="context">The recovery context.</param>
        /// <returns>The recovery result.</returns>
        public override RecoveryResult Recover(RecoveryContext context)
        {
            _logger.LogInformation("Executing bulkhead recovery strategy for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // Get configuration values
            int maxConcurrentExecutions = GetConfigValue("MaxConcurrentExecutions", 10);
            string bulkheadTypeStr = GetConfigValue("BulkheadType", "Service");
            Enum.TryParse<BulkheadType>(bulkheadTypeStr, true, out var bulkheadType);

            // Get the isolation key based on the bulkhead type
            string isolationKey = GetIsolationKey(context, bulkheadType);

            // Check if we're already at the maximum concurrent executions
            if (_serviceExecutionCounts.TryGetValue(isolationKey, out var executionCount) && 
                executionCount >= maxConcurrentExecutions)
            {
                _logger.LogWarning("Bulkhead for {IsolationKey} is full ({ExecutionCount}/{MaxConcurrentExecutions})", 
                    isolationKey, executionCount, maxConcurrentExecutions);
                
                return RecoveryResult.Failed(
                    RecoveryAction.FAIL_FAST,
                    $"Bulkhead for {isolationKey} is full ({executionCount}/{maxConcurrentExecutions})",
                    context,
                    context.ErrorContext);
            }

            // Get or create the semaphore for this isolation key
            var semaphore = _serviceSemaphores.GetOrAdd(isolationKey, _ => new SemaphoreSlim(maxConcurrentExecutions));

            // Try to acquire the semaphore
            bool acquired = semaphore.Wait(0);
            if (!acquired)
            {
                _logger.LogWarning("Failed to acquire semaphore for {IsolationKey}", isolationKey);
                
                return RecoveryResult.Failed(
                    RecoveryAction.FAIL_FAST,
                    $"Failed to acquire semaphore for {isolationKey}",
                    context,
                    context.ErrorContext);
            }

            try
            {
                // Increment the execution count
                _serviceExecutionCounts.AddOrUpdate(
                    isolationKey,
                    1,
                    (_, count) => count + 1);

                _logger.LogInformation("Acquired semaphore for {IsolationKey} ({ExecutionCount}/{MaxConcurrentExecutions})", 
                    isolationKey, _serviceExecutionCounts.GetOrAdd(isolationKey, 0), maxConcurrentExecutions);

                // In a real implementation, we would execute the operation in the bulkhead
                // For now, we'll just return a successful result
                return RecoveryResult.Successful(
                    RecoveryAction.RETRY,
                    $"Operation isolated in bulkhead for {isolationKey}",
                    context);
            }
            finally
            {
                // Release the semaphore and decrement the execution count
                semaphore.Release();
                _serviceExecutionCounts.AddOrUpdate(
                    isolationKey,
                    0,
                    (_, count) => Math.Max(0, count - 1));
            }
        }

        private string GetIsolationKey(RecoveryContext context, BulkheadType bulkheadType)
        {
            return bulkheadType switch
            {
                BulkheadType.Service => $"service:{context.ErrorContext.ServiceId}",
                BulkheadType.Branch => $"branch:{context.ErrorContext.ExecutionId}:{context.ErrorContext.BranchId}",
                BulkheadType.Resource => $"resource:{context.ErrorContext.ServiceId}:{context.ErrorContext.ErrorCode}",
                _ => $"service:{context.ErrorContext.ServiceId}"
            };
        }
    }

    /// <summary>
    /// Defines the types of bulkhead isolation.
    /// </summary>
    public enum BulkheadType
    {
        /// <summary>
        /// Isolate by service.
        /// </summary>
        Service,

        /// <summary>
        /// Isolate by branch.
        /// </summary>
        Branch,

        /// <summary>
        /// Isolate by resource.
        /// </summary>
        Resource
    }
}
