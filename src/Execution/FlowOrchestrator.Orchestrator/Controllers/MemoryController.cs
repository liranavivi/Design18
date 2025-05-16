using FlowOrchestrator.Orchestrator.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Orchestrator.Controllers
{
    /// <summary>
    /// Controller for memory addressing operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly ILogger<MemoryController> _logger;
        private readonly MemoryAddressService _memoryAddressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="memoryAddressService">The memory address service.</param>
        public MemoryController(
            ILogger<MemoryController> logger,
            MemoryAddressService memoryAddressService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memoryAddressService = memoryAddressService ?? throw new ArgumentNullException(nameof(memoryAddressService));
        }

        /// <summary>
        /// Generates a memory address.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="stepType">The step type.</param>
        /// <param name="branchPath">The branch path.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <param name="dataType">The data type.</param>
        /// <param name="additionalInfo">Additional information to include in the address.</param>
        /// <returns>The memory address.</returns>
        [HttpGet("address")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GenerateMemoryAddress(
            [FromQuery] string executionId,
            [FromQuery] string flowId,
            [FromQuery] string stepType,
            [FromQuery] string branchPath,
            [FromQuery] string stepId,
            [FromQuery] string dataType,
            [FromQuery] string? additionalInfo = null)
        {
            try
            {
                _logger.LogInformation("Received request to generate memory address for execution {ExecutionId}, flow {FlowId}, step type {StepType}, branch {BranchPath}, step {StepId}, data type {DataType}",
                    executionId, flowId, stepType, branchPath, stepId, dataType);

                var address = _memoryAddressService.GenerateMemoryAddress(
                    executionId,
                    flowId,
                    stepType,
                    branchPath,
                    stepId,
                    dataType,
                    additionalInfo);

                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating memory address");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Parses a memory address.
        /// </summary>
        /// <param name="address">The memory address.</param>
        /// <returns>The address components.</returns>
        [HttpGet("parse")]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ParseMemoryAddress([FromQuery] string address)
        {
            try
            {
                _logger.LogInformation("Received request to parse memory address {Address}", address);

                var components = _memoryAddressService.ParseMemoryAddress(address);
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing memory address {Address}", address);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
