using FlowOrchestrator.Abstractions.Messaging;

namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters
{
    /// <summary>
    /// Adapter for converting MassTransit's ConsumeContext to FlowOrchestrator's ConsumeContext.
    /// </summary>
    public static class ConsumeContextAdapter
    {
        /// <summary>
        /// Adapts a MassTransit ConsumeContext to a FlowOrchestrator ConsumeContext.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="context">The MassTransit ConsumeContext.</param>
        /// <returns>A FlowOrchestrator ConsumeContext.</returns>
        public static ConsumeContext<TMessage> Adapt<TMessage>(global::MassTransit.ConsumeContext<TMessage> context)
            where TMessage : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new ConsumeContext<TMessage>(
                context.Message,
                context.MessageId?.ToString() ?? Guid.NewGuid().ToString(),
                context.CorrelationId?.ToString() ?? Guid.NewGuid().ToString());
        }
    }
}
