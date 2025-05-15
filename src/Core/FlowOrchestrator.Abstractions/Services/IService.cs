using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Defines the base interface for all services in the FlowOrchestrator system.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Gets the unique identifier for the service.
        /// </summary>
        string ServiceId { get; }

        /// <summary>
        /// Gets the version of the service.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        string ServiceType { get; }

        /// <summary>
        /// Initializes the service with the specified configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        void Initialize(ConfigurationParameters parameters);

        /// <summary>
        /// Terminates the service.
        /// </summary>
        void Terminate();

        /// <summary>
        /// Gets the current state of the service.
        /// </summary>
        /// <returns>The current service state.</returns>
        ServiceState GetState();
    }
}
