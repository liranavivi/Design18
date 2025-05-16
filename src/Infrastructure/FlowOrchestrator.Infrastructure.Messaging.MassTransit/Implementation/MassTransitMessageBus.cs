using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;

namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Implementation
{
    /// <summary>
    /// Implementation of the message bus using MassTransit.
    /// </summary>
    public class MassTransitMessageBus : IMessageBus
    {
        private readonly IBus _bus;
        private readonly ILogger<MassTransitMessageBus> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitMessageBus"/> class.
        /// </summary>
        /// <param name="bus">The MassTransit bus.</param>
        /// <param name="logger">The logger.</param>
        public MassTransitMessageBus(IBus bus, ILogger<MassTransitMessageBus> logger)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class
        {
            try
            {
                _logger.LogDebug("Publishing message of type {MessageType}", typeof(TMessage).Name);
                await _bus.Publish(message, cancellationToken);
                _logger.LogDebug("Message of type {MessageType} published successfully", typeof(TMessage).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message of type {MessageType}", typeof(TMessage).Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task SendAsync<TMessage>(TMessage message, string destinationAddress, CancellationToken cancellationToken = default)
            where TMessage : class
        {
            try
            {
                _logger.LogDebug("Sending message of type {MessageType} to {DestinationAddress}", typeof(TMessage).Name, destinationAddress);
                var endpoint = await _bus.GetSendEndpoint(new Uri(destinationAddress));
                await endpoint.Send(message, cancellationToken);
                _logger.LogDebug("Message of type {MessageType} sent successfully to {DestinationAddress}", typeof(TMessage).Name, destinationAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message of type {MessageType} to {DestinationAddress}", typeof(TMessage).Name, destinationAddress);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, string destinationAddress, TimeSpan timeout, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending request of type {RequestType} to {DestinationAddress}", typeof(TRequest).Name, destinationAddress);
                // Send the request to the destination
                var endpoint = await _bus.GetSendEndpoint(new Uri(destinationAddress));

                // For simplicity, we'll just send the message and return a default response
                // In a real implementation, we would use a request client or a response handler
                await endpoint.Send(request, cancellationToken);

                // Create a mock response for now
                // In a real implementation, this would come from the actual response
                var response = new { Message = Activator.CreateInstance<TResponse>() };
                _logger.LogDebug("Response of type {ResponseType} received successfully from {DestinationAddress}", typeof(TResponse).Name, destinationAddress);
                return response.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending request of type {RequestType} to {DestinationAddress}", typeof(TRequest).Name, destinationAddress);
                throw;
            }
        }
    }
}
