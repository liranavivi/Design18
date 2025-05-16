namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Configuration
{
    /// <summary>
    /// Options for configuring the message bus.
    /// </summary>
    public class MessageBusOptions
    {
        /// <summary>
        /// Gets or sets the transport type.
        /// </summary>
        public TransportType TransportType { get; set; } = TransportType.InMemory;

        /// <summary>
        /// Gets or sets the host address.
        /// </summary>
        public string? HostAddress { get; set; }

        /// <summary>
        /// Gets or sets the username for authentication.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the password for authentication.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the virtual host.
        /// </summary>
        public string? VirtualHost { get; set; }

        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry interval in seconds.
        /// </summary>
        public int RetryIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the prefetch count.
        /// </summary>
        public int PrefetchCount { get; set; } = 16;

        /// <summary>
        /// Gets or sets the concurrency limit.
        /// </summary>
        public int ConcurrencyLimit { get; set; } = 10;
    }

    /// <summary>
    /// Defines the transport types for the message bus.
    /// </summary>
    public enum TransportType
    {
        /// <summary>
        /// In-memory transport.
        /// </summary>
        InMemory,

        /// <summary>
        /// RabbitMQ transport.
        /// </summary>
        RabbitMq,

        /// <summary>
        /// Azure Service Bus transport.
        /// </summary>
        AzureServiceBus
    }
}
