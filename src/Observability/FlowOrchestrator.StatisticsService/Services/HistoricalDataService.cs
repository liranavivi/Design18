using FlowOrchestrator.Common.Utilities;
using FlowOrchestrator.Observability.Statistics.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Statistics.Services
{
    /// <summary>
    /// Service for managing historical statistics data.
    /// </summary>
    public class HistoricalDataService
    {
        private readonly ILogger<HistoricalDataService> _logger;
        private readonly StatisticsOptions _options;
        private readonly List<HistoricalDataRecord> _records = new();
        private readonly object _lock = new();
        private Timer? _cleanupTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalDataService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The options.</param>
        public HistoricalDataService(
            ILogger<HistoricalDataService> logger,
            IOptions<StatisticsOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            
            // Start the cleanup timer
            _cleanupTimer = new Timer(CleanupExpiredRecords, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        }

        /// <summary>
        /// Stores a historical data record.
        /// </summary>
        /// <param name="record">The record to store.</param>
        public void StoreRecord(HistoricalDataRecord record)
        {
            Guard.AgainstNull(record, nameof(record));

            try
            {
                lock (_lock)
                {
                    _records.Add(record);
                }
                
                _logger.LogDebug("Stored historical data record: {RecordId}, Metric: {MetricName}", 
                    record.RecordId, record.MetricName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing historical data record");
            }
        }

        /// <summary>
        /// Stores multiple historical data records.
        /// </summary>
        /// <param name="records">The records to store.</param>
        public void StoreRecords(IEnumerable<HistoricalDataRecord> records)
        {
            Guard.AgainstNull(records, nameof(records));

            try
            {
                lock (_lock)
                {
                    _records.AddRange(records);
                }
                
                _logger.LogDebug("Stored {Count} historical data records", records.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing historical data records");
            }
        }

        /// <summary>
        /// Queries historical data records based on the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The matching records.</returns>
        public List<HistoricalDataRecord> QueryRecords(MetricQuery query)
        {
            Guard.AgainstNull(query, nameof(query));

            try
            {
                lock (_lock)
                {
                    var result = _records
                        .Where(r => (query.MetricNames.Count == 0 || query.MetricNames.Contains(r.MetricName)) &&
                                   r.Timestamp >= query.StartTime &&
                                   r.Timestamp <= query.EndTime)
                        .ToList();
                    
                    // Apply attribute filters
                    if (query.FilterAttributes.Count > 0)
                    {
                        result = result.Where(r => 
                        {
                            foreach (var filter in query.FilterAttributes)
                            {
                                if (!r.ContextAttributes.TryGetValue(filter.Key, out var value) || value != filter.Value)
                                {
                                    return false;
                                }
                            }
                            return true;
                        }).ToList();
                    }
                    
                    // Apply max results limit
                    if (query.MaxResults.HasValue)
                    {
                        result = result.Take(query.MaxResults.Value).ToList();
                    }
                    
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying historical data records");
                return new List<HistoricalDataRecord>();
            }
        }

        /// <summary>
        /// Archives records older than the specified date.
        /// </summary>
        /// <param name="olderThan">The date threshold.</param>
        /// <returns>The number of records archived.</returns>
        public int ArchiveRecords(DateTime olderThan)
        {
            try
            {
                int count = 0;
                
                lock (_lock)
                {
                    foreach (var record in _records.Where(r => !r.IsArchived && r.Timestamp < olderThan))
                    {
                        record.IsArchived = true;
                        record.ArchiveDate = DateTime.UtcNow;
                        count++;
                    }
                }
                
                _logger.LogInformation("Archived {Count} historical data records older than {Date}", 
                    count, olderThan);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving historical data records");
                return 0;
            }
        }

        /// <summary>
        /// Deletes records older than the specified date.
        /// </summary>
        /// <param name="olderThan">The date threshold.</param>
        /// <returns>The number of records deleted.</returns>
        public int DeleteRecords(DateTime olderThan)
        {
            try
            {
                int count;
                
                lock (_lock)
                {
                    var recordsToDelete = _records.Where(r => r.Timestamp < olderThan).ToList();
                    count = recordsToDelete.Count;
                    
                    foreach (var record in recordsToDelete)
                    {
                        _records.Remove(record);
                    }
                }
                
                _logger.LogInformation("Deleted {Count} historical data records older than {Date}", 
                    count, olderThan);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting historical data records");
                return 0;
            }
        }

        /// <summary>
        /// Gets the total number of records.
        /// </summary>
        /// <returns>The total number of records.</returns>
        public int GetTotalRecordCount()
        {
            lock (_lock)
            {
                return _records.Count;
            }
        }

        /// <summary>
        /// Gets the number of archived records.
        /// </summary>
        /// <returns>The number of archived records.</returns>
        public int GetArchivedRecordCount()
        {
            lock (_lock)
            {
                return _records.Count(r => r.IsArchived);
            }
        }

        /// <summary>
        /// Gets the oldest record timestamp.
        /// </summary>
        /// <returns>The oldest record timestamp, or null if no records exist.</returns>
        public DateTime? GetOldestRecordTimestamp()
        {
            lock (_lock)
            {
                return _records.Count > 0 ? _records.Min(r => r.Timestamp) : null;
            }
        }

        /// <summary>
        /// Gets the newest record timestamp.
        /// </summary>
        /// <returns>The newest record timestamp, or null if no records exist.</returns>
        public DateTime? GetNewestRecordTimestamp()
        {
            lock (_lock)
            {
                return _records.Count > 0 ? _records.Max(r => r.Timestamp) : null;
            }
        }

        private void CleanupExpiredRecords(object? state)
        {
            try
            {
                _logger.LogInformation("Cleaning up expired historical data records");
                
                int count;
                
                lock (_lock)
                {
                    var now = DateTime.UtcNow;
                    var recordsToDelete = _records.Where(r => r.ExpirationDate < now).ToList();
                    count = recordsToDelete.Count;
                    
                    foreach (var record in recordsToDelete)
                    {
                        _records.Remove(record);
                    }
                }
                
                _logger.LogInformation("Cleaned up {Count} expired historical data records", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired historical data records");
            }
        }
    }
}
