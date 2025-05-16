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
        public async Task<ActionResult<IEnumerable<IImporterService>>> GetAll()
        {
            try
            {
                var services = await _importerServiceManager.GetAllServicesAsync();
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
        public async Task<ActionResult<IImporterService>> Get(string id)
        {
            try
            {
                var service = await _importerServiceManager.GetServiceByIdAsync(id);
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
        public async Task<ActionResult<ServiceRegistrationResult>> Register([FromBody] IImporterService service)
        {
            try
            {
                var registeredService = await _importerServiceManager.RegisterServiceAsync(service);
                var result = new ServiceRegistrationResult
                {
                    Success = true,
                    ServiceId = registeredService.ServiceId
                };

                return CreatedAtAction(nameof(Get), new { id = result.ServiceId }, result);
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
        public async Task<ActionResult<ServiceUnregistrationResult>> Unregister(string id)
        {
            try
            {
                var success = await _importerServiceManager.DeregisterServiceAsync(id);
                var result = new ServiceUnregistrationResult
                {
                    Success = success,
                    ServiceId = id,
                    ErrorMessage = success ? null : "Service not found"
                };

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
