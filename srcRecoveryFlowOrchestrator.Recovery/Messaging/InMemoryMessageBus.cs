using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Recovery.Messaging
{
    /// <summary>
    /// In-memory implementation of the message bus for testing purposes.
    /// </summary>
    public class InMemoryMessageBus : IMessageBus
    {
        private readonly ILogger<InMemoryMessageBus> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMessageBus"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public InMemoryMessageBus(ILogger<InMemoryMessageBus> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class
        {
            _logger.LogInformation("Publishing message of type {MessageType}", typeof(TMessage).Name);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SendAsync<TMessage>(TMessage message, string destinationAddress, CancellationToken cancellationToken = default)
            where TMessage : class
        {
            _logger.LogInformation("Sending message of type {MessageType} to {DestinationAddress}", typeof(TMessage).Name, destinationAddress);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogInformation("Requesting response for message of type {RequestType}", typeof(TRequest).Name);
            return Task.FromResult(Activator.CreateInstance<TResponse>());
        }

        /// <inheritdoc/>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, TimeSpan timeout, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogInformation("Requesting response for message of type {RequestType} with timeout {Timeout}", typeof(TRequest).Name, timeout);
            return Task.FromResult(Activator.CreateInstance<TResponse>());
        }

        /// <inheritdoc/>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, string destinationAddress, TimeSpan timeout, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogInformation("Requesting response for message of type {RequestType} to {DestinationAddress} with timeout {Timeout}", typeof(TRequest).Name, destinationAddress, timeout);
            return Task.FromResult(Activator.CreateInstance<TResponse>());
        }
    }
}
