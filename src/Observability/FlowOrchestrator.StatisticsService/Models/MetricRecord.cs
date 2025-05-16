namespace FlowOrchestrator.Observability.Statistics.Models
{
    /// <summary>
    /// Represents a metric record.
    /// </summary>
    public class MetricRecord
    {
        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric value.
        /// </summary>
        public object Value { get; set; } = 0;

        /// <summary>
        /// Gets or sets the metric unit.
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the component identifier.
        /// </summary>
        public string ComponentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        public string ComponentType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the context attributes.
        /// </summary>
        public Dictionary<string, string> ContextAttributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricRecord"/> class.
        /// </summary>
        public MetricRecord()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricRecord"/> class with the specified name and value.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The metric value.</param>
        public MetricRecord(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricRecord"/> class with the specified name, value, and component information.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="componentType">The component type.</param>
        public MetricRecord(string name, object value, string componentId, string componentType)
        {
            Name = name;
            Value = value;
            ComponentId = componentId;
            ComponentType = componentType;
        }
    }
}
