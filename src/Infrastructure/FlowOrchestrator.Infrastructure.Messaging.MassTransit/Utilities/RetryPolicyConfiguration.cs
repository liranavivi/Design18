using MassTransit;
using MassTransit.RetryPolicies;
using System;

namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Utilities
{
    /// <summary>
    /// Configuration for retry policies.
    /// </summary>
    public static class RetryPolicyConfiguration
    {
        /// <summary>
        /// Configures an exponential retry policy.
        /// </summary>
        /// <param name="configurator">The retry configurator.</param>
        /// <param name="retryLimit">The retry limit.</param>
        /// <param name="initialInterval">The initial interval.</param>
        /// <param name="intervalIncrement">The interval increment.</param>
        public static void ConfigureExponentialRetry(
            IRetryConfigurator configurator,
            int retryLimit = 3,
            TimeSpan? initialInterval = null,
            TimeSpan? intervalIncrement = null)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.Exponential(
                retryLimit,
                initialInterval ?? TimeSpan.FromSeconds(1),
                intervalIncrement ?? TimeSpan.FromSeconds(2),
                TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Configures an incremental retry policy.
        /// </summary>
        /// <param name="configurator">The retry configurator.</param>
        /// <param name="retryLimit">The retry limit.</param>
        /// <param name="initialInterval">The initial interval.</param>
        /// <param name="intervalIncrement">The interval increment.</param>
        public static void ConfigureIncrementalRetry(
            IRetryConfigurator configurator,
            int retryLimit = 3,
            TimeSpan? initialInterval = null,
            TimeSpan? intervalIncrement = null)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.Incremental(
                retryLimit,
                initialInterval ?? TimeSpan.FromSeconds(1),
                intervalIncrement ?? TimeSpan.FromSeconds(2));
        }

        /// <summary>
        /// Configures an interval retry policy.
        /// </summary>
        /// <param name="configurator">The retry configurator.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="interval">The interval.</param>
        public static void ConfigureIntervalRetry(
            IRetryConfigurator configurator,
            int retryCount = 3,
            TimeSpan? interval = null)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.Interval(retryCount, interval ?? TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Creates a retry policy for specific exception types.
        /// </summary>
        /// <param name="configurator">The retry configurator.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="exceptionTypes">The exception types to retry on.</param>
        public static void ConfigureRetryForExceptions(
            IRetryConfigurator configurator,
            int retryCount = 3,
            TimeSpan? interval = null,
            params Type[] exceptionTypes)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            if (exceptionTypes == null || exceptionTypes.Length == 0)
            {
                throw new ArgumentException("At least one exception type must be specified", nameof(exceptionTypes));
            }

            // MassTransit doesn't support specifying exception types directly in the Interval method
            // Use a filter instead
            configurator.Interval(retryCount, interval ?? TimeSpan.FromSeconds(5));

            // Add exception filters if needed
            foreach (var exceptionType in exceptionTypes)
            {
                configurator.Handle(exceptionType);
            }
        }
    }
}
