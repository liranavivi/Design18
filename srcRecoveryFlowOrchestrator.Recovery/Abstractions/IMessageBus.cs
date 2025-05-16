namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions
{
    /// <summary>
    /// Defines the interface for a message bus in the FlowOrchestrator system.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Publishes a message to all subscribed consumers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to publish.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Sends a message to a specific endpoint.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="destinationAddress">The destination address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendAsync<TMessage>(TMessage message, string destinationAddress, CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Sends a request and waits for a response.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class;

        /// <summary>
        /// Sends a request and waits for a response with a timeout.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, TimeSpan timeout, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class;

        /// <summary>
        /// Sends a request to a specific endpoint and waits for a response with a timeout.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="destinationAddress">The destination address.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, string destinationAddress, TimeSpan timeout, CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class;
    }
}
