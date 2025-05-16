using FlowOrchestrator.Management.Configuration.Models.DTOs;
using FlowOrchestrator.Management.Configuration.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Configuration.Controllers
{
    /// <summary>
    /// Controller for managing configuration schemas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SchemaController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<SchemaController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaController"/> class.
        /// </summary>
        /// <param name="configurationService">The configuration service.</param>
        /// <param name="logger">The logger.</param>
        public SchemaController(IConfigurationService configurationService, ILogger<SchemaController> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all configuration schemas.
        /// </summary>
        /// <returns>A collection of configuration schemas.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ConfigurationSchemaResponse>>> GetAll()
        {
            try
            {
                var schemas = await _configurationService.GetAllSchemasAsync();
                return Ok(schemas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all schemas");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving schemas");
            }
        }

        /// <summary>
        /// Gets a configuration schema by ID.
        /// </summary>
        /// <param name="id">The ID of the schema.</param>
        /// <returns>The configuration schema.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConfigurationSchemaResponse>> GetById(string id)
        {
            try
            {
                var schema = await _configurationService.GetSchemaByIdAsync(id);
                if (schema == null)
                {
                    return NotFound($"Schema with ID {id} not found");
                }

                return Ok(schema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schema {SchemaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the schema");
            }
        }

        /// <summary>
        /// Creates a new configuration schema.
        /// </summary>
        /// <param name="request">The schema request.</param>
        /// <returns>The created configuration schema.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ConfigurationSchemaResponse>> Create([FromBody] ConfigurationSchemaRequest request)
        {
            try
            {
                var schema = await _configurationService.CreateSchemaAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = schema.Id }, schema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schema");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the schema");
            }
        }

        /// <summary>
        /// Updates an existing configuration schema.
        /// </summary>
        /// <param name="id">The ID of the schema to update.</param>
        /// <param name="request">The updated schema request.</param>
        /// <returns>The updated configuration schema.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConfigurationSchemaResponse>> Update(string id, [FromBody] ConfigurationSchemaRequest request)
        {
            try
            {
                var schema = await _configurationService.UpdateSchemaAsync(id, request);
                if (schema == null)
                {
                    return NotFound($"Schema with ID {id} not found");
                }

                return Ok(schema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schema {SchemaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the schema");
            }
        }

        /// <summary>
        /// Deletes a configuration schema.
        /// </summary>
        /// <param name="id">The ID of the schema to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _configurationService.DeleteSchemaAsync(id);
                if (!result)
                {
                    return NotFound($"Schema with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schema {SchemaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the schema");
            }
        }

        /// <summary>
        /// Validates configuration values against a schema.
        /// </summary>
        /// <param name="request">The validation request.</param>
        /// <returns>The validation result.</returns>
        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConfigurationValidationResponse>> Validate([FromBody] ConfigurationValidationRequest request)
        {
            try
            {
                var validationResult = await _configurationService.ValidateConfigurationAsync(request);
                return Ok(new ConfigurationValidationResponse
                {
                    IsValid = validationResult.IsValid,
                    Errors = validationResult.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration against schema {SchemaId}", request.SchemaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while validating the configuration");
            }
        }
    }
}
