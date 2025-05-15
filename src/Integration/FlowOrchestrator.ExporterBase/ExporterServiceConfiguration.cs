using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Common.Configuration;
using FlowOrchestrator.Common.Validation;
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Integration.Exporters
{
    /// <summary>
    /// Configuration for exporter services.
    /// </summary>
    public class ExporterServiceConfiguration : ConfigurationBase
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the protocol used by the service.
        /// </summary>
        public string Protocol { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the operation timeout in seconds.
        /// </summary>
        public int OperationTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the maximum retry count for failed operations.
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
        /// Gets or sets the batch size for export operations.
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets a value indicating whether to validate data during export.
        /// </summary>
        public bool ValidateData { get; set; } = true;

        /// <summary>
        /// Gets or sets the schema identifier for data validation.
        /// </summary>
        public string SchemaId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema version for data validation.
        /// </summary>
        public string SchemaVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the merge strategy to use when merging data from multiple branches.
        /// </summary>
        public MergeStrategy MergeStrategy { get; set; } = MergeStrategy.CONCATENATE;

        /// <summary>
        /// Gets or sets additional parameters for the service.
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceConfiguration"/> class.
        /// </summary>
        public ExporterServiceConfiguration()
        {
            Name = "ExporterServiceConfiguration";
            Description = "Configuration for an exporter service";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceConfiguration"/> class with the specified parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public ExporterServiceConfiguration(ConfigurationParameters parameters)
        {
            Name = "ExporterServiceConfiguration";
            Description = "Configuration for an exporter service";

            if (parameters.TryGetParameter<string>("ServiceId", out var serviceId))
            {
                ServiceId = serviceId;
            }

            if (parameters.TryGetParameter<string>("Protocol", out var protocol))
            {
                Protocol = protocol;
            }

            if (parameters.TryGetParameter<int>("ConnectionTimeoutSeconds", out var connectionTimeoutSeconds))
            {
                ConnectionTimeoutSeconds = connectionTimeoutSeconds;
            }

            if (parameters.TryGetParameter<int>("OperationTimeoutSeconds", out var operationTimeoutSeconds))
            {
                OperationTimeoutSeconds = operationTimeoutSeconds;
            }

            if (parameters.TryGetParameter<int>("MaxRetryCount", out var maxRetryCount))
            {
                MaxRetryCount = maxRetryCount;
            }

            if (parameters.TryGetParameter<int>("RetryDelayMilliseconds", out var retryDelayMilliseconds))
            {
                RetryDelayMilliseconds = retryDelayMilliseconds;
            }

            if (parameters.TryGetParameter<bool>("UseExponentialBackoff", out var useExponentialBackoff))
            {
                UseExponentialBackoff = useExponentialBackoff;
            }

            if (parameters.TryGetParameter<int>("BatchSize", out var batchSize))
            {
                BatchSize = batchSize;
            }

            if (parameters.TryGetParameter<bool>("ValidateData", out var validateData))
            {
                ValidateData = validateData;
            }

            if (parameters.TryGetParameter<string>("SchemaId", out var schemaId))
            {
                SchemaId = schemaId;
            }

            if (parameters.TryGetParameter<string>("SchemaVersion", out var schemaVersion))
            {
                SchemaVersion = schemaVersion;
            }

            if (parameters.TryGetParameter<MergeStrategy>("MergeStrategy", out var mergeStrategy))
            {
                MergeStrategy = mergeStrategy;
            }

            // Copy any additional parameters
            foreach (var key in parameters.Parameters.Keys)
            {
                if (!IsStandardParameter(key) && parameters.TryGetParameter<object>(key, out var value))
                {
                    AdditionalParameters[key] = value;
                }
            }
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <returns>A validation result indicating whether the configuration is valid.</returns>
        public override ValidationResult Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ServiceId))
            {
                errors.Add("ServiceId cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(Protocol))
            {
                errors.Add("Protocol cannot be empty.");
            }

            if (ConnectionTimeoutSeconds <= 0)
            {
                errors.Add("ConnectionTimeoutSeconds must be greater than zero.");
            }

            if (OperationTimeoutSeconds <= 0)
            {
                errors.Add("OperationTimeoutSeconds must be greater than zero.");
            }

            if (MaxRetryCount < 0)
            {
                errors.Add("MaxRetryCount cannot be negative.");
            }

            if (RetryDelayMilliseconds < 0)
            {
                errors.Add("RetryDelayMilliseconds cannot be negative.");
            }

            if (BatchSize <= 0)
            {
                errors.Add("BatchSize must be greater than zero.");
            }

            if (ValidateData && string.IsNullOrWhiteSpace(SchemaId))
            {
                errors.Add("SchemaId cannot be empty when ValidateData is true.");
            }

            if (ValidateData && string.IsNullOrWhiteSpace(SchemaVersion))
            {
                errors.Add("SchemaVersion cannot be empty when ValidateData is true.");
            }

            if (errors.Count > 0)
            {
                return ValidationResult.Error("Configuration validation failed.", errors.ToArray());
            }

            return ValidationResult.Success("Configuration validation successful.");
        }

        /// <summary>
        /// Determines whether the specified parameter is a standard parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>True if the parameter is a standard parameter, otherwise false.</returns>
        private bool IsStandardParameter(string parameterName)
        {
            return parameterName switch
            {
                "ServiceId" or "Protocol" or "ConnectionTimeoutSeconds" or "OperationTimeoutSeconds" or
                "MaxRetryCount" or "RetryDelayMilliseconds" or "UseExponentialBackoff" or
                "BatchSize" or "ValidateData" or "SchemaId" or "SchemaVersion" or "MergeStrategy" => true,
                _ => false
            };
        }
    }
}
