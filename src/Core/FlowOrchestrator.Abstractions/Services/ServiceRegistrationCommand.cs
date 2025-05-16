namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Represents a command to register a service.
    /// </summary>
    /// <typeparam name="TService">The type of service to register.</typeparam>
    public class ServiceRegistrationCommand<TService>
        where TService : IService
    {
        /// <summary>
        /// Gets or sets the service to register.
        /// </summary>
        public required TService Service { get; set; }
    }
}
