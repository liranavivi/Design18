using FlowOrchestrator.Common.Utilities;
using FlowOrchestrator.Observability.Statistics.Models;
using FlowOrchestrator.Observability.Statistics.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Observability.Statistics.Controllers
{
    /// <summary>
    /// Controller for historical data operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HistoricalDataController : ControllerBase
    {
        private readonly ILogger<HistoricalDataController> _logger;
        private readonly HistoricalDataService _historicalDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalDataController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="historicalDataService">The historical data service.</param>
        public HistoricalDataController(
            ILogger<HistoricalDataController> logger,
            HistoricalDataService historicalDataService)
        {
            _logger = logger;
            _historicalDataService = historicalDataService;
        }

        /// <summary>
        /// Queries historical data records.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The matching records.</returns>
        [HttpPost("query")]
        [ProducesResponseType(typeof(List<HistoricalDataRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult QueryRecords([FromBody] MetricQuery query)
        {
            try
            {
                Guard.AgainstNull(query, nameof(query));
                
                var records = _historicalDataService.QueryRecords(query);
                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying historical data records");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Stores a historical data record.
        /// </summary>
        /// <param name="record">The record to store.</param>
        /// <returns>A success response.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult StoreRecord([FromBody] HistoricalDataRecord record)
        {
            try
            {
                Guard.AgainstNull(record, nameof(record));
                
                _historicalDataService.StoreRecord(record);
                return Ok(new { success = true, message = "Historical data record stored", recordId = record.RecordId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing historical data record");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Stores multiple historical data records.
        /// </summary>
        /// <param name="records">The records to store.</param>
        /// <returns>A success response.</returns>
        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult StoreRecords([FromBody] List<HistoricalDataRecord> records)
        {
            try
            {
                Guard.AgainstNull(records, nameof(records));
                
                _historicalDataService.StoreRecords(records);
                return Ok(new { success = true, message = $"{records.Count} historical data records stored" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing historical data records");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Archives records older than the specified date.
        /// </summary>
        /// <param name="olderThan">The date threshold.</param>
        /// <returns>The number of records archived.</returns>
        [HttpPost("archive")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult ArchiveRecords([FromQuery] DateTime olderThan)
        {
            try
            {
                var count = _historicalDataService.ArchiveRecords(olderThan);
                return Ok(new { success = true, message = $"{count} historical data records archived", count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving historical data records");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes records older than the specified date.
        /// </summary>
        /// <param name="olderThan">The date threshold.</param>
        /// <returns>The number of records deleted.</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult DeleteRecords([FromQuery] DateTime olderThan)
        {
            try
            {
                var count = _historicalDataService.DeleteRecords(olderThan);
                return Ok(new { success = true, message = $"{count} historical data records deleted", count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting historical data records");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets statistics about the historical data.
        /// </summary>
        /// <returns>The statistics.</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GetStatistics()
        {
            try
            {
                var stats = new
                {
                    TotalRecords = _historicalDataService.GetTotalRecordCount(),
                    ArchivedRecords = _historicalDataService.GetArchivedRecordCount(),
                    OldestRecordTimestamp = _historicalDataService.GetOldestRecordTimestamp(),
                    NewestRecordTimestamp = _historicalDataService.GetNewestRecordTimestamp()
                };
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historical data statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
