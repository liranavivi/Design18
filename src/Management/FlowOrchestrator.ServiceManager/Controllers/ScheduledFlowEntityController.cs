using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing scheduled flow entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduledFlowEntityController : ControllerBase
    {
        private readonly ScheduledFlowEntityManager _scheduledFlowEntityManager;
        private readonly ILogger<ScheduledFlowEntityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledFlowEntityController"/> class.
        /// </summary>
        /// <param name="scheduledFlowEntityManager">The scheduled flow entity manager.</param>
        /// <param name="logger">The logger instance.</param>
        public ScheduledFlowEntityController(ScheduledFlowEntityManager scheduledFlowEntityManager, ILogger<ScheduledFlowEntityController> logger)
        {
            _scheduledFlowEntityManager = scheduledFlowEntityManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered scheduled flow entities.
        /// </summary>
        /// <returns>The collection of registered scheduled flow entities.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IScheduledFlowEntity>> GetAll()
        {
            try
            {
                var entities = _scheduledFlowEntityManager.GetAllServices();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all scheduled flow entities");
                return StatusCode(500, "An error occurred while retrieving scheduled flow entities");
            }
        }

        /// <summary>
        /// Gets a scheduled flow entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The scheduled flow entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<IScheduledFlowEntity> Get(string id)
        {
            try
            {
                var entity = _scheduledFlowEntityManager.GetService(id);
                if (entity == null)
                {
                    return NotFound($"Scheduled flow entity with ID '{id}' not found");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scheduled flow entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while retrieving scheduled flow entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a scheduled flow entity.
        /// </summary>
        /// <param name="entity">The scheduled flow entity to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] IScheduledFlowEntity entity)
        {
            try
            {
                var result = _scheduledFlowEntityManager.RegisterService(entity);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering scheduled flow entity");
                return StatusCode(500, "An error occurred while registering the scheduled flow entity");
            }
        }

        /// <summary>
        /// Unregisters a scheduled flow entity.
        /// </summary>
        /// <param name="id">The identifier of the entity to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _scheduledFlowEntityManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering scheduled flow entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while unregistering scheduled flow entity with ID '{id}'");
            }
        }
    }
}
