using FlowOrchestrator.Abstractions.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters
{
    /// <summary>
    /// Adapter for converting FlowOrchestrator's IMessageConsumer to MassTransit's IConsumer.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    public class ConsumerAdapter<TMessage> : global::MassTransit.IConsumer<TMessage>
        where TMessage : class
    {
        private readonly IMessageConsumer<TMessage> _consumer;
        private readonly ILogger<ConsumerAdapter<TMessage>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumerAdapter{TMessage}"/> class.
        /// </summary>
        /// <param name="consumer">The FlowOrchestrator message consumer.</param>
        /// <param name="logger">The logger.</param>
        public ConsumerAdapter(IMessageConsumer<TMessage> consumer, ILogger<ConsumerAdapter<TMessage>> logger)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Consumes a message.
        /// </summary>
        /// <param name="context">The MassTransit consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(global::MassTransit.ConsumeContext<TMessage> context)
        {
            try
            {
                _logger.LogDebug("Adapting MassTransit ConsumeContext to FlowOrchestrator ConsumeContext for message type {MessageType}", typeof(TMessage).Name);
                var adaptedContext = ConsumeContextAdapter.Adapt(context);
                await _consumer.Consume(adaptedContext);
                _logger.LogDebug("Message of type {MessageType} consumed successfully", typeof(TMessage).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming message of type {MessageType}", typeof(TMessage).Name);
                throw;
            }
        }
    }
}
