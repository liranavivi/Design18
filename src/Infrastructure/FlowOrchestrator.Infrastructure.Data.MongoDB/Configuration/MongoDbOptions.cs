namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration
{
    /// <summary>
    /// Configuration options for MongoDB.
    /// </summary>
    public class MongoDbOptions
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string DatabaseName { get; set; } = "FlowOrchestrator";

        /// <summary>
        /// Gets or sets the maximum connection pool size.
        /// </summary>
        public int MaxConnectionPoolSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the socket timeout in seconds.
        /// </summary>
        public int SocketTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the server selection timeout in seconds.
        /// </summary>
        public int ServerSelectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL.
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to retry writes.
        /// </summary>
        public bool RetryWrites { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to retry reads.
        /// </summary>
        public bool RetryReads { get; set; } = true;
    }
}
