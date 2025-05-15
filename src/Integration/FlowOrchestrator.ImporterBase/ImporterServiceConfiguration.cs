using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Common.Configuration;
using FlowOrchestrator.Common.Validation;
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Integration.Importers
{
    /// <summary>
    /// Configuration for importer services.
    /// </summary>
    public class ImporterServiceConfiguration : ConfigurationBase
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the protocol supported by the importer service.
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
        /// Gets or sets the batch size for import operations.
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets a value indicating whether to validate data during import.
        /// </summary>
        public bool ValidateData { get; set; } = true;

        /// <summary>
        /// Gets or sets the schema identifier for validation.
        /// </summary>
        public string? SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the schema version for validation.
        /// </summary>
        public string? SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets additional configuration parameters.
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceConfiguration"/> class.
        /// </summary>
        public ImporterServiceConfiguration()
        {
            Name = "Default Importer Configuration";
            Description = "Default configuration for importer services";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceConfiguration"/> class with the specified parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public ImporterServiceConfiguration(ConfigurationParameters parameters)
        {
            if (parameters.TryGetParameter<string>("Name", out var name))
            {
                Name = name;
            }

            if (parameters.TryGetParameter<string>("Description", out var description))
            {
                Description = description;
            }

            if (parameters.TryGetParameter<string>("Version", out var version))
            {
                Version = version;
            }

            if (parameters.TryGetParameter<string>("ServiceId", out var serviceId))
            {
                ServiceId = serviceId;
            }

            if (parameters.TryGetParameter<string>("Protocol", out var protocol))
            {
                Protocol = protocol;
            }

            if (parameters.TryGetParameter<int>("ConnectionTimeoutSeconds", out var connectionTimeout))
            {
                ConnectionTimeoutSeconds = connectionTimeout;
            }

            if (parameters.TryGetParameter<int>("OperationTimeoutSeconds", out var operationTimeout))
            {
                OperationTimeoutSeconds = operationTimeout;
            }

            if (parameters.TryGetParameter<int>("MaxRetryCount", out var maxRetryCount))
            {
                MaxRetryCount = maxRetryCount;
            }

            if (parameters.TryGetParameter<int>("RetryDelayMilliseconds", out var retryDelay))
            {
                RetryDelayMilliseconds = retryDelay;
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

            // Validate required fields
            if (string.IsNullOrWhiteSpace(ServiceId))
            {
                errors.Add("ServiceId is required");
            }

            if (string.IsNullOrWhiteSpace(Protocol))
            {
                errors.Add("Protocol is required");
            }

            // Validate numeric fields
            if (ConnectionTimeoutSeconds <= 0)
            {
                errors.Add("ConnectionTimeoutSeconds must be greater than zero");
            }

            if (OperationTimeoutSeconds <= 0)
            {
                errors.Add("OperationTimeoutSeconds must be greater than zero");
            }

            if (MaxRetryCount < 0)
            {
                errors.Add("MaxRetryCount must be greater than or equal to zero");
            }

            if (RetryDelayMilliseconds < 0)
            {
                errors.Add("RetryDelayMilliseconds must be greater than or equal to zero");
            }

            if (BatchSize <= 0)
            {
                errors.Add("BatchSize must be greater than zero");
            }

            // Validate schema configuration
            if (ValidateData && string.IsNullOrWhiteSpace(SchemaId))
            {
                errors.Add("SchemaId is required when ValidateData is true");
            }

            if (errors.Count > 0)
            {
                return ValidationResult.Error("Configuration validation failed", errors.ToArray());
            }

            return ValidationResult.Success("Configuration validation successful");
        }

        /// <summary>
        /// Converts the configuration to a ConfigurationParameters object.
        /// </summary>
        /// <returns>A ConfigurationParameters object containing the configuration values.</returns>
        public ConfigurationParameters ToConfigurationParameters()
        {
            var parameters = new ConfigurationParameters();

            parameters.SetParameter("Name", Name);
            parameters.SetParameter("Description", Description);
            parameters.SetParameter("Version", Version);
            parameters.SetParameter("ServiceId", ServiceId);
            parameters.SetParameter("Protocol", Protocol);
            parameters.SetParameter("ConnectionTimeoutSeconds", ConnectionTimeoutSeconds);
            parameters.SetParameter("OperationTimeoutSeconds", OperationTimeoutSeconds);
            parameters.SetParameter("MaxRetryCount", MaxRetryCount);
            parameters.SetParameter("RetryDelayMilliseconds", RetryDelayMilliseconds);
            parameters.SetParameter("UseExponentialBackoff", UseExponentialBackoff);
            parameters.SetParameter("BatchSize", BatchSize);
            parameters.SetParameter("ValidateData", ValidateData);

            if (!string.IsNullOrWhiteSpace(SchemaId))
            {
                parameters.SetParameter("SchemaId", SchemaId);
            }

            if (!string.IsNullOrWhiteSpace(SchemaVersion))
            {
                parameters.SetParameter("SchemaVersion", SchemaVersion);
            }

            // Add additional parameters
            foreach (var kvp in AdditionalParameters)
            {
                parameters.SetParameter(kvp.Key, kvp.Value);
            }

            return parameters;
        }

        private bool IsStandardParameter(string key)
        {
            return key switch
            {
                "Name" or "Description" or "Version" or "ServiceId" or "Protocol" or
                "ConnectionTimeoutSeconds" or "OperationTimeoutSeconds" or "MaxRetryCount" or
                "RetryDelayMilliseconds" or "UseExponentialBackoff" or "BatchSize" or
                "ValidateData" or "SchemaId" or "SchemaVersion" => true,
                _ => false
            };
        }
    }
}
