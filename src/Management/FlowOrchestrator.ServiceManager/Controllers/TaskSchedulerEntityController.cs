using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing task scheduler entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TaskSchedulerEntityController : ControllerBase
    {
        private readonly TaskSchedulerEntityManager _taskSchedulerEntityManager;
        private readonly ILogger<TaskSchedulerEntityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerEntityController"/> class.
        /// </summary>
        /// <param name="taskSchedulerEntityManager">The task scheduler entity manager.</param>
        /// <param name="logger">The logger instance.</param>
        public TaskSchedulerEntityController(TaskSchedulerEntityManager taskSchedulerEntityManager, ILogger<TaskSchedulerEntityController> logger)
        {
            _taskSchedulerEntityManager = taskSchedulerEntityManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered task scheduler entities.
        /// </summary>
        /// <returns>The collection of registered task scheduler entities.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ITaskSchedulerEntity>> GetAll()
        {
            try
            {
                var entities = _taskSchedulerEntityManager.GetAllServices();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all task scheduler entities");
                return StatusCode(500, "An error occurred while retrieving task scheduler entities");
            }
        }

        /// <summary>
        /// Gets a task scheduler entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The task scheduler entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<ITaskSchedulerEntity> Get(string id)
        {
            try
            {
                var entity = _taskSchedulerEntityManager.GetService(id);
                if (entity == null)
                {
                    return NotFound($"Task scheduler entity with ID '{id}' not found");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task scheduler entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while retrieving task scheduler entity with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a task scheduler entity.
        /// </summary>
        /// <param name="entity">The task scheduler entity to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] ITaskSchedulerEntity entity)
        {
            try
            {
                var result = _taskSchedulerEntityManager.RegisterService(entity);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering task scheduler entity");
                return StatusCode(500, "An error occurred while registering the task scheduler entity");
            }
        }

        /// <summary>
        /// Unregisters a task scheduler entity.
        /// </summary>
        /// <param name="id">The identifier of the entity to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _taskSchedulerEntityManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering task scheduler entity with ID {EntityId}", id);
                return StatusCode(500, $"An error occurred while unregistering task scheduler entity with ID '{id}'");
            }
        }
    }
}
