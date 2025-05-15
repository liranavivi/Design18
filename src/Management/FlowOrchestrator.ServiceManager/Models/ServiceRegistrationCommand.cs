using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Management.Services
{
    /// <summary>
    /// Represents a command to register a service.
    /// </summary>
    /// <typeparam name="TService">The type of service to register.</typeparam>
    public class ServiceRegistrationCommand<TService>
        where TService : IService
    {
        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the service to register.
        /// </summary>
        public TService Service { get; set; } = default!;
    }
}
