namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Configuration
{
    /// <summary>
    /// Configuration options for Hazelcast.
    /// </summary>
    public class HazelcastOptions
    {
        /// <summary>
        /// Gets or sets the cluster name.
        /// </summary>
        public string ClusterName { get; set; } = "dev";

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the network addresses of the Hazelcast cluster members.
        /// Format: "hostname:port" or "ip:port"
        /// </summary>
        public List<string> NetworkAddresses { get; set; } = new List<string> { "localhost:5701" };

        /// <summary>
        /// Gets or sets the retry count for initial connection.
        /// </summary>
        public int ConnectionRetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry delay in milliseconds for initial connection.
        /// </summary>
        public int ConnectionRetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether to use smart routing.
        /// </summary>
        public bool SmartRouting { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL.
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Gets or sets the SSL certificate path.
        /// </summary>
        public string? SslCertificatePath { get; set; }

        /// <summary>
        /// Gets or sets the SSL certificate password.
        /// </summary>
        public string? SslCertificatePassword { get; set; }

        /// <summary>
        /// Gets or sets the maximum connection idle time in seconds.
        /// </summary>
        public int MaxConnectionIdleTimeSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the client name.
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// Gets or sets the maximum concurrent invocations per connection.
        /// </summary>
        public int MaxConcurrentInvocations { get; set; } = 100;

        /// <summary>
        /// Gets or sets the invocation timeout in seconds.
        /// </summary>
        public int InvocationTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Gets or sets the statistics collection period in seconds.
        /// </summary>
        public int StatisticsPeriodSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets a value indicating whether to enable statistics collection.
        /// </summary>
        public bool StatisticsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the near cache configuration.
        /// </summary>
        public Dictionary<string, NearCacheOptions> NearCaches { get; set; } = new Dictionary<string, NearCacheOptions>();
    }

    /// <summary>
    /// Configuration options for near cache.
    /// </summary>
    public class NearCacheOptions
    {
        /// <summary>
        /// Gets or sets the maximum size of the near cache.
        /// </summary>
        public int MaxSize { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the time-to-live in seconds.
        /// </summary>
        public int TimeToLiveSeconds { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum idle time in seconds.
        /// </summary>
        public int MaxIdleSeconds { get; set; } = 0;

        /// <summary>
        /// Gets or sets the eviction policy.
        /// </summary>
        public string EvictionPolicy { get; set; } = "LRU";

        /// <summary>
        /// Gets or sets a value indicating whether to invalidate on change.
        /// </summary>
        public bool InvalidateOnChange { get; set; } = true;
    }
}
