using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Recovery.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Recovery.Strategies
{
    /// <summary>
    /// A recovery strategy that implements the compensation pattern.
    /// </summary>
    public class CompensationRecoveryStrategy : AbstractRecoveryStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompensationRecoveryStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CompensationRecoveryStrategy(ILogger<CompensationRecoveryStrategy> logger) : base(logger)
        {
        }

        /// <summary>
        /// Gets the name of the strategy.
        /// </summary>
        public override string Name => "CompensationStrategy";

        /// <summary>
        /// Gets the description of the strategy.
        /// </summary>
        public override string Description => "Reverses completed steps when later steps fail";

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateConfiguration(Dictionary<string, object> parameters)
        {
            var errors = new List<string>();

            // Validate CompensationTimeoutSeconds
            if (parameters.TryGetValue("CompensationTimeoutSeconds", out var timeoutObj))
            {
                if (timeoutObj is not int timeout || timeout <= 0)
                {
                    errors.Add("CompensationTimeoutSeconds must be a positive integer");
                }
            }

            // Validate CompensationOrder
            if (parameters.TryGetValue("CompensationOrder", out var orderObj))
            {
                if (orderObj is not string order || 
                    !Enum.TryParse<CompensationOrder>(order, true, out _))
                {
                    errors.Add("CompensationOrder must be one of: ReverseOrder, ParallelCompensation");
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
            // Compensation is applicable when we need to reverse completed steps
            // For now, we'll apply it to processing errors
            return errorContext.Classification switch
            {
                FlowOrchestrator.Common.Exceptions.ErrorClassification.PROCESSING_ERROR => true,
                FlowOrchestrator.Common.Exceptions.ErrorClassification.DATA_ERROR => true,
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
            _logger.LogInformation("Executing compensation recovery strategy for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // Get configuration values
            int compensationTimeoutSeconds = GetConfigValue("CompensationTimeoutSeconds", 60);
            string compensationOrderStr = GetConfigValue("CompensationOrder", "ReverseOrder");
            Enum.TryParse<CompensationOrder>(compensationOrderStr, true, out var compensationOrder);

            // In a real implementation, we would:
            // 1. Retrieve the completed steps for the execution
            // 2. Determine which steps need compensation
            // 3. Execute the compensation actions in the appropriate order
            // 4. Track the compensation results

            // For now, we'll just simulate a successful compensation
            _logger.LogInformation("Simulating compensation for execution {ExecutionId}, branch {BranchId}", 
                context.ErrorContext.ExecutionId, context.ErrorContext.BranchId);

            // Add this attempt to the history
            context.History.Add(new RecoveryAttempt
            {
                AttemptNumber = 1,
                Timestamp = DateTime.UtcNow,
                StrategyName = Name,
                Success = true,
                ResultMessage = "Compensation actions executed successfully"
            });

            return RecoveryResult.Successful(
                RecoveryAction.COMPENSATE,
                "Compensation actions executed successfully",
                context);
        }
    }

    /// <summary>
    /// Defines the order in which compensation actions are executed.
    /// </summary>
    public enum CompensationOrder
    {
        /// <summary>
        /// Execute compensation actions in reverse order of the original operations.
        /// </summary>
        ReverseOrder,

        /// <summary>
        /// Execute compensation actions in parallel.
        /// </summary>
        ParallelCompensation
    }
}
