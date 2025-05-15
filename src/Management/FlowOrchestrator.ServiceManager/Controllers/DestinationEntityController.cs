using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing destination entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DestinationEntityController : ControllerBase
    {
        private readonly DestinationEntityManager _destinationEntityManager;
        private readonly ILogger<DestinationEntityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationEntityController"/> class.
        /// </summary>
        /// <param name="destinationEntityManager">The destination entity manager.</param>
        /// <param name="logger">The logger instance.</param>
        public DestinationEntityController(DestinationEntityManager destinationEntityManager, ILogger<DestinationEntityController> logger)
        {
            _destinationEntityManager = destinationEntityManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered destination entities.
        /// </summary>
        /// <returns>The collection of registered destination entities.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IDestinationEntity>> GetAll()
        {
            try
            {
                var entities = _destinationEntityManager.GetAllServices();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all destination entities");
                return StatusCode(500, "An error occurred while retrieving destination entities");
            }
        }

        /// <summary>
        /// Gets a destination entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The destination entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<IDestinationEntity> Get(string id)
        {
            try
            {
                var entity = _destinationEntityManager.GetService(id);
                if (entity == null)
                {
                    return NotFound($"Destination entity with ID '{id}' not found");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting destination entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while retrieving destination entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a destination entity.
        /// </summary>
        /// <param name="entity">The destination entity to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] IDestinationEntity entity)
        {
            try
            {
                var result = _destinationEntityManager.RegisterService(entity);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering destination entity");
                return StatusCode(500, "An error occurred while registering the destination entity");
            }
        }

        /// <summary>
        /// Unregisters a destination entity.
        /// </summary>
        /// <param name="id">The identifier of the entity to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _destinationEntityManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering destination entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while unregistering destination entity with ID '{id}'");
            }
        }
    }
}
