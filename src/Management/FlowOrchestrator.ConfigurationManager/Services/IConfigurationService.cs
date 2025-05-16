using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Management.Configuration.Models;
using FlowOrchestrator.Management.Configuration.Models.DTOs;

namespace FlowOrchestrator.Management.Configuration.Services
{
    /// <summary>
    /// Interface for the configuration service.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets all configuration entries.
        /// </summary>
        /// <returns>A collection of configuration entries.</returns>
        Task<IEnumerable<ConfigurationResponse>> GetAllConfigurationsAsync();

        /// <summary>
        /// Gets a configuration entry by ID.
        /// </summary>
        /// <param name="id">The ID of the configuration entry.</param>
        /// <returns>The configuration entry, or null if not found.</returns>
        Task<ConfigurationResponse?> GetConfigurationByIdAsync(string id);

        /// <summary>
        /// Gets configuration entries by scope.
        /// </summary>
        /// <param name="scope">The scope to filter by.</param>
        /// <returns>A collection of configuration entries.</returns>
        Task<IEnumerable<ConfigurationResponse>> GetConfigurationsByScopeAsync(ConfigurationScope scope);

        /// <summary>
        /// Gets configuration entries by target ID.
        /// </summary>
        /// <param name="targetId">The target ID to filter by.</param>
        /// <returns>A collection of configuration entries.</returns>
        Task<IEnumerable<ConfigurationResponse>> GetConfigurationsByTargetIdAsync(string targetId);

        /// <summary>
        /// Gets configuration entries by environment.
        /// </summary>
        /// <param name="environment">The environment to filter by.</param>
        /// <returns>A collection of configuration entries.</returns>
        Task<IEnumerable<ConfigurationResponse>> GetConfigurationsByEnvironmentAsync(string environment);

        /// <summary>
        /// Creates a new configuration entry.
        /// </summary>
        /// <param name="request">The configuration request.</param>
        /// <param name="userName">The name of the user creating the configuration.</param>
        /// <returns>The created configuration entry.</returns>
        Task<ConfigurationResponse> CreateConfigurationAsync(ConfigurationRequest request, string userName);

        /// <summary>
        /// Updates an existing configuration entry.
        /// </summary>
        /// <param name="id">The ID of the configuration entry to update.</param>
        /// <param name="request">The updated configuration request.</param>
        /// <param name="userName">The name of the user updating the configuration.</param>
        /// <returns>The updated configuration entry, or null if not found.</returns>
        Task<ConfigurationResponse?> UpdateConfigurationAsync(string id, ConfigurationRequest request, string userName);

        /// <summary>
        /// Deletes a configuration entry.
        /// </summary>
        /// <param name="id">The ID of the configuration entry to delete.</param>
        /// <returns>True if the configuration was deleted, false if not found.</returns>
        Task<bool> DeleteConfigurationAsync(string id);

        /// <summary>
        /// Activates a configuration entry.
        /// </summary>
        /// <param name="id">The ID of the configuration entry to activate.</param>
        /// <param name="userName">The name of the user activating the configuration.</param>
        /// <returns>The activated configuration entry, or null if not found.</returns>
        Task<ConfigurationResponse?> ActivateConfigurationAsync(string id, string userName);

        /// <summary>
        /// Deactivates a configuration entry.
        /// </summary>
        /// <param name="id">The ID of the configuration entry to deactivate.</param>
        /// <param name="userName">The name of the user deactivating the configuration.</param>
        /// <returns>The deactivated configuration entry, or null if not found.</returns>
        Task<ConfigurationResponse?> DeactivateConfigurationAsync(string id, string userName);

        /// <summary>
        /// Gets all configuration schemas.
        /// </summary>
        /// <returns>A collection of configuration schemas.</returns>
        Task<IEnumerable<ConfigurationSchemaResponse>> GetAllSchemasAsync();

        /// <summary>
        /// Gets a configuration schema by ID.
        /// </summary>
        /// <param name="id">The ID of the schema.</param>
        /// <returns>The configuration schema, or null if not found.</returns>
        Task<ConfigurationSchemaResponse?> GetSchemaByIdAsync(string id);

        /// <summary>
        /// Creates a new configuration schema.
        /// </summary>
        /// <param name="request">The schema request.</param>
        /// <returns>The created configuration schema.</returns>
        Task<ConfigurationSchemaResponse> CreateSchemaAsync(ConfigurationSchemaRequest request);

        /// <summary>
        /// Updates an existing configuration schema.
        /// </summary>
        /// <param name="id">The ID of the schema to update.</param>
        /// <param name="request">The updated schema request.</param>
        /// <returns>The updated configuration schema, or null if not found.</returns>
        Task<ConfigurationSchemaResponse?> UpdateSchemaAsync(string id, ConfigurationSchemaRequest request);

        /// <summary>
        /// Deletes a configuration schema.
        /// </summary>
        /// <param name="id">The ID of the schema to delete.</param>
        /// <returns>True if the schema was deleted, false if not found.</returns>
        Task<bool> DeleteSchemaAsync(string id);

        /// <summary>
        /// Validates configuration values against a schema.
        /// </summary>
        /// <param name="request">The validation request.</param>
        /// <returns>The validation result.</returns>
        Task<ValidationResult> ValidateConfigurationAsync(ConfigurationValidationRequest request);
    }
}
