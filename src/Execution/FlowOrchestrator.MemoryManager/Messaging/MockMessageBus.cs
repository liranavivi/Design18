using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.MemoryManager.Messaging
{
    /// <summary>
    /// A mock implementation of the IMessageBus interface for testing purposes.
    /// </summary>
    public class MockMessageBus : IMessageBus
    {
        private readonly ILogger<MockMessageBus> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockMessageBus"/> class.
        /// </summary>
        public MockMessageBus()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<MockMessageBus>();
        }

        /// <inheritdoc/>
        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogInformation("Publishing message of type {MessageType}: {Message}", typeof(T).Name, message);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SendAsync<T>(T message, string destination, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogInformation("Sending message of type {MessageType} to {Destination}: {Message}", typeof(T).Name, destination, message);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogInformation("Requesting response for message of type {RequestType}: {Request}", typeof(TRequest).Name, request);
            return Task.FromResult<TResponse>(null!);
        }

        /// <inheritdoc/>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, TimeSpan timeout, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogInformation("Requesting response for message of type {RequestType} with timeout {Timeout}: {Request}", typeof(TRequest).Name, timeout, request);
            return Task.FromResult<TResponse>(null!);
        }

        /// <inheritdoc/>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, string destination, TimeSpan timeout, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogInformation("Requesting response for message of type {RequestType} to {Destination} with timeout {Timeout}: {Request}",
                typeof(TRequest).Name, destination, timeout, request);
            return Task.FromResult<TResponse>(null!);
        }
    }
}
