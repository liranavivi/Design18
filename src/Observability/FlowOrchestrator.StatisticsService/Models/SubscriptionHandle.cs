namespace FlowOrchestrator.Observability.Statistics.Models
{
    /// <summary>
    /// Represents a handle for a metric subscription.
    /// </summary>
    public class SubscriptionHandle
    {
        /// <summary>
        /// Gets or sets the subscription identifier.
        /// </summary>
        public string SubscriptionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the subscriber identifier.
        /// </summary>
        public string SubscriberId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the filter attributes.
        /// </summary>
        public Dictionary<string, string> FilterAttributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionHandle"/> class.
        /// </summary>
        public SubscriptionHandle()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionHandle"/> class with the specified metric name and subscriber identifier.
        /// </summary>
        /// <param name="metricName">The metric name.</param>
        /// <param name="subscriberId">The subscriber identifier.</param>
        public SubscriptionHandle(string metricName, string subscriberId)
        {
            MetricName = metricName;
            SubscriberId = subscriberId;
        }
    }

    /// <summary>
    /// Delegate for handling metric notifications.
    /// </summary>
    /// <param name="metricName">The metric name.</param>
    /// <param name="value">The metric value.</param>
    /// <param name="attributes">The metric attributes.</param>
    public delegate void NotificationHandler(string metricName, object value, Dictionary<string, string> attributes);
}
