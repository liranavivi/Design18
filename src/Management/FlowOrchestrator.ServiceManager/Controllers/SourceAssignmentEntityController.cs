using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing source assignment entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SourceAssignmentEntityController : ControllerBase
    {
        private readonly SourceAssignmentEntityManager _sourceAssignmentEntityManager;
        private readonly ILogger<SourceAssignmentEntityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAssignmentEntityController"/> class.
        /// </summary>
        /// <param name="sourceAssignmentEntityManager">The source assignment entity manager.</param>
        /// <param name="logger">The logger instance.</param>
        public SourceAssignmentEntityController(SourceAssignmentEntityManager sourceAssignmentEntityManager, ILogger<SourceAssignmentEntityController> logger)
        {
            _sourceAssignmentEntityManager = sourceAssignmentEntityManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered source assignment entities.
        /// </summary>
        /// <returns>The collection of registered source assignment entities.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ISourceAssignmentEntity>> GetAll()
        {
            try
            {
                var entities = _sourceAssignmentEntityManager.GetAllServices();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all source assignment entities");
                return StatusCode(500, "An error occurred while retrieving source assignment entities");
            }
        }

        /// <summary>
        /// Gets a source assignment entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The source assignment entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<ISourceAssignmentEntity> Get(string id)
        {
            try
            {
                var entity = _sourceAssignmentEntityManager.GetService(id);
                if (entity == null)
                {
                    return NotFound($"Source assignment entity with ID '{id}' not found");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting source assignment entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while retrieving source assignment entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a source assignment entity.
        /// </summary>
        /// <param name="entity">The source assignment entity to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] ISourceAssignmentEntity entity)
        {
            try
            {
                var result = _sourceAssignmentEntityManager.RegisterService(entity);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering source assignment entity");
                return StatusCode(500, "An error occurred while registering the source assignment entity");
            }
        }

        /// <summary>
        /// Unregisters a source assignment entity.
        /// </summary>
        /// <param name="id">The identifier of the entity to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _sourceAssignmentEntityManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering source assignment entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while unregistering source assignment entity with ID '{id}'");
            }
        }
    }
}
