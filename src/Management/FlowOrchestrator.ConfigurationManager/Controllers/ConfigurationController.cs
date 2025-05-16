using FlowOrchestrator.Management.Configuration.Models;
using FlowOrchestrator.Management.Configuration.Models.DTOs;
using FlowOrchestrator.Management.Configuration.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Configuration.Controllers
{
    /// <summary>
    /// Controller for managing configuration entries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<ConfigurationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationController"/> class.
        /// </summary>
        /// <param name="configurationService">The configuration service.</param>
        /// <param name="logger">The logger.</param>
        public ConfigurationController(IConfigurationService configurationService, ILogger<ConfigurationController> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all configuration entries.
        /// </summary>
        /// <returns>A collection of configuration entries.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetAll()
        {
            try
            {
                var configurations = await _configurationService.GetAllConfigurationsAsync();
                return Ok(configurations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all configurations");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving configurations");
            }
        }

        /// <summary>
        /// Gets a configuration entry by ID.
        /// </summary>
        /// <param name="id">The ID of the configuration entry.</param>
        /// <returns>The configuration entry.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConfigurationResponse>> GetById(string id)
        {
            try
            {
                var configuration = await _configurationService.GetConfigurationByIdAsync(id);
                if (configuration == null)
                {
                    return NotFound($"Configuration with ID {id} not found");
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration {ConfigurationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the configuration");
            }
        }

        /// <summary>
        /// Gets configuration entries by scope.
        /// </summary>
        /// <param name="scope">The scope to filter by.</param>
        /// <returns>A collection of configuration entries.</returns>
        [HttpGet("scope/{scope}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetByScope(ConfigurationScope scope)
        {
            try
            {
                var configurations = await _configurationService.GetConfigurationsByScopeAsync(scope);
                return Ok(configurations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configurations by scope {Scope}", scope);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving configurations");
            }
        }

        /// <summary>
        /// Gets configuration entries by target ID.
        /// </summary>
        /// <param name="targetId">The target ID to filter by.</param>
        /// <returns>A collection of configuration entries.</returns>
        [HttpGet("target/{targetId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetByTargetId(string targetId)
        {
            try
            {
                var configurations = await _configurationService.GetConfigurationsByTargetIdAsync(targetId);
                return Ok(configurations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configurations by target ID {TargetId}", targetId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving configurations");
            }
        }

        /// <summary>
        /// Gets configuration entries by environment.
        /// </summary>
        /// <param name="environment">The environment to filter by.</param>
        /// <returns>A collection of configuration entries.</returns>
        [HttpGet("environment/{environment}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetByEnvironment(string environment)
        {
            try
            {
                var configurations = await _configurationService.GetConfigurationsByEnvironmentAsync(environment);
                return Ok(configurations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configurations by environment {Environment}", environment);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving configurations");
            }
        }

        /// <summary>
        /// Creates a new configuration entry.
        /// </summary>
        /// <param name="request">The configuration request.</param>
        /// <returns>The created configuration entry.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ConfigurationResponse>> Create([FromBody] ConfigurationRequest request)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                var configuration = await _configurationService.CreateConfigurationAsync(request, userName);
                return CreatedAtAction(nameof(GetById), new { id = configuration.Id }, configuration);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid configuration request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the configuration");
            }
        }

        /// <summary>
        /// Updates an existing configuration entry.
        /// </summary>
        /// <param name="id">The ID of the configuration entry to update.</param>
        /// <param name="request">The updated configuration request.</param>
        /// <returns>The updated configuration entry.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ConfigurationResponse>> Update(string id, [FromBody] ConfigurationRequest request)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                var configuration = await _configurationService.UpdateConfigurationAsync(id, request, userName);
                if (configuration == null)
                {
                    return NotFound($"Configuration with ID {id} not found");
                }

                return Ok(configuration);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid configuration request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration {ConfigurationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the configuration");
            }
        }

        /// <summary>
        /// Deletes a configuration entry.
        /// </summary>
        /// <param name="id">The ID of the configuration entry to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _configurationService.DeleteConfigurationAsync(id);
                if (!result)
                {
                    return NotFound($"Configuration with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration {ConfigurationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the configuration");
            }
        }

        /// <summary>
        /// Activates a configuration entry.
        /// </summary>
        /// <param name="id">The ID of the configuration entry to activate.</param>
        /// <returns>The activated configuration entry.</returns>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConfigurationResponse>> Activate(string id)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                var configuration = await _configurationService.ActivateConfigurationAsync(id, userName);
                if (configuration == null)
                {
                    return NotFound($"Configuration with ID {id} not found");
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating configuration {ConfigurationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while activating the configuration");
            }
        }

        /// <summary>
        /// Deactivates a configuration entry.
        /// </summary>
        /// <param name="id">The ID of the configuration entry to deactivate.</param>
        /// <returns>The deactivated configuration entry.</returns>
        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConfigurationResponse>> Deactivate(string id)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                var configuration = await _configurationService.DeactivateConfigurationAsync(id, userName);
                if (configuration == null)
                {
                    return NotFound($"Configuration with ID {id} not found");
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating configuration {ConfigurationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deactivating the configuration");
            }
        }
    }
}
