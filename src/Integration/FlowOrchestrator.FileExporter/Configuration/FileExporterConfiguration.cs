namespace FlowOrchestrator.Integration.Exporters.File.Configuration
{
    /// <summary>
    /// Configuration for the file exporter service.
    /// </summary>
    public class FileExporterConfiguration
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = "FILE-EXPORTER-001";

        /// <summary>
        /// Gets or sets the service type.
        /// </summary>
        public string ServiceType { get; set; } = "FileExporter";

        /// <summary>
        /// Gets or sets the protocol.
        /// </summary>
        public string Protocol { get; set; } = "file";

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the operation timeout in seconds.
        /// </summary>
        public int OperationTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the maximum retry count.
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry delay in milliseconds.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether to use exponential backoff for retries.
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        /// Gets or sets the batch size.
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether to validate data before exporting.
        /// </summary>
        public bool ValidateData { get; set; } = true;

        /// <summary>
        /// Gets or sets the schema identifier.
        /// </summary>
        public string? SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the schema version.
        /// </summary>
        public string? SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the merge strategy.
        /// </summary>
        public string MergeStrategy { get; set; } = "append";

        /// <summary>
        /// Gets or sets the file configuration.
        /// </summary>
        public FileConfiguration File { get; set; } = new FileConfiguration();
    }

    /// <summary>
    /// Configuration for file operations.
    /// </summary>
    public class FileConfiguration
    {
        /// <summary>
        /// Gets or sets the base path for file operations.
        /// </summary>
        public string BasePath { get; set; } = "C:\\Data\\Export";

        /// <summary>
        /// Gets or sets the file pattern for file operations.
        /// </summary>
        public string FilePattern { get; set; } = "*.*";

        /// <summary>
        /// Gets or sets a value indicating whether to search subdirectories recursively.
        /// </summary>
        public bool Recursive { get; set; } = false;

        /// <summary>
        /// Gets or sets the encoding for text files.
        /// </summary>
        public string Encoding { get; set; } = "utf-8";

        /// <summary>
        /// Gets or sets the buffer size for file operations.
        /// </summary>
        public int BufferSize { get; set; } = 4096;
    }
}
