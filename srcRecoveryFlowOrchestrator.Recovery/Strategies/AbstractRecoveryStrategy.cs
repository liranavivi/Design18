using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Recovery.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Recovery.Strategies
{
    /// <summary>
    /// Abstract base class for recovery strategies.
    /// </summary>
    public abstract class AbstractRecoveryStrategy : IRecoveryStrategy
    {
        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// The configuration parameters.
        /// </summary>
        protected Dictionary<string, object> _configuration = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractRecoveryStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected AbstractRecoveryStrategy(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the name of the strategy.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the description of the strategy.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Initializes the strategy with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration parameters.</param>
        public virtual void Initialize(Dictionary<string, object> configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger.LogInformation("Initialized {StrategyName} recovery strategy with {ParameterCount} parameters", 
                Name, configuration.Count);
        }

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <returns>The validation result.</returns>
        public abstract ValidationResult ValidateConfiguration(Dictionary<string, object> parameters);

        /// <summary>
        /// Determines whether this strategy is applicable to the specified error context.
        /// </summary>
        /// <param name="errorContext">The error context.</param>
        /// <returns><c>true</c> if this strategy is applicable; otherwise, <c>false</c>.</returns>
        public abstract bool IsApplicable(ErrorContext errorContext);

        /// <summary>
        /// Executes the recovery strategy.
        /// </summary>
        /// <param name="context">The recovery context.</param>
        /// <returns>The recovery result.</returns>
        public abstract RecoveryResult Recover(RecoveryContext context);

        /// <summary>
        /// Gets a configuration value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The configuration value.</returns>
        protected T GetConfigValue<T>(string key, T defaultValue)
        {
            if (_configuration.TryGetValue(key, out var value))
            {
                try
                {
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }

                    // Try to convert the value
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to convert configuration value for key {Key} to type {Type}", 
                        key, typeof(T).Name);
                }
            }

            return defaultValue;
        }
    }
}
