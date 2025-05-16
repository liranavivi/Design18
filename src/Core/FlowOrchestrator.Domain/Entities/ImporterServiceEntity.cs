using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using System;
using System.Collections.Generic;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Entity class for importer services.
    /// </summary>
    public class ImporterServiceEntity : AbstractEntity, IImporterService
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        public new string Version { get; set; }

        /// <summary>
        /// Gets or sets the service type.
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the service name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the service description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the service status.
        /// </summary>
        public Abstractions.Services.ServiceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the service capabilities.
        /// </summary>
        public List<string> Capabilities { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the service configuration.
        /// </summary>
        public ConfigurationParameters Configuration { get; set; } = new ConfigurationParameters();

        /// <summary>
        /// Gets or sets the service protocol.
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets the service endpoint.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the supported file formats.
        /// </summary>
        public List<string> SupportedFormats { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the maximum file size in bytes.
        /// </summary>
        public long MaxFileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the polling interval in seconds.
        /// </summary>
        public int PollingIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the retry delay in seconds.
        /// </summary>
        public int RetryDelaySeconds { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the concurrency limit.
        /// </summary>
        public int ConcurrencyLimit { get; set; }

        /// <summary>
        /// Gets or sets the last heartbeat timestamp.
        /// </summary>
        public DateTime LastHeartbeat { get; set; }

        /// <summary>
        /// Gets or sets the registration timestamp.
        /// </summary>
        public DateTime RegistrationTimestamp { get; set; }

        /// <summary>
        /// Initializes the service with the specified configuration.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public void Initialize(ConfigurationParameters parameters)
        {
            Configuration = parameters;
            Status = Abstractions.Services.ServiceStatus.ACTIVE;
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        public void Start()
        {
            Status = Abstractions.Services.ServiceStatus.ACTIVE;
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public void Stop()
        {
            Status = Abstractions.Services.ServiceStatus.INACTIVE;
        }

        /// <summary>
        /// Pauses the service.
        /// </summary>
        public void Pause()
        {
            Status = Abstractions.Services.ServiceStatus.SUSPENDED;
        }

        /// <summary>
        /// Resumes the service.
        /// </summary>
        public void Resume()
        {
            Status = Abstractions.Services.ServiceStatus.ACTIVE;
        }

        /// <summary>
        /// Gets the service status.
        /// </summary>
        /// <returns>The service status.</returns>
        public Abstractions.Services.ServiceStatus GetStatus()
        {
            return Status;
        }

        /// <summary>
        /// Gets the service health.
        /// </summary>
        /// <returns>The service health.</returns>
        public Abstractions.Common.ServiceHealth GetHealth()
        {
            // Simple health check based on status
            if (Status == Abstractions.Services.ServiceStatus.ACTIVE)
                return Abstractions.Common.ServiceHealth.HEALTHY;
            else if (Status == Abstractions.Services.ServiceStatus.SUSPENDED)
                return Abstractions.Common.ServiceHealth.WARNING;
            else
                return Abstractions.Common.ServiceHealth.UNHEALTHY;
        }

        /// <summary>
        /// Gets the service metrics.
        /// </summary>
        /// <returns>The service metrics.</returns>
        public Dictionary<string, object> GetMetrics()
        {
            return new Dictionary<string, object>
            {
                { "status", Status.ToString() },
                { "lastHeartbeat", LastHeartbeat },
                { "uptime", (DateTime.UtcNow - RegistrationTimestamp).TotalSeconds }
            };
        }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public override string GetEntityId()
        {
            return ServiceId;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public override string GetEntityType()
        {
            return "ImporterService";
        }

        /// <summary>
        /// Validates the entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override Abstractions.Common.ValidationResult Validate()
        {
            var result = new Abstractions.Common.ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(ServiceId))
            {
                result.IsValid = false;
                result.Errors.Add(new Abstractions.Common.ValidationError { Code = "EMPTY_SERVICE_ID", Message = "ServiceId cannot be null or empty" });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.IsValid = false;
                result.Errors.Add(new Abstractions.Common.ValidationError { Code = "EMPTY_NAME", Message = "Name cannot be null or empty" });
            }

            if (string.IsNullOrWhiteSpace(ServiceType))
            {
                result.IsValid = false;
                result.Errors.Add(new Abstractions.Common.ValidationError { Code = "EMPTY_SERVICE_TYPE", Message = "ServiceType cannot be null or empty" });
            }

            if (string.IsNullOrWhiteSpace(Protocol))
            {
                result.IsValid = false;
                result.Errors.Add(new Abstractions.Common.ValidationError { Code = "EMPTY_PROTOCOL", Message = "Protocol cannot be null or empty" });
            }

            return result;
        }

        /// <summary>
        /// Terminates the service.
        /// </summary>
        public void Terminate()
        {
            Status = Abstractions.Services.ServiceStatus.RETIRED;
        }

        /// <summary>
        /// Gets the service state.
        /// </summary>
        /// <returns>The service state.</returns>
        public Abstractions.Common.ServiceState GetState()
        {
            // Map the service status to service state
            Abstractions.Common.ServiceState state;

            switch (Status)
            {
                case Abstractions.Services.ServiceStatus.ACTIVE:
                    state = Abstractions.Common.ServiceState.READY;
                    break;
                case Abstractions.Services.ServiceStatus.INACTIVE:
                    state = Abstractions.Common.ServiceState.UNINITIALIZED;
                    break;
                case Abstractions.Services.ServiceStatus.SUSPENDED:
                    state = Abstractions.Common.ServiceState.PROCESSING;
                    break;
                case Abstractions.Services.ServiceStatus.DEPRECATED:
                    state = Abstractions.Common.ServiceState.ERROR;
                    break;
                case Abstractions.Services.ServiceStatus.RETIRED:
                    state = Abstractions.Common.ServiceState.TERMINATED;
                    break;
                default:
                    state = Abstractions.Common.ServiceState.UNINITIALIZED;
                    break;
            }

            return state;
        }

        /// <summary>
        /// Gets the service capabilities.
        /// </summary>
        /// <returns>The service capabilities.</returns>
        public Abstractions.Services.ImporterCapabilities GetCapabilities()
        {
            return new Abstractions.Services.ImporterCapabilities
            {
                SupportedSourceTypes = new List<string> { "File" },
                SupportedDataFormats = SupportedFormats,
                SupportsValidation = true,
                SupportsStreaming = false,
                SupportsBatching = true,
                SupportsFiltering = false,
                SupportsTransformation = false,
                MaxBatchSize = 100
            };
        }

        /// <summary>
        /// Validates the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <returns>The validation result.</returns>
        public Abstractions.Common.ValidationResult ValidateParameters(Abstractions.Services.ImportParameters parameters)
        {
            // This is a stub implementation for the entity
            return Abstractions.Common.ValidationResult.Valid();
        }

        /// <summary>
        /// Imports data using the specified parameters.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The import result.</returns>
        public Abstractions.Services.ImportResult Import(Abstractions.Services.ImportParameters parameters, Abstractions.Common.ExecutionContext context)
        {
            // This is a stub implementation for the entity
            return new Abstractions.Services.ImportResult
            {
                Success = true,
                SourceMetadata = new Dictionary<string, object>
                {
                    { "message", "Import operation not supported in entity representation" }
                }
            };
        }
    }
}
