using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Common.Validation;
using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Use ValidationResult from FlowOrchestrator.Common.Validation to avoid ambiguity
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Management.Services
{
    /// <summary>
    /// Manager service for importer services.
    /// </summary>
    public class ImporterServiceManager : BaseManagerService<IImporterService>
    {
        private readonly IEntityRepository<ImporterServiceEntity> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceManager"/> class.
        /// </summary>
        /// <param name="repository">The importer service repository.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public ImporterServiceManager(
            IEntityRepository<ImporterServiceEntity> repository,
            ILogger<ImporterServiceManager> logger,
            IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "IMPORTER-SERVICE-MANAGER";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "ImporterServiceManager";

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType.Contains("Importer", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "ImporterService";
        }

        /// <summary>
        /// Validates service-specific aspects.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateServiceSpecifics(IImporterService service)
        {
            var errors = new List<string>();

            // Validate protocol
            if (string.IsNullOrWhiteSpace(service.Protocol))
            {
                errors.Add("Protocol cannot be null or empty");
            }

            // Validate capabilities
            try
            {
                var capabilities = service.GetCapabilities();
                if (capabilities == null)
                {
                    errors.Add("GetCapabilities() returned null");
                }
                else
                {
                    // Validate supported source types
                    if (capabilities.SupportedSourceTypes == null || !capabilities.SupportedSourceTypes.Any())
                    {
                        errors.Add("Importer must support at least one source type");
                    }

                    // Validate supported data formats
                    if (capabilities.SupportedDataFormats == null || !capabilities.SupportedDataFormats.Any())
                    {
                        errors.Add("Importer must support at least one data format");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error getting capabilities: {ex.Message}");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Importer service validation failed", errors.ToArray())
                : ValidationResult.Success("Importer service validation successful");
        }

        /// <summary>
        /// Validates manager-specific configuration.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateManagerSpecificConfiguration(ConfigurationParameters parameters)
        {
            // No additional validation for now
            return ValidationResult.Success("Configuration validation successful");
        }



        /// <summary>
        /// Gets a service by its identifier.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The service, or null if not found.</returns>
        public async Task<IImporterService> GetServiceByIdAsync(string serviceId)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(serviceId);
                if (entity == null)
                {
                    _logger.LogWarning("Importer service with ID {ServiceId} not found", serviceId);
                    return new ImporterServiceEntity
                    {
                        ServiceId = "NOT_FOUND",
                        Version = "0.0.0",
                        ServiceType = "ImporterService",
                        Protocol = "none"
                    };
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving importer service with ID {ServiceId}", serviceId);
                throw;
            }
        }

        /// <summary>
        /// Gets all services.
        /// </summary>
        /// <returns>A collection of all services.</returns>
        public async Task<IEnumerable<IImporterService>> GetAllServicesAsync()
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                return entities.Cast<IImporterService>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all importer services");
                throw;
            }
        }

        /// <summary>
        /// Registers a service.
        /// </summary>
        /// <param name="service">The service to register.</param>
        /// <returns>The registered service.</returns>
        public async Task<IImporterService> RegisterServiceAsync(IImporterService service)
        {
            try
            {
                // Convert to entity if not already an entity
                var entity = service as ImporterServiceEntity;
                if (entity == null)
                {
                    entity = new ImporterServiceEntity
                    {
                        ServiceId = service.ServiceId,
                        Version = service.Version,
                        ServiceType = service.ServiceType,
                        Protocol = service.Protocol,
                        RegistrationTimestamp = DateTime.UtcNow,
                        LastHeartbeat = DateTime.UtcNow
                    };

                    // Set default capabilities
                    entity.Capabilities = new List<string>();

                    // Set default configuration
                    entity.Configuration = new ConfigurationParameters();
                }

                await _repository.AddAsync(entity);
                _logger.LogInformation("Importer service with ID {ServiceId} registered successfully", service.ServiceId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering importer service with ID {ServiceId}", service.ServiceId);
                throw;
            }
        }

        /// <summary>
        /// Updates a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="service">The updated service.</param>
        /// <returns>True if the service was updated; otherwise, false.</returns>
        public async Task<bool> UpdateServiceAsync(string serviceId, IImporterService service)
        {
            try
            {
                // Convert to entity if not already an entity
                var entity = service as ImporterServiceEntity;
                if (entity == null)
                {
                    entity = new ImporterServiceEntity
                    {
                        ServiceId = service.ServiceId,
                        Version = service.Version,
                        ServiceType = service.ServiceType,
                        Protocol = service.Protocol,
                        LastHeartbeat = DateTime.UtcNow
                    };

                    // Set default capabilities
                    entity.Capabilities = new List<string>();

                    // Set default configuration
                    entity.Configuration = new ConfigurationParameters();
                }

                var result = await _repository.UpdateAsync(serviceId, entity);
                if (result)
                {
                    _logger.LogInformation("Importer service with ID {ServiceId} updated successfully", serviceId);
                }
                else
                {
                    _logger.LogWarning("Importer service with ID {ServiceId} not found for update", serviceId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating importer service with ID {ServiceId}", serviceId);
                throw;
            }
        }

        /// <summary>
        /// Deregisters a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>True if the service was deregistered; otherwise, false.</returns>
        public async Task<bool> DeregisterServiceAsync(string serviceId)
        {
            try
            {
                var result = await _repository.DeleteAsync(serviceId);
                if (result)
                {
                    _logger.LogInformation("Importer service with ID {ServiceId} deregistered successfully", serviceId);
                }
                else
                {
                    _logger.LogWarning("Importer service with ID {ServiceId} not found for deregistration", serviceId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deregistering importer service with ID {ServiceId}", serviceId);
                throw;
            }
        }
    }
}
