using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing source entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SourceEntityController : ControllerBase
    {
        private readonly SourceEntityManager _sourceEntityManager;
        private readonly ILogger<SourceEntityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceEntityController"/> class.
        /// </summary>
        /// <param name="sourceEntityManager">The source entity manager.</param>
        /// <param name="logger">The logger instance.</param>
        public SourceEntityController(SourceEntityManager sourceEntityManager, ILogger<SourceEntityController> logger)
        {
            _sourceEntityManager = sourceEntityManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered source entities.
        /// </summary>
        /// <returns>The collection of registered source entities.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ISourceEntity>> GetAll()
        {
            try
            {
                var entities = _sourceEntityManager.GetAllServices();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all source entities");
                return StatusCode(500, "An error occurred while retrieving source entities");
            }
        }

        /// <summary>
        /// Gets a source entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The source entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<ISourceEntity> Get(string id)
        {
            try
            {
                var entity = _sourceEntityManager.GetService(id);
                if (entity == null)
                {
                    return NotFound($"Source entity with ID '{id}' not found");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting source entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while retrieving source entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a source entity.
        /// </summary>
        /// <param name="entity">The source entity to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] ISourceEntity entity)
        {
            try
            {
                var result = _sourceEntityManager.RegisterService(entity);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering source entity");
                return StatusCode(500, "An error occurred while registering the source entity");
            }
        }

        /// <summary>
        /// Unregisters a source entity.
        /// </summary>
        /// <param name="id">The identifier of the entity to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _sourceEntityManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering source entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while unregistering source entity with ID '{id}'");
            }
        }
    }
}
