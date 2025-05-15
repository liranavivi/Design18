using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing exporter services.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExporterServiceController : ControllerBase
    {
        private readonly ExporterServiceManager _exporterServiceManager;
        private readonly ILogger<ExporterServiceController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceController"/> class.
        /// </summary>
        /// <param name="exporterServiceManager">The exporter service manager.</param>
        /// <param name="logger">The logger instance.</param>
        public ExporterServiceController(ExporterServiceManager exporterServiceManager, ILogger<ExporterServiceController> logger)
        {
            _exporterServiceManager = exporterServiceManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered exporter services.
        /// </summary>
        /// <returns>The collection of registered exporter services.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IExporterService>> GetAll()
        {
            try
            {
                var services = _exporterServiceManager.GetAllServices();
                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all exporter services");
                return StatusCode(500, "An error occurred while retrieving exporter services");
            }
        }

        /// <summary>
        /// Gets an exporter service by its identifier.
        /// </summary>
        /// <param name="id">The service identifier.</param>
        /// <returns>The exporter service if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<IExporterService> Get(string id)
        {
            try
            {
                var service = _exporterServiceManager.GetService(id);
                if (service == null)
                {
                    return NotFound($"Exporter service with ID '{id}' not found");
                }

                return Ok(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exporter service with ID {ServiceId}", id);
                return StatusCode(500, $"An error occurred while retrieving exporter service with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers an exporter service.
        /// </summary>
        /// <param name="service">The exporter service to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] IExporterService service)
        {
            try
            {
                var result = _exporterServiceManager.RegisterService(service);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering exporter service");
                return StatusCode(500, "An error occurred while registering the exporter service");
            }
        }

        /// <summary>
        /// Unregisters an exporter service.
        /// </summary>
        /// <param name="id">The identifier of the service to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _exporterServiceManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering exporter service with ID {ServiceId}", id);
                return StatusCode(500, $"An error occurred while unregistering exporter service with ID '{id}'");
            }
        }
    }
}
