namespace FlowOrchestrator.Abstractions.Messaging
{
    /// <summary>
    /// Defines the interface for message consumers in the FlowOrchestrator system.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to consume.</typeparam>
    public interface IMessageConsumer<TMessage>
        where TMessage : class
    {
        /// <summary>
        /// Consumes a message.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Consume(ConsumeContext<TMessage> context);
    }
}
