using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing destination assignment entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DestinationAssignmentEntityController : ControllerBase
    {
        private readonly DestinationAssignmentEntityManager _destinationAssignmentEntityManager;
        private readonly ILogger<DestinationAssignmentEntityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationAssignmentEntityController"/> class.
        /// </summary>
        /// <param name="destinationAssignmentEntityManager">The destination assignment entity manager.</param>
        /// <param name="logger">The logger instance.</param>
        public DestinationAssignmentEntityController(DestinationAssignmentEntityManager destinationAssignmentEntityManager, ILogger<DestinationAssignmentEntityController> logger)
        {
            _destinationAssignmentEntityManager = destinationAssignmentEntityManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered destination assignment entities.
        /// </summary>
        /// <returns>The collection of registered destination assignment entities.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IDestinationAssignmentEntity>> GetAll()
        {
            try
            {
                var entities = _destinationAssignmentEntityManager.GetAllServices();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all destination assignment entities");
                return StatusCode(500, "An error occurred while retrieving destination assignment entities");
            }
        }

        /// <summary>
        /// Gets a destination assignment entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The destination assignment entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<IDestinationAssignmentEntity> Get(string id)
        {
            try
            {
                var entity = _destinationAssignmentEntityManager.GetService(id);
                if (entity == null)
                {
                    return NotFound($"Destination assignment entity with ID '{id}' not found");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting destination assignment entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while retrieving destination assignment entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a destination assignment entity.
        /// </summary>
        /// <param name="entity">The destination assignment entity to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] IDestinationAssignmentEntity entity)
        {
            try
            {
                var result = _destinationAssignmentEntityManager.RegisterService(entity);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering destination assignment entity");
                return StatusCode(500, "An error occurred while registering the destination assignment entity");
            }
        }

        /// <summary>
        /// Unregisters a destination assignment entity.
        /// </summary>
        /// <param name="id">The identifier of the entity to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _destinationAssignmentEntityManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering destination assignment entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while unregistering destination assignment entity with ID '{id}'");
            }
        }
    }
}
