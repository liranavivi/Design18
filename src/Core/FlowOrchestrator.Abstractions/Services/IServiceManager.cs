using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Defines the interface for service managers in the FlowOrchestrator system.
    /// </summary>
    /// <typeparam name="TService">The type of service being managed.</typeparam>
    /// <typeparam name="TServiceId">The type of service identifier.</typeparam>
    public interface IServiceManager<TService, TServiceId> : IService
        where TService : IService
    {
        /// <summary>
        /// Registers a service with the manager.
        /// </summary>
        /// <param name="service">The service to register.</param>
        /// <returns>The result of the registration.</returns>
        ServiceRegistrationResult RegisterService(TService service);

        /// <summary>
        /// Unregisters a service from the manager.
        /// </summary>
        /// <param name="serviceId">The identifier of the service to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        ServiceUnregistrationResult UnregisterService(TServiceId serviceId);

        /// <summary>
        /// Gets a service by its identifier.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The service if found; otherwise, null.</returns>
        TService? GetService(TServiceId serviceId);

        /// <summary>
        /// Gets all registered services.
        /// </summary>
        /// <returns>The collection of registered services.</returns>
        IEnumerable<TService> GetAllServices();

        /// <summary>
        /// Gets services by type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The collection of services of the specified type.</returns>
        IEnumerable<TService> GetServicesByType(string serviceType);

        /// <summary>
        /// Gets services by version.
        /// </summary>
        /// <param name="version">The service version.</param>
        /// <returns>The collection of services with the specified version.</returns>
        IEnumerable<TService> GetServicesByVersion(string version);

        /// <summary>
        /// Checks if a service with the specified identifier exists.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>true if a service with the specified identifier exists; otherwise, false.</returns>
        bool ServiceExists(TServiceId serviceId);

        /// <summary>
        /// Gets the count of registered services.
        /// </summary>
        /// <returns>The count of registered services.</returns>
        int GetServiceCount();
    }

    /// <summary>
    /// Represents the result of a service registration operation.
    /// </summary>
    public class ServiceRegistrationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the registration was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message if the registration failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error details if the registration failed.
        /// </summary>
        public Dictionary<string, object>? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Represents the result of a service unregistration operation.
    /// </summary>
    public class ServiceUnregistrationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the unregistration was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message if the unregistration failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error details if the unregistration failed.
        /// </summary>
        public Dictionary<string, object>? ErrorDetails { get; set; }
    }
}
