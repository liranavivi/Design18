using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Management.Configuration.Models;
using FlowOrchestrator.Management.Configuration.Models.DTOs;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;
using CommonValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Management.Configuration.Services
{
    /// <summary>
    /// Service for managing configuration entries and schemas.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private readonly Dictionary<string, ConfigurationEntry> _configurations = new();
        private readonly Dictionary<string, ConfigurationSchema> _schemas = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            InitializeDefaultSchemas();
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ConfigurationResponse>> GetAllConfigurationsAsync()
        {
            var configurations = _configurations.Values
                .Select(MapToConfigurationResponse)
                .ToList();

            return Task.FromResult<IEnumerable<ConfigurationResponse>>(configurations);
        }

        /// <inheritdoc/>
        public Task<ConfigurationResponse?> GetConfigurationByIdAsync(string id)
        {
            if (_configurations.TryGetValue(id, out var configuration))
            {
                return Task.FromResult<ConfigurationResponse?>(MapToConfigurationResponse(configuration));
            }

            return Task.FromResult<ConfigurationResponse?>(null);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ConfigurationResponse>> GetConfigurationsByScopeAsync(ConfigurationScope scope)
        {
            var configurations = _configurations.Values
                .Where(c => c.Scope == scope)
                .Select(MapToConfigurationResponse)
                .ToList();

            return Task.FromResult<IEnumerable<ConfigurationResponse>>(configurations);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ConfigurationResponse>> GetConfigurationsByTargetIdAsync(string targetId)
        {
            var configurations = _configurations.Values
                .Where(c => c.TargetId == targetId)
                .Select(MapToConfigurationResponse)
                .ToList();

            return Task.FromResult<IEnumerable<ConfigurationResponse>>(configurations);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ConfigurationResponse>> GetConfigurationsByEnvironmentAsync(string environment)
        {
            var configurations = _configurations.Values
                .Where(c => c.Environment == environment)
                .Select(MapToConfigurationResponse)
                .ToList();

            return Task.FromResult<IEnumerable<ConfigurationResponse>>(configurations);
        }

        /// <inheritdoc/>
        public Task<ConfigurationResponse> CreateConfigurationAsync(ConfigurationRequest request, string userName)
        {
            var configuration = new ConfigurationEntry
            {
                Name = request.Name,
                Description = request.Description,
                Version = request.Version,
                Scope = request.Scope,
                TargetId = request.TargetId,
                Environment = request.Environment,
                Values = request.Values,
                CreatedBy = userName,
                UpdatedBy = userName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            if (!string.IsNullOrEmpty(request.SchemaId) && _schemas.TryGetValue(request.SchemaId, out var schema))
            {
                configuration.Schema = schema;
            }

            var validationResult = configuration.Validate();
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Invalid configuration: {string.Join(", ", validationResult.Errors)}");
            }

            _configurations[configuration.Id] = configuration;
            _logger.LogInformation("Created configuration {ConfigurationId} for {Scope} {TargetId}", configuration.Id, configuration.Scope, configuration.TargetId);

            return Task.FromResult(MapToConfigurationResponse(configuration));
        }

        /// <inheritdoc/>
        public Task<ConfigurationResponse?> UpdateConfigurationAsync(string id, ConfigurationRequest request, string userName)
        {
            if (!_configurations.TryGetValue(id, out var configuration))
            {
                return Task.FromResult<ConfigurationResponse?>(null);
            }

            configuration.Name = request.Name;
            configuration.Description = request.Description;
            configuration.Version = request.Version;
            configuration.Scope = request.Scope;
            configuration.TargetId = request.TargetId;
            configuration.Environment = request.Environment;
            configuration.Values = request.Values;
            configuration.UpdatedBy = userName;
            configuration.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.SchemaId) && _schemas.TryGetValue(request.SchemaId, out var schema))
            {
                configuration.Schema = schema;
            }

            var validationResult = configuration.Validate();
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Invalid configuration: {string.Join(", ", validationResult.Errors)}");
            }

            _configurations[id] = configuration;
            _logger.LogInformation("Updated configuration {ConfigurationId}", id);

            return Task.FromResult<ConfigurationResponse?>(MapToConfigurationResponse(configuration));
        }

        /// <inheritdoc/>
        public Task<bool> DeleteConfigurationAsync(string id)
        {
            if (_configurations.Remove(id))
            {
                _logger.LogInformation("Deleted configuration {ConfigurationId}", id);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<ConfigurationResponse?> ActivateConfigurationAsync(string id, string userName)
        {
            if (!_configurations.TryGetValue(id, out var configuration))
            {
                return Task.FromResult<ConfigurationResponse?>(null);
            }

            configuration.IsActive = true;
            configuration.UpdatedBy = userName;
            configuration.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Activated configuration {ConfigurationId}", id);

            return Task.FromResult<ConfigurationResponse?>(MapToConfigurationResponse(configuration));
        }

        /// <inheritdoc/>
        public Task<ConfigurationResponse?> DeactivateConfigurationAsync(string id, string userName)
        {
            if (!_configurations.TryGetValue(id, out var configuration))
            {
                return Task.FromResult<ConfigurationResponse?>(null);
            }

            configuration.IsActive = false;
            configuration.UpdatedBy = userName;
            configuration.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Deactivated configuration {ConfigurationId}", id);

            return Task.FromResult<ConfigurationResponse?>(MapToConfigurationResponse(configuration));
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ConfigurationSchemaResponse>> GetAllSchemasAsync()
        {
            var schemas = _schemas.Values
                .Select(MapToConfigurationSchemaResponse)
                .ToList();

            return Task.FromResult<IEnumerable<ConfigurationSchemaResponse>>(schemas);
        }

        /// <inheritdoc/>
        public Task<ConfigurationSchemaResponse?> GetSchemaByIdAsync(string id)
        {
            if (_schemas.TryGetValue(id, out var schema))
            {
                return Task.FromResult<ConfigurationSchemaResponse?>(MapToConfigurationSchemaResponse(schema));
            }

            return Task.FromResult<ConfigurationSchemaResponse?>(null);
        }

        /// <inheritdoc/>
        public Task<ConfigurationSchemaResponse> CreateSchemaAsync(ConfigurationSchemaRequest request)
        {
            var schema = new ConfigurationSchema
            {
                Name = request.Name,
                Description = request.Description,
                Version = request.Version,
                Parameters = request.Parameters
            };

            _schemas[schema.Id] = schema;
            _logger.LogInformation("Created schema {SchemaId}", schema.Id);

            return Task.FromResult(MapToConfigurationSchemaResponse(schema));
        }

        /// <inheritdoc/>
        public Task<ConfigurationSchemaResponse?> UpdateSchemaAsync(string id, ConfigurationSchemaRequest request)
        {
            if (!_schemas.TryGetValue(id, out var schema))
            {
                return Task.FromResult<ConfigurationSchemaResponse?>(null);
            }

            schema.Name = request.Name;
            schema.Description = request.Description;
            schema.Version = request.Version;
            schema.Parameters = request.Parameters;

            _logger.LogInformation("Updated schema {SchemaId}", id);

            return Task.FromResult<ConfigurationSchemaResponse?>(MapToConfigurationSchemaResponse(schema));
        }

        /// <inheritdoc/>
        public Task<bool> DeleteSchemaAsync(string id)
        {
            if (_schemas.Remove(id))
            {
                _logger.LogInformation("Deleted schema {SchemaId}", id);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<ValidationResult> ValidateConfigurationAsync(ConfigurationValidationRequest request)
        {
            if (!_schemas.TryGetValue(request.SchemaId, out var schema))
            {
                return Task.FromResult(new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Code = "SCHEMA_NOT_FOUND",
                            Message = $"Schema with ID {request.SchemaId} not found."
                        }
                    }
                });
            }

            var validationResult = schema.ValidateValues(request.Values);
            return Task.FromResult(validationResult);
        }

        private ConfigurationResponse MapToConfigurationResponse(ConfigurationEntry configuration)
        {
            return new ConfigurationResponse
            {
                Id = configuration.Id,
                Name = configuration.Name,
                Description = configuration.Description,
                Version = configuration.Version,
                Scope = configuration.Scope,
                TargetId = configuration.TargetId,
                Environment = configuration.Environment,
                Values = configuration.Values,
                SchemaId = configuration.Schema?.Id,
                CreatedAt = configuration.CreatedAt,
                UpdatedAt = configuration.UpdatedAt,
                CreatedBy = configuration.CreatedBy,
                UpdatedBy = configuration.UpdatedBy,
                IsActive = configuration.IsActive
            };
        }

        private ConfigurationSchemaResponse MapToConfigurationSchemaResponse(ConfigurationSchema schema)
        {
            return new ConfigurationSchemaResponse
            {
                Id = schema.Id,
                Name = schema.Name,
                Description = schema.Description,
                Version = schema.Version,
                Parameters = schema.Parameters
            };
        }

        private void InitializeDefaultSchemas()
        {
            // Create system configuration schema
            var systemSchema = new ConfigurationSchema
            {
                Name = "SystemConfiguration",
                Description = "Schema for system-wide configuration",
                Version = "1.0.0",
                Parameters = new List<ConfigurationParameter>
                {
                    new ConfigurationParameter
                    {
                        Name = "LogLevel",
                        Description = "Logging level for the system",
                        Type = ParameterType.Enum,
                        Required = true,
                        DefaultValue = "Information",
                        AllowedValues = new List<string> { "Trace", "Debug", "Information", "Warning", "Error", "Critical" }
                    },
                    new ConfigurationParameter
                    {
                        Name = "EnableTelemetry",
                        Description = "Whether to enable telemetry collection",
                        Type = ParameterType.Boolean,
                        Required = false,
                        DefaultValue = true
                    },
                    new ConfigurationParameter
                    {
                        Name = "DataRetentionDays",
                        Description = "Number of days to retain data",
                        Type = ParameterType.Number,
                        Required = false,
                        DefaultValue = 30,
                        Minimum = 1,
                        Maximum = 365
                    }
                }
            };

            _schemas[systemSchema.Id] = systemSchema;
            _logger.LogInformation("Created default system schema {SchemaId}", systemSchema.Id);
        }
    }
}
