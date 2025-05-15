using FlowOrchestrator.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Management.Services.Controllers
{
    /// <summary>
    /// Controller for managing importer services.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ImporterServiceController : ControllerBase
    {
        private readonly ImporterServiceManager _importerServiceManager;
        private readonly ILogger<ImporterServiceController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceController"/> class.
        /// </summary>
        /// <param name="importerServiceManager">The importer service manager.</param>
        /// <param name="logger">The logger instance.</param>
        public ImporterServiceController(ImporterServiceManager importerServiceManager, ILogger<ImporterServiceController> logger)
        {
            _importerServiceManager = importerServiceManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets all registered importer services.
        /// </summary>
        /// <returns>The collection of registered importer services.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IImporterService>> GetAll()
        {
            try
            {
                var services = _importerServiceManager.GetAllServices();
                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all importer services");
                return StatusCode(500, "An error occurred while retrieving importer services");
            }
        }

        /// <summary>
        /// Gets an importer service by its identifier.
        /// </summary>
        /// <param name="id">The service identifier.</param>
        /// <returns>The importer service if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public ActionResult<IImporterService> Get(string id)
        {
            try
            {
                var service = _importerServiceManager.GetService(id);
                if (service == null)
                {
                    return NotFound($"Importer service with ID '{id}' not found");
                }

                return Ok(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting importer service with ID {ServiceId}", id);
                return StatusCode(500, $"An error occurred while retrieving importer service with ID '{id}'");
            }
        }

        /// <summary>
        /// Registers an importer service.
        /// </summary>
        /// <param name="service">The importer service to register.</param>
        /// <returns>The result of the registration.</returns>
        [HttpPost]
        public ActionResult<ServiceRegistrationResult> Register([FromBody] IImporterService service)
        {
            try
            {
                var result = _importerServiceManager.RegisterService(service);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering importer service");
                return StatusCode(500, "An error occurred while registering the importer service");
            }
        }

        /// <summary>
        /// Unregisters an importer service.
        /// </summary>
        /// <param name="id">The identifier of the service to unregister.</param>
        /// <returns>The result of the unregistration.</returns>
        [HttpDelete("{id}")]
        public ActionResult<ServiceUnregistrationResult> Unregister(string id)
        {
            try
            {
                var result = _importerServiceManager.UnregisterService(id);
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering importer service with ID {ServiceId}", id);
                return StatusCode(500, $"An error occurred while unregistering importer service with ID '{id}'");
            }
        }
    }
}
