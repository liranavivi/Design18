using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Recovery.Models;

namespace FlowOrchestrator.Recovery.Strategies
{
    /// <summary>
    /// Defines the interface for a recovery strategy.
    /// </summary>
    public interface IRecoveryStrategy
    {
        /// <summary>
        /// Gets the name of the strategy.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the strategy.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Initializes the strategy with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration parameters.</param>
        void Initialize(Dictionary<string, object> configuration);

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <returns>The validation result.</returns>
        ValidationResult ValidateConfiguration(Dictionary<string, object> parameters);

        /// <summary>
        /// Determines whether this strategy is applicable to the specified error context.
        /// </summary>
        /// <param name="errorContext">The error context.</param>
        /// <returns><c>true</c> if this strategy is applicable; otherwise, <c>false</c>.</returns>
        bool IsApplicable(ErrorContext errorContext);

        /// <summary>
        /// Executes the recovery strategy.
        /// </summary>
        /// <param name="context">The recovery context.</param>
        /// <returns>The recovery result.</returns>
        RecoveryResult Recover(RecoveryContext context);
    }
}
