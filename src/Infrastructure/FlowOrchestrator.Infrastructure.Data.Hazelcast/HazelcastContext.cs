using FlowOrchestrator.Infrastructure.Data.Hazelcast.Configuration;
using Hazelcast;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HazelcastOptions = FlowOrchestrator.Infrastructure.Data.Hazelcast.Configuration.HazelcastOptions;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast
{
    /// <summary>
    /// Provides a context for Hazelcast operations.
    /// </summary>
    public class HazelcastContext : IAsyncDisposable
    {
        private readonly ILogger<HazelcastContext> _logger;
        private readonly HazelcastOptions _options;
        private IHazelcastClient? _client;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastContext"/> class.
        /// </summary>
        /// <param name="options">The Hazelcast options.</param>
        /// <param name="logger">The logger.</param>
        public HazelcastContext(IOptions<HazelcastOptions> options, ILogger<HazelcastContext> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gets the Hazelcast client.
        /// </summary>
        /// <returns>The Hazelcast client.</returns>
        public async Task<IHazelcastClient> GetClientAsync()
        {
            if (_client != null)
                return _client;

            _logger.LogInformation("Initializing Hazelcast client with cluster name: {ClusterName}", _options.ClusterName);

            var optionsBuilder = new HazelcastOptionsBuilder()
                .With(options =>
                {
                    options.ClusterName = _options.ClusterName;

                    // Configure networking
                    options.Networking.Addresses.Clear();
                    foreach (var address in _options.NetworkAddresses)
                    {
                        options.Networking.Addresses.Add(address);
                    }

                    // Set connection retry settings
                    options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = _options.ConnectionRetryCount * _options.ConnectionRetryDelayMs;
                    options.Networking.ConnectionRetry.InitialBackoffMilliseconds = _options.ConnectionRetryDelayMs;
                    options.Networking.SmartRouting = _options.SmartRouting;

                    // Configure SSL if enabled
                    if (_options.UseSsl)
                    {
                        options.Networking.Ssl.Enabled = true;
                        if (!string.IsNullOrEmpty(_options.SslCertificatePath))
                        {
                            options.Networking.Ssl.CertificatePath = _options.SslCertificatePath;
                            options.Networking.Ssl.CertificatePassword = _options.SslCertificatePassword;
                        }
                    }

                    // Configure client name if provided
                    if (!string.IsNullOrEmpty(_options.ClientName))
                    {
                        options.ClientName = _options.ClientName;
                    }
                });

            _client = await HazelcastClientFactory.StartNewClientAsync(optionsBuilder.Build());
            _logger.LogInformation("Hazelcast client initialized successfully");

            return _client;
        }

        /// <summary>
        /// Disposes the Hazelcast context.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            if (_client != null)
            {
                _logger.LogInformation("Disposing Hazelcast client");
                await _client.DisposeAsync();
                _client = null;
            }

            _disposed = true;
        }
    }
}
