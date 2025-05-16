using Hazelcast;
using Hazelcast.Core;
using Hazelcast.Models;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Cluster
{
    /// <summary>
    /// Manages Hazelcast cluster operations.
    /// </summary>
    public class ClusterManager
    {
        private readonly HazelcastContext _context;
        private readonly ILogger<ClusterManager> _logger;
        private IHazelcastClient? _client;
        private Guid? _subscriptionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterManager"/> class.
        /// </summary>
        /// <param name="context">The Hazelcast context.</param>
        /// <param name="logger">The logger.</param>
        public ClusterManager(HazelcastContext context, ILogger<ClusterManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Initializes the cluster manager.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            _client = await _context.GetClientAsync();

            // Subscribe to client state changes
            _subscriptionId = await _client.SubscribeAsync(events => events
                .StateChanged((sender, args) =>
                {
                    _logger.LogInformation("Hazelcast client state changed to: {State}", args.State);
                })
                .MembersUpdated((sender, args) =>
                {
                    _logger.LogInformation("Hazelcast cluster members updated. Added: {Added}, Removed: {Removed}",
                        args.AddedMembers.Count, args.RemovedMembers.Count);

                    foreach (var member in args.AddedMembers)
                    {
                        _logger.LogDebug("Member added: {Member}", member);
                    }

                    foreach (var member in args.RemovedMembers)
                    {
                        _logger.LogDebug("Member removed: {Member}", member);
                    }
                })
                .PartitionsUpdated((sender, args) =>
                {
                    _logger.LogDebug("Hazelcast partitions updated.");
                })
            );

            _logger.LogInformation("Cluster manager initialized");
        }

        /// <summary>
        /// Gets the cluster members.
        /// </summary>
        /// <returns>The cluster members.</returns>
        public async Task<IReadOnlyCollection<MemberInfoState>> GetClusterMembersAsync()
        {
            if (_client == null)
            {
                _client = await _context.GetClientAsync();
            }

            return _client.Members;
        }

        /// <summary>
        /// Gets the cluster statistics.
        /// </summary>
        /// <returns>The cluster statistics.</returns>
        public async Task<Dictionary<string, object>> GetClusterStatisticsAsync()
        {
            if (_client == null)
            {
                _client = await _context.GetClientAsync();
            }

            var stats = new Dictionary<string, object>();

            // Add basic client statistics
            stats["ClientName"] = _client.Name;
            stats["ClientId"] = _client.Id.ToString();
            stats["ClusterName"] = _client.ClusterName;

            // Add member count
            stats["ClusterSize"] = _client.Members.Count;

            // Add connection status
            stats["ConnectionState"] = _client.State.ToString();

            return stats;
        }

        /// <summary>
        /// Checks if the cluster is healthy.
        /// </summary>
        /// <returns>true if the cluster is healthy; otherwise, false.</returns>
        public async Task<bool> IsClusterHealthyAsync()
        {
            if (_client == null)
            {
                try
                {
                    _client = await _context.GetClientAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to Hazelcast cluster");
                    return false;
                }
            }

            return _client.State == ClientState.Connected && _client.Members.Count > 0;
        }

        /// <summary>
        /// Disposes the cluster manager.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DisposeAsync()
        {
            if (_client != null && _subscriptionId.HasValue)
            {
                await _client.UnsubscribeAsync(_subscriptionId.Value);
                _subscriptionId = null;
            }

            _logger.LogInformation("Cluster manager disposed");
        }
    }
}
