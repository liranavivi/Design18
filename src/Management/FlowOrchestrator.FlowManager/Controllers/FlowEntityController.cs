using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Management.Flows.Models;
using FlowOrchestrator.Management.Flows.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Flows.Controllers
{
    /// <summary>
    /// Controller for managing flow entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FlowEntityController : ControllerBase
    {
        private readonly FlowEntityManager _flowEntityManager;
        private readonly ILogger<FlowEntityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowEntityController"/> class.
        /// </summary>
        /// <param name="flowEntityManager">The flow entity manager.</param>
        /// <param name="logger">The logger instance.</param>
        public FlowEntityController(FlowEntityManager flowEntityManager, ILogger<FlowEntityController> logger)
        {
            _flowEntityManager = flowEntityManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered flow entities.
        /// </summary>
        /// <returns>The collection of registered flow entities.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IFlowEntity>> GetAll()
        {
            try
            {
                var flows = _flowEntityManager.GetAllFlows();
                return Ok(flows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all flow entities");
                return StatusCode(500, "An error occurred while retrieving flow entities");
            }
        }

        /// <summary>
        /// Gets a flow entity by its identifier.
        /// </summary>
        /// <param name="id">The flow identifier.</param>
        /// <returns>The flow entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<IFlowEntity> Get(string id)
        {
            try
            {
                var flow = _flowEntityManager.GetFlow(id);
                if (flow == null)
                {
                    return NotFound($"Flow with ID '{id}' not found");
                }

                return Ok(flow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flow entity with ID {FlowId}", id);
                return StatusCode(500, $"An error occurred while retrieving flow entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Gets all versions of a flow entity.
        /// </summary>
        /// <param name="id">The flow identifier.</param>
        /// <returns>The collection of flow entity versions.</returns>
        [HttpGet("{id}/versions")]
        public ActionResult<IEnumerable<IFlowEntity>> GetVersions(string id)
        {
            try
            {
                var versions = _flowEntityManager.GetFlowVersions(id);
                if (versions == null || !versions.Any())
                {
                    return NotFound($"No versions found for flow with ID '{id}'");
                }

                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting versions for flow entity with ID {FlowId}", id);
                return StatusCode(500, $"An error occurred while retrieving versions for flow entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a flow entity.
        /// </summary>
        /// <param name="flow">The flow entity to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] IFlowEntity flow)
        {
            try
            {
                var result = _flowEntityManager.RegisterService(flow);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering flow entity");
                return StatusCode(500, "An error occurred while registering the flow entity");
            }
        }

        /// <summary>
        /// Creates a new version of a flow entity.
        /// </summary>
        /// <param name="id">The flow identifier.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <returns>The new flow entity version.</returns>
        [HttpPost("{id}/versions")]
        public ActionResult<IFlowEntity> CreateVersion(string id, [FromBody] Models.VersionInfo versionInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(versionInfo.Description))
                {
                    return BadRequest("Version description is required");
                }

                var newVersion = _flowEntityManager.CreateNewVersion(id, versionInfo.Description);
                var result = _flowEntityManager.RegisterService(newVersion);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, newVersion);
                }

                return BadRequest(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating new version for flow entity with ID {FlowId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version for flow entity with ID {FlowId}", id);
                return StatusCode(500, $"An error occurred while creating a new version for flow entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Validates a flow entity.
        /// </summary>
        /// <param name="id">The flow identifier.</param>
        /// <returns>The validation result.</returns>
        [HttpGet("{id}/validate")]
        public ActionResult<Common.Validation.ValidationResult> Validate(string id)
        {
            try
            {
                var result = _flowEntityManager.ValidateFlow(id);
                if (!result.IsValid)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating flow entity with ID {FlowId}", id);
                return StatusCode(500, $"An error occurred while validating flow entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Unregisters a flow entity.
        /// </summary>
        /// <param name="id">The identifier of the flow entity to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _flowEntityManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering flow entity with ID {FlowId}", id);
                return StatusCode(500, $"An error occurred while unregistering flow entity with ID '{id}'");
            }
        }
    }
}
