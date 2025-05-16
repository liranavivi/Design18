namespace FlowOrchestrator.Observability.Statistics.Models
{
    /// <summary>
    /// Represents a historical data record.
    /// </summary>
    public class HistoricalDataRecord
    {
        /// <summary>
        /// Gets or sets the record identifier.
        /// </summary>
        public string RecordId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric value.
        /// </summary>
        public object Value { get; set; } = 0;

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
        /// Gets or sets the retention policy.
        /// </summary>
        public RetentionPolicy RetentionPolicy { get; set; } = RetentionPolicy.Standard;

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(30);

        /// <summary>
        /// Gets or sets a value indicating whether the record is archived.
        /// </summary>
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Gets or sets the archive date.
        /// </summary>
        public DateTime? ArchiveDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalDataRecord"/> class.
        /// </summary>
        public HistoricalDataRecord()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalDataRecord"/> class from a metric record.
        /// </summary>
        /// <param name="metricRecord">The metric record.</param>
        /// <param name="retentionPolicy">The retention policy.</param>
        public HistoricalDataRecord(MetricRecord metricRecord, RetentionPolicy retentionPolicy = RetentionPolicy.Standard)
        {
            MetricName = metricRecord.Name;
            Value = metricRecord.Value;
            Timestamp = metricRecord.Timestamp;
            ComponentId = metricRecord.ComponentId;
            ComponentType = metricRecord.ComponentType;
            ContextAttributes = new Dictionary<string, string>(metricRecord.ContextAttributes);
            RetentionPolicy = retentionPolicy;
            ExpirationDate = CalculateExpirationDate(retentionPolicy);
        }

        private DateTime CalculateExpirationDate(RetentionPolicy policy)
        {
            return policy switch
            {
                RetentionPolicy.ShortTerm => DateTime.UtcNow.AddDays(1),
                RetentionPolicy.Standard => DateTime.UtcNow.AddDays(30),
                RetentionPolicy.LongTerm => DateTime.UtcNow.AddDays(90),
                RetentionPolicy.Archive => DateTime.UtcNow.AddDays(365),
                _ => DateTime.UtcNow.AddDays(30)
            };
        }
    }
}
