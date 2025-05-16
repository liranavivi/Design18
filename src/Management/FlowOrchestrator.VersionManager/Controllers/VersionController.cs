using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Management.Versioning.Models;
using FlowOrchestrator.Management.Versioning.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Versioning.Controllers
{
    /// <summary>
    /// Controller for managing version information.
    /// </summary>
    [ApiController]
    [Route("api/versions")]
    public class VersionController : ControllerBase
    {
        private readonly IVersionManager _versionManager;
        private readonly ILogger<VersionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionController"/> class.
        /// </summary>
        /// <param name="versionManager">The version manager service.</param>
        /// <param name="logger">The logger.</param>
        public VersionController(IVersionManager versionManager, ILogger<VersionController> logger)
        {
            _versionManager = versionManager ?? throw new ArgumentNullException(nameof(versionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets version information for a specific component and version.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <returns>The version information.</returns>
        [HttpGet("{componentType}/{componentId}/{version}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VersionInfo>> GetVersionInfo(ComponentType componentType, string componentId, string version)
        {
            try
            {
                var versionInfo = await _versionManager.GetVersionInfoAsync(componentType, componentId, version);
                if (versionInfo == null)
                {
                    return NotFound();
                }

                return Ok(versionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version info for {ComponentType} {ComponentId} {Version}", componentType, componentId, version);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the version history for a specific component.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <returns>The version history.</returns>
        [HttpGet("{componentType}/{componentId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VersionInfo>>> GetVersionHistory(ComponentType componentType, string componentId)
        {
            try
            {
                var history = await _versionManager.GetVersionHistoryAsync(componentType, componentId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for {ComponentType} {ComponentId}", componentType, componentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates the status of a specific component version.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="status">The new status.</param>
        /// <returns>A status code indicating the result of the operation.</returns>
        [HttpPut("{componentType}/{componentId}/{version}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVersionStatus(
            ComponentType componentType, string componentId, string version, [FromBody] VersionStatus status)
        {
            try
            {
                var result = await _versionManager.UpdateVersionStatusAsync(componentType, componentId, version, status);
                if (!result)
                {
                    return NotFound();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating version status for {ComponentType} {ComponentId} {Version} to {Status}",
                    componentType, componentId, version, status);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Registers a new version of a component.
        /// </summary>
        /// <param name="request">The registration request.</param>
        /// <returns>The registration result.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegistrationResult>> RegisterVersion([FromBody] VersionRegistrationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request cannot be null");
                }

                var result = await _versionManager.RegisterVersionAsync(
                    request.ComponentType,
                    request.ComponentId,
                    request.Version,
                    request.VersionInfo,
                    request.CompatibilityMatrix);

                if (result.Success)
                {
                    return CreatedAtAction(
                        nameof(GetVersionInfo),
                        new { componentType = request.ComponentType, componentId = request.ComponentId, version = request.Version },
                        result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering version for {ComponentType} {ComponentId} {Version}",
                    request?.ComponentType, request?.ComponentId, request?.Version);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specific component version.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <returns>A status code indicating the result of the operation.</returns>
        [HttpDelete("{componentType}/{componentId}/{version}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVersion(ComponentType componentType, string componentId, string version)
        {
            try
            {
                var result = await _versionManager.DeleteVersionAsync(componentType, componentId, version);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting version for {ComponentType} {ComponentId} {Version}",
                    componentType, componentId, version);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all registered components of a specific type.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns>The list of component identifiers.</returns>
        [HttpGet("{componentType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetComponents(ComponentType componentType)
        {
            try
            {
                var components = await _versionManager.GetComponentsAsync(componentType);
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting components for {ComponentType}", componentType);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all registered component types.
        /// </summary>
        /// <returns>The list of component types.</returns>
        [HttpGet("types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ComponentType>>> GetComponentTypes()
        {
            try
            {
                var types = await _versionManager.GetComponentTypesAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting component types");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
