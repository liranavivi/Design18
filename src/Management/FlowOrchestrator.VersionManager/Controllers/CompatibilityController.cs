using FlowOrchestrator.Management.Versioning.Models;
using FlowOrchestrator.Management.Versioning.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Versioning.Controllers
{
    /// <summary>
    /// Controller for managing compatibility information.
    /// </summary>
    [ApiController]
    [Route("api/compatibility")]
    public class CompatibilityController : ControllerBase
    {
        private readonly IVersionManager _versionManager;
        private readonly ILogger<CompatibilityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibilityController"/> class.
        /// </summary>
        /// <param name="versionManager">The version manager service.</param>
        /// <param name="logger">The logger.</param>
        public CompatibilityController(IVersionManager versionManager, ILogger<CompatibilityController> logger)
        {
            _versionManager = versionManager ?? throw new ArgumentNullException(nameof(versionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the compatibility matrix for a specific component and version.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <returns>The compatibility matrix.</returns>
        [HttpGet("{componentType}/{componentId}/{version}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CompatibilityMatrix>> GetCompatibilityMatrix(
            ComponentType componentType, string componentId, string version)
        {
            try
            {
                var matrix = await _versionManager.GetCompatibilityMatrixAsync(componentType, componentId, version);
                if (matrix == null)
                {
                    return NotFound();
                }

                return Ok(matrix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compatibility matrix for {ComponentType} {ComponentId} {Version}",
                    componentType, componentId, version);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Checks if two component versions are compatible.
        /// </summary>
        /// <param name="request">The compatibility check request.</param>
        /// <returns>The compatibility check result.</returns>
        [HttpPost("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CompatibilityCheckResult>> CheckCompatibility([FromBody] CompatibilityCheckRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request cannot be null");
                }

                var isCompatible = await _versionManager.IsVersionCompatibleAsync(
                    request.SourceType,
                    request.SourceId,
                    request.SourceVersion,
                    request.TargetType,
                    request.TargetId,
                    request.TargetVersion);

                var result = new CompatibilityCheckResult
                {
                    SourceType = request.SourceType,
                    SourceId = request.SourceId,
                    SourceVersion = request.SourceVersion,
                    TargetType = request.TargetType,
                    TargetId = request.TargetId,
                    TargetVersion = request.TargetVersion,
                    IsCompatible = isCompatible
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking compatibility between {SourceType} {SourceId} {SourceVersion} and {TargetType} {TargetId} {TargetVersion}",
                    request?.SourceType, request?.SourceId, request?.SourceVersion,
                    request?.TargetType, request?.TargetId, request?.TargetVersion);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
