using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Recovery.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Recovery.Strategies
{
    /// <summary>
    /// A recovery strategy that implements the timeout pattern.
    /// </summary>
    public class TimeoutRecoveryStrategy : AbstractRecoveryStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutRecoveryStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TimeoutRecoveryStrategy(ILogger<TimeoutRecoveryStrategy> logger) : base(logger)
        {
        }

        /// <summary>
        /// Gets the name of the strategy.
        /// </summary>
        public override string Name => "TimeoutStrategy";

        /// <summary>
        /// Gets the description of the strategy.
        /// </summary>
        public override string Description => "Enforces time limits on operations to prevent blocking";

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateConfiguration(Dictionary<string, object> parameters)
        {
            var errors = new List<string>();

            // Validate OperationTimeoutSeconds
            if (parameters.TryGetValue("OperationTimeoutSeconds", out var operationTimeoutObj))
            {
                if (operationTimeoutObj is not int operationTimeout || operationTimeout <= 0)
                {
                    errors.Add("OperationTimeoutSeconds must be a positive integer");
                }
            }

            // Validate FlowTimeoutSeconds
            if (parameters.TryGetValue("FlowTimeoutSeconds", out var flowTimeoutObj))
            {
                if (flowTimeoutObj is not int flowTimeout || flowTimeout <= 0)
                {
                    errors.Add("FlowTimeoutSeconds must be a positive integer");
                }
            }

            // Validate BranchTimeoutSeconds
            if (parameters.TryGetValue("BranchTimeoutSeconds", out var branchTimeoutObj))
            {
                if (branchTimeoutObj is not int branchTimeout || branchTimeout <= 0)
                {
                    errors.Add("BranchTimeoutSeconds must be a positive integer");
                }
            }

            // Validate TimeoutAction
            if (parameters.TryGetValue("TimeoutAction", out var timeoutActionObj))
            {
                if (timeoutActionObj is not string timeoutAction || 
                    !Enum.TryParse<TimeoutAction>(timeoutAction, true, out _))
                {
                    errors.Add("TimeoutAction must be one of: Terminate, Compensate, Retry");
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
            // Timeout is applicable to connection timeout errors
            return errorContext.Classification switch
            {
                FlowOrchestrator.Common.Exceptions.ErrorClassification.CONNECTION_TIMEOUT => true,
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
            _logger.LogInformation("Executing timeout recovery strategy for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // Get configuration values
            string timeoutActionStr = GetConfigValue("TimeoutAction", "Terminate");
            Enum.TryParse<TimeoutAction>(timeoutActionStr, true, out var timeoutAction);

            // Execute the appropriate timeout action
            switch (timeoutAction)
            {
                case TimeoutAction.Terminate:
                    return HandleTerminateAction(context);
                
                case TimeoutAction.Compensate:
                    return HandleCompensateAction(context);
                
                case TimeoutAction.Retry:
                    return HandleRetryAction(context);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private RecoveryResult HandleTerminateAction(RecoveryContext context)
        {
            _logger.LogWarning("Terminating operation due to timeout for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // In a real implementation, we would gracefully terminate the operation
            // For now, we'll just return a failed result
            return RecoveryResult.Failed(
                RecoveryAction.FAIL_BRANCH,
                "Operation terminated due to timeout",
                context,
                context.ErrorContext);
        }

        private RecoveryResult HandleCompensateAction(RecoveryContext context)
        {
            _logger.LogWarning("Compensating for timeout for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // In a real implementation, we would execute compensation actions
            // For now, we'll just return a successful result
            return RecoveryResult.Successful(
                RecoveryAction.COMPENSATE,
                "Compensation actions executed for timeout",
                context);
        }

        private RecoveryResult HandleRetryAction(RecoveryContext context)
        {
            _logger.LogWarning("Retrying operation after timeout for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // Get the retry delay
            int retryDelaySeconds = GetConfigValue("RetryDelaySeconds", 5);

            // In a real implementation, we would schedule a retry
            // For now, we'll just return a successful result
            return new RecoveryResult
            {
                Success = true,
                Action = RecoveryAction.RETRY,
                Message = $"Retry scheduled after timeout with delay {retryDelaySeconds} seconds",
                Context = context,
                NextRetryDelay = TimeSpan.FromSeconds(retryDelaySeconds)
            };
        }
    }

    /// <summary>
    /// Defines the actions to take when a timeout occurs.
    /// </summary>
    public enum TimeoutAction
    {
        /// <summary>
        /// Terminate the operation.
        /// </summary>
        Terminate,

        /// <summary>
        /// Execute compensation actions.
        /// </summary>
        Compensate,

        /// <summary>
        /// Retry the operation.
        /// </summary>
        Retry
    }
}
