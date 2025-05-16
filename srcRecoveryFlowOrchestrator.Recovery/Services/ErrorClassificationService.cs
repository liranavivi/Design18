using FlowOrchestrator.Common.Exceptions;
using FlowOrchestrator.Recovery.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FlowOrchestrator.Recovery.Services
{
    /// <summary>
    /// Service for classifying errors and tracking error patterns.
    /// </summary>
    public class ErrorClassificationService
    {
        private readonly ILogger<ErrorClassificationService> _logger;
        private readonly ConcurrentDictionary<string, ErrorPattern> _errorPatterns = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorClassificationService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ErrorClassificationService(ILogger<ErrorClassificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Classifies an error based on its context.
        /// </summary>
        /// <param name="errorContext">The error context.</param>
        /// <returns>The error classification.</returns>
        public ErrorClassification ClassifyError(ErrorContext errorContext)
        {
            // If the error already has a classification, use it
            if (errorContext.Classification != ErrorClassification.GENERAL_ERROR)
            {
                return errorContext.Classification;
            }

            // Otherwise, classify based on the error code
            return errorContext.ErrorCode switch
            {
                var code when code.Contains("TIMEOUT", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.CONNECTION_TIMEOUT,
                
                var code when code.Contains("CONNECTION", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.CONNECTION_ERROR,
                
                var code when code.Contains("AUTH", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.AUTHENTICATION_ERROR,
                
                var code when code.Contains("PERMISSION", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.AUTHENTICATION_INSUFFICIENT_PERMISSIONS,
                
                var code when code.Contains("DATA", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.DATA_ERROR,
                
                var code when code.Contains("SCHEMA", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.DATA_SCHEMA_VIOLATION,
                
                var code when code.Contains("FORMAT", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.DATA_INVALID_FORMAT,
                
                var code when code.Contains("RESOURCE", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.RESOURCE_ERROR,
                
                var code when code.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.RESOURCE_NOT_FOUND,
                
                var code when code.Contains("UNAVAILABLE", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.RESOURCE_UNAVAILABLE,
                
                var code when code.Contains("QUOTA", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.RESOURCE_QUOTA_EXCEEDED,
                
                var code when code.Contains("PROCESSING", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.PROCESSING_ERROR,
                
                var code when code.Contains("VALIDATION", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.VALIDATION_ERROR,
                
                var code when code.Contains("CONFIG", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.CONFIGURATION_ERROR,
                
                var code when code.Contains("VERSION", StringComparison.OrdinalIgnoreCase) => 
                    ErrorClassification.VERSION_COMPATIBILITY_ERROR,
                
                _ => ErrorClassification.GENERAL_ERROR
            };
        }

        /// <summary>
        /// Determines the severity of an error based on its classification.
        /// </summary>
        /// <param name="classification">The error classification.</param>
        /// <returns>The error severity.</returns>
        public ErrorSeverity DetermineSeverity(ErrorClassification classification)
        {
            return classification switch
            {
                ErrorClassification.CONNECTION_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.CONNECTION_TIMEOUT => ErrorSeverity.MAJOR,
                ErrorClassification.CONNECTION_UNREACHABLE => ErrorSeverity.MAJOR,
                ErrorClassification.CONNECTION_HANDSHAKE_FAILURE => ErrorSeverity.MAJOR,
                ErrorClassification.AUTHENTICATION_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.AUTHENTICATION_INVALID_CREDENTIALS => ErrorSeverity.MAJOR,
                ErrorClassification.AUTHENTICATION_EXPIRED_TOKEN => ErrorSeverity.MAJOR,
                ErrorClassification.AUTHENTICATION_INSUFFICIENT_PERMISSIONS => ErrorSeverity.MAJOR,
                ErrorClassification.DATA_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.DATA_INVALID_FORMAT => ErrorSeverity.MAJOR,
                ErrorClassification.DATA_SCHEMA_VIOLATION => ErrorSeverity.MAJOR,
                ErrorClassification.DATA_CORRUPTION => ErrorSeverity.CRITICAL,
                ErrorClassification.RESOURCE_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.RESOURCE_NOT_FOUND => ErrorSeverity.MAJOR,
                ErrorClassification.RESOURCE_UNAVAILABLE => ErrorSeverity.MAJOR,
                ErrorClassification.RESOURCE_QUOTA_EXCEEDED => ErrorSeverity.MAJOR,
                ErrorClassification.PROCESSING_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.VALIDATION_ERROR => ErrorSeverity.MINOR,
                ErrorClassification.CONFIGURATION_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.VERSION_COMPATIBILITY_ERROR => ErrorSeverity.CRITICAL,
                ErrorClassification.GENERAL_ERROR => ErrorSeverity.MAJOR,
                _ => ErrorSeverity.MAJOR
            };
        }

        /// <summary>
        /// Tracks an error occurrence and updates error patterns.
        /// </summary>
        /// <param name="errorContext">The error context.</param>
        public void TrackError(ErrorContext errorContext)
        {
            // Create a pattern key based on service ID and error code
            string patternKey = $"{errorContext.ServiceId}:{errorContext.ErrorCode}";

            // Get or create the error pattern
            var pattern = _errorPatterns.GetOrAdd(patternKey, _ => new ErrorPattern
            {
                ServiceId = errorContext.ServiceId,
                ErrorCode = errorContext.ErrorCode,
                Classification = errorContext.Classification,
                FirstOccurrence = DateTime.UtcNow
            });

            // Update the pattern
            pattern.OccurrenceCount++;
            pattern.LastOccurrence = DateTime.UtcNow;

            // Calculate the frequency (occurrences per hour)
            var timeSpan = pattern.LastOccurrence - pattern.FirstOccurrence;
            if (timeSpan.TotalHours > 0)
            {
                pattern.FrequencyPerHour = pattern.OccurrenceCount / timeSpan.TotalHours;
            }

            // Log the pattern
            _logger.LogDebug("Error pattern {PatternKey}: {OccurrenceCount} occurrences, {FrequencyPerHour:F2} per hour", 
                patternKey, pattern.OccurrenceCount, pattern.FrequencyPerHour);
        }

        /// <summary>
        /// Gets all error patterns.
        /// </summary>
        /// <returns>The error patterns.</returns>
        public IEnumerable<ErrorPattern> GetErrorPatterns()
        {
            return _errorPatterns.Values;
        }

        /// <summary>
        /// Gets the error patterns for a specific service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The error patterns.</returns>
        public IEnumerable<ErrorPattern> GetErrorPatternsForService(string serviceId)
        {
            return _errorPatterns.Values.Where(p => p.ServiceId == serviceId);
        }
    }

    /// <summary>
    /// Represents an error pattern.
    /// </summary>
    public class ErrorPattern
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error classification.
        /// </summary>
        public ErrorClassification Classification { get; set; }

        /// <summary>
        /// Gets or sets the number of occurrences.
        /// </summary>
        public int OccurrenceCount { get; set; }

        /// <summary>
        /// Gets or sets the first occurrence timestamp.
        /// </summary>
        public DateTime FirstOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the last occurrence timestamp.
        /// </summary>
        public DateTime LastOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the frequency per hour.
        /// </summary>
        public double FrequencyPerHour { get; set; }
    }
}
