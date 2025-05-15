using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing processor services.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessorServiceController : ControllerBase
    {
        private readonly ProcessorServiceManager _processorServiceManager;
        private readonly ILogger<ProcessorServiceController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorServiceController"/> class.
        /// </summary>
        /// <param name="processorServiceManager">The processor service manager.</param>
        /// <param name="logger">The logger instance.</param>
        public ProcessorServiceController(ProcessorServiceManager processorServiceManager, ILogger<ProcessorServiceController> logger)
        {
            _processorServiceManager = processorServiceManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered processor services.
        /// </summary>
        /// <returns>The collection of registered processor services.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IProcessorService>> GetAll()
        {
            try
            {
                var services = _processorServiceManager.GetAllServices();
                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all processor services");
                return StatusCode(500, "An error occurred while retrieving processor services");
            }
        }

        /// <summary>
        /// Gets a processor service by its identifier.
        /// </summary>
        /// <param name="id">The service identifier.</param>
        /// <returns>The processor service if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<IProcessorService> Get(string id)
        {
            try
            {
                var service = _processorServiceManager.GetService(id);
                if (service == null)
                {
                    return NotFound($"Processor service with ID '{id}' not found");
                }

                return Ok(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting processor service with ID {ServiceId}", id);
                return StatusCode(500, $"An error occurred while retrieving processor service with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers a processor service.
        /// </summary>
        /// <param name="service">The processor service to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] IProcessorService service)
        {
            try
            {
                var result = _processorServiceManager.RegisterService(service);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering processor service");
                return StatusCode(500, "An error occurred while registering the processor service");
            }
        }

        /// <summary>
        /// Unregisters a processor service.
        /// </summary>
        /// <param name="id">The identifier of the service to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _processorServiceManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering processor service with ID {ServiceId}", id);
                return StatusCode(500, $"An error occurred while unregistering processor service with ID '{id}'");
            }
        }
    }
}
