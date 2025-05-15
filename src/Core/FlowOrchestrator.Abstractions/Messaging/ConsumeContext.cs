namespace FlowOrchestrator.Abstractions.Messaging
{
    /// <summary>
    /// Represents the context for consuming a message.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    public class ConsumeContext<TMessage> where TMessage : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumeContext{TMessage}"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public ConsumeContext(TMessage message, string messageId, string correlationId)
        {
            Message = message;
            MessageId = messageId;
            CorrelationId = correlationId;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public TMessage Message { get; }

        /// <summary>
        /// Gets the message identifier.
        /// </summary>
        public string MessageId { get; }

        /// <summary>
        /// Gets the correlation identifier.
        /// </summary>
        public string CorrelationId { get; }
    }
}
