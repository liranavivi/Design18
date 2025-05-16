using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Common.Exceptions;
using FlowOrchestrator.Recovery.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Recovery.Strategies
{
    /// <summary>
    /// A recovery strategy that implements the retry pattern.
    /// </summary>
    public class RetryRecoveryStrategy : AbstractRecoveryStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryRecoveryStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public RetryRecoveryStrategy(ILogger<RetryRecoveryStrategy> logger) : base(logger)
        {
        }

        /// <summary>
        /// Gets the name of the strategy.
        /// </summary>
        public override string Name => "RetryStrategy";

        /// <summary>
        /// Gets the description of the strategy.
        /// </summary>
        public override string Description => "Automatically retry failed operations with configurable backoff";

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateConfiguration(Dictionary<string, object> parameters)
        {
            var errors = new List<string>();

            // Validate MaxRetryCount
            if (parameters.TryGetValue("MaxRetryCount", out var maxRetryCountObj))
            {
                if (maxRetryCountObj is not int maxRetryCount || maxRetryCount < 0)
                {
                    errors.Add("MaxRetryCount must be a non-negative integer");
                }
            }

            // Validate RetryDelayMilliseconds
            if (parameters.TryGetValue("RetryDelayMilliseconds", out var retryDelayObj))
            {
                if (retryDelayObj is not int retryDelay || retryDelay < 0)
                {
                    errors.Add("RetryDelayMilliseconds must be a non-negative integer");
                }
            }

            // Validate BackoffFactor
            if (parameters.TryGetValue("BackoffFactor", out var backoffFactorObj))
            {
                if (backoffFactorObj is not double backoffFactor || backoffFactor < 1.0)
                {
                    errors.Add("BackoffFactor must be a double greater than or equal to 1.0");
                }
            }

            // Validate RetryType
            if (parameters.TryGetValue("RetryType", out var retryTypeObj))
            {
                if (retryTypeObj is not string retryType || 
                    !Enum.TryParse<RetryType>(retryType, true, out _))
                {
                    errors.Add("RetryType must be one of: Fixed, Exponential, Jittered");
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
            // Check if the error is retryable based on classification
            bool isRetryable = errorContext.Classification switch
            {
                ErrorClassification.CONNECTION_TIMEOUT => true,
                ErrorClassification.CONNECTION_ERROR => true,
                ErrorClassification.RESOURCE_UNAVAILABLE => true,
                _ => false
            };

            // Check if we've exceeded the maximum retry count
            int maxRetryCount = GetConfigValue("MaxRetryCount", 3);
            bool belowMaxRetries = errorContext.RetryAttempts < maxRetryCount;

            return isRetryable && belowMaxRetries;
        }

        /// <summary>
        /// Executes the recovery strategy.
        /// </summary>
        /// <param name="context">The recovery context.</param>
        /// <returns>The recovery result.</returns>
        public override RecoveryResult Recover(RecoveryContext context)
        {
            _logger.LogInformation("Executing retry recovery strategy for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // Get configuration values
            int maxRetryCount = GetConfigValue("MaxRetryCount", 3);
            int baseRetryDelay = GetConfigValue("RetryDelayMilliseconds", 1000);
            double backoffFactor = GetConfigValue("BackoffFactor", 2.0);
            string retryTypeStr = GetConfigValue("RetryType", "Exponential");
            Enum.TryParse<RetryType>(retryTypeStr, true, out var retryType);

            // Check if we've exceeded the maximum retry count
            if (context.ErrorContext.RetryAttempts >= maxRetryCount)
            {
                _logger.LogWarning("Maximum retry count {MaxRetryCount} exceeded for error {ErrorCode}", 
                    maxRetryCount, context.ErrorContext.ErrorCode);
                
                return RecoveryResult.Failed(
                    RecoveryAction.FAIL_BRANCH,
                    $"Maximum retry count {maxRetryCount} exceeded",
                    context,
                    context.ErrorContext);
            }

            // Calculate the next retry delay
            TimeSpan retryDelay = CalculateRetryDelay(
                context.ErrorContext.RetryAttempts, 
                baseRetryDelay, 
                backoffFactor, 
                retryType);

            // Update the recovery context
            context.ErrorContext.RetryAttempts++;
            context.ErrorContext.MaxRetryAttempts = maxRetryCount;
            
            // Add this attempt to the history
            context.History.Add(new RecoveryAttempt
            {
                AttemptNumber = context.ErrorContext.RetryAttempts,
                Timestamp = DateTime.UtcNow,
                StrategyName = Name,
                Success = true,
                ResultMessage = $"Retry scheduled with delay {retryDelay}"
            });

            // Return the result
            return new RecoveryResult
            {
                Success = true,
                Action = RecoveryAction.RETRY,
                Message = $"Retry scheduled with delay {retryDelay}",
                Context = context,
                NextRetryDelay = retryDelay
            };
        }

        private TimeSpan CalculateRetryDelay(int retryAttempt, int baseDelayMs, double backoffFactor, RetryType retryType)
        {
            return retryType switch
            {
                RetryType.Fixed => TimeSpan.FromMilliseconds(baseDelayMs),
                RetryType.Exponential => TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(backoffFactor, retryAttempt)),
                RetryType.Jittered => CalculateJitteredDelay(retryAttempt, baseDelayMs, backoffFactor),
                _ => TimeSpan.FromMilliseconds(baseDelayMs)
            };
        }

        private TimeSpan CalculateJitteredDelay(int retryAttempt, int baseDelayMs, double backoffFactor)
        {
            // Calculate the exponential delay
            double exponentialDelay = baseDelayMs * Math.Pow(backoffFactor, retryAttempt);
            
            // Add jitter (random value between 0 and exponentialDelay * 0.2)
            Random random = new Random();
            double jitter = random.NextDouble() * exponentialDelay * 0.2;
            
            return TimeSpan.FromMilliseconds(exponentialDelay + jitter);
        }
    }

    /// <summary>
    /// Defines the types of retry strategies.
    /// </summary>
    public enum RetryType
    {
        /// <summary>
        /// Fixed delay between retries.
        /// </summary>
        Fixed,

        /// <summary>
        /// Exponential backoff between retries.
        /// </summary>
        Exponential,

        /// <summary>
        /// Jittered exponential backoff between retries.
        /// </summary>
        Jittered
    }
}
