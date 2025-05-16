using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Common.Exceptions;
using FlowOrchestrator.Recovery.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Recovery.Strategies
{
    /// <summary>
    /// A recovery strategy that implements the fallback pattern.
    /// </summary>
    public class FallbackRecoveryStrategy : AbstractRecoveryStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackRecoveryStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FallbackRecoveryStrategy(ILogger<FallbackRecoveryStrategy> logger) : base(logger)
        {
        }

        /// <summary>
        /// Gets the name of the strategy.
        /// </summary>
        public override string Name => "FallbackStrategy";

        /// <summary>
        /// Gets the description of the strategy.
        /// </summary>
        public override string Description => "Uses alternative processing paths when primary paths fail";

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateConfiguration(Dictionary<string, object> parameters)
        {
            var errors = new List<string>();

            // Validate FallbackType
            if (parameters.TryGetValue("FallbackType", out var fallbackTypeObj))
            {
                if (fallbackTypeObj is not string fallbackType || 
                    !Enum.TryParse<FallbackType>(fallbackType, true, out _))
                {
                    errors.Add("FallbackType must be one of: CachedResult, AlternativePath, GracefulDegradation, MessageQueue, ManualIntervention");
                }
            }

            // Validate FallbackServiceId
            if (parameters.TryGetValue("FallbackType", out var typeObj) && 
                typeObj is string type && 
                type.Equals("AlternativePath", StringComparison.OrdinalIgnoreCase))
            {
                if (!parameters.TryGetValue("FallbackServiceId", out var serviceIdObj) || 
                    serviceIdObj is not string serviceId || 
                    string.IsNullOrWhiteSpace(serviceId))
                {
                    errors.Add("FallbackServiceId is required when FallbackType is AlternativePath");
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
            // Check if the error is suitable for fallback
            bool isFallbackable = errorContext.Classification switch
            {
                ErrorClassification.CONNECTION_ERROR => true,
                ErrorClassification.CONNECTION_TIMEOUT => true,
                ErrorClassification.CONNECTION_UNREACHABLE => true,
                ErrorClassification.RESOURCE_UNAVAILABLE => true,
                ErrorClassification.RESOURCE_NOT_FOUND => true,
                _ => false
            };

            // Check if we have a fallback configured
            string fallbackType = GetConfigValue("FallbackType", string.Empty);
            bool hasFallback = !string.IsNullOrEmpty(fallbackType);

            return isFallbackable && hasFallback;
        }

        /// <summary>
        /// Executes the recovery strategy.
        /// </summary>
        /// <param name="context">The recovery context.</param>
        /// <returns>The recovery result.</returns>
        public override RecoveryResult Recover(RecoveryContext context)
        {
            _logger.LogInformation("Executing fallback recovery strategy for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // Get the fallback type
            string fallbackTypeStr = GetConfigValue("FallbackType", "GracefulDegradation");
            Enum.TryParse<FallbackType>(fallbackTypeStr, true, out var fallbackType);

            // Execute the appropriate fallback strategy
            switch (fallbackType)
            {
                case FallbackType.CachedResult:
                    return HandleCachedResultFallback(context);
                
                case FallbackType.AlternativePath:
                    return HandleAlternativePathFallback(context);
                
                case FallbackType.GracefulDegradation:
                    return HandleGracefulDegradationFallback(context);
                
                case FallbackType.MessageQueue:
                    return HandleMessageQueueFallback(context);
                
                case FallbackType.ManualIntervention:
                    return HandleManualInterventionFallback(context);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private RecoveryResult HandleCachedResultFallback(RecoveryContext context)
        {
            _logger.LogInformation("Using cached result fallback for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // In a real implementation, we would retrieve the cached result
            // For now, we'll just return a successful result
            return RecoveryResult.Successful(
                RecoveryAction.USE_FALLBACK,
                "Using cached result fallback",
                context);
        }

        private RecoveryResult HandleAlternativePathFallback(RecoveryContext context)
        {
            string fallbackServiceId = GetConfigValue("FallbackServiceId", string.Empty);
            
            _logger.LogInformation("Using alternative path fallback to service {FallbackServiceId} for error {ErrorCode}", 
                fallbackServiceId, context.ErrorContext.ErrorCode);

            // In a real implementation, we would route to the alternative service
            // For now, we'll just return a successful result
            return RecoveryResult.Successful(
                RecoveryAction.USE_FALLBACK,
                $"Using alternative path fallback to service {fallbackServiceId}",
                context);
        }

        private RecoveryResult HandleGracefulDegradationFallback(RecoveryContext context)
        {
            _logger.LogInformation("Using graceful degradation fallback for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // In a real implementation, we would provide reduced functionality
            // For now, we'll just return a successful result
            return RecoveryResult.Successful(
                RecoveryAction.CONTINUE_DEGRADED,
                "Using graceful degradation fallback",
                context);
        }

        private RecoveryResult HandleMessageQueueFallback(RecoveryContext context)
        {
            _logger.LogInformation("Using message queue fallback for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // In a real implementation, we would queue the operation for later retry
            // For now, we'll just return a successful result
            return RecoveryResult.Successful(
                RecoveryAction.USE_FALLBACK,
                "Using message queue fallback",
                context);
        }

        private RecoveryResult HandleManualInterventionFallback(RecoveryContext context)
        {
            _logger.LogInformation("Using manual intervention fallback for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // In a real implementation, we would request manual intervention
            // For now, we'll just return a successful result
            return RecoveryResult.Successful(
                RecoveryAction.USE_FALLBACK,
                "Using manual intervention fallback",
                context);
        }
    }

    /// <summary>
    /// Defines the types of fallback strategies.
    /// </summary>
    public enum FallbackType
    {
        /// <summary>
        /// Use cached results when operation fails.
        /// </summary>
        CachedResult,

        /// <summary>
        /// Use alternative processing path.
        /// </summary>
        AlternativePath,

        /// <summary>
        /// Provide reduced functionality.
        /// </summary>
        GracefulDegradation,

        /// <summary>
        /// Queue operation for later retry.
        /// </summary>
        MessageQueue,

        /// <summary>
        /// Request manual intervention.
        /// </summary>
        ManualIntervention
    }
}
