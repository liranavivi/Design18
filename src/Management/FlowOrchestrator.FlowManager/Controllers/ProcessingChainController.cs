using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Management.Flows.Models;
using FlowOrchestrator.Management.Flows.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Flows.Controllers
{
    /// <summary>
    /// Controller for managing processing chain entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingChainController : ControllerBase
    {
        private readonly ProcessingChainManager _processingChainManager;
        private readonly ILogger<ProcessingChainController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingChainController"/> class.
        /// </summary>
        /// <param name="processingChainManager">The processing chain manager.</param>
        /// <param name="logger">The logger instance.</param>
        public ProcessingChainController(ProcessingChainManager processingChainManager, ILogger<ProcessingChainController> logger)
        {
            _processingChainManager = processingChainManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered processing chain entities.
        /// </summary>
        /// <returns>The collection of registered processing chain entities.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IProcessingChainEntity>> GetAll()
        {
            try
            {
                var chains = _processingChainManager.GetAllProcessingChains();
                return Ok(chains);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all processing chain entities");
                return StatusCode(500, "An error occurred while retrieving processing chain entities");
            }
        }

        /// <summary>
        /// Gets a processing chain entity by its identifier.
        /// </summary>
        /// <param name="id">The chain identifier.</param>
        /// <returns>The processing chain entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<IProcessingChainEntity> Get(string id)
        {
            try
            {
                var chain = _processingChainManager.GetProcessingChain(id);
                if (chain == null)
                {
                    return NotFound($"Processing chain with ID '{id}' not found");
                }

                return Ok(chain);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting processing chain entity with ID {ChainId}", id);
                return StatusCode(500, $"An error occurred while retrieving processing chain entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Gets all versions of a processing chain entity.
        /// </summary>
        /// <param name="id">The chain identifier.</param>
        /// <returns>The collection of processing chain entity versions.</returns>
        [HttpGet("{id}/versions")]
        public ActionResult<IEnumerable<IProcessingChainEntity>> GetVersions(string id)
        {
            try
            {
                var versions = _processingChainManager.GetProcessingChainVersions(id);
                if (versions == null || !versions.Any())
                {
                    return NotFound($"No versions found for processing chain with ID '{id}'");
                }

                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting versions for processing chain entity with ID {ChainId}", id);
                return StatusCode(500, $"An error occurred while retrieving versions for processing chain entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a processing chain entity.
        /// </summary>
        /// <param name="chain">The processing chain entity to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] IProcessingChainEntity chain)
        {
            try
            {
                var result = _processingChainManager.RegisterService(chain);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering processing chain entity");
                return StatusCode(500, "An error occurred while registering the processing chain entity");
            }
        }

        /// <summary>
        /// Creates a new version of a processing chain entity.
        /// </summary>
        /// <param name="id">The chain identifier.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <returns>The new processing chain entity version.</returns>
        [HttpPost("{id}/versions")]
        public ActionResult<IProcessingChainEntity> CreateVersion(string id, [FromBody] Models.VersionInfo versionInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(versionInfo.Description))
                {
                    return BadRequest("Version description is required");
                }

                var newVersion = _processingChainManager.CreateNewVersion(id, versionInfo.Description);
                var result = _processingChainManager.RegisterService(newVersion);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, newVersion);
                }

                return BadRequest(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating new version for processing chain entity with ID {ChainId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version for processing chain entity with ID {ChainId}", id);
                return StatusCode(500, $"An error occurred while creating a new version for processing chain entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Unregisters a processing chain entity.
        /// </summary>
        /// <param name="id">The identifier of the processing chain entity to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _processingChainManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering processing chain entity with ID {ChainId}", id);
                return StatusCode(500, $"An error occurred while unregistering processing chain entity with ID '{id}'");
            }
        }
    }
}
