using System.Diagnostics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Tracing;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Logging
{
    /// <summary>
    /// Service for logging integration with OpenTelemetry.
    /// </summary>
    public class LoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly TracingService _tracingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="tracingService">The tracing service.</param>
        public LoggingService(
            ILogger<LoggingService> logger,
            TracingService tracingService)
        {
            _logger = logger;
            _tracingService = tracingService;
        }

        /// <summary>
        /// Logs a message at the specified log level and adds it as an event to the current span.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="args">The message arguments.</param>
        public void Log(LogLevel logLevel, string message, params object[] args)
        {
            try
            {
                // Format the message with the provided arguments
                string formattedMessage = string.Format(message, args);
                
                // Log using the standard logger
                _logger.Log(logLevel, formattedMessage);
                
                // Add the log as an event to the current span if one exists
                var currentActivity = Activity.Current;
                if (currentActivity != null)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        ["log.level"] = logLevel.ToString(),
                        ["log.message"] = formattedMessage
                    };
                    
                    _tracingService.AddEvent(currentActivity.Id ?? string.Empty, "log", attributes);
                }
            }
            catch (Exception ex)
            {
                // If there's an error in the logging process, log it directly
                _logger.LogError(ex, "Error in logging service");
            }
        }

        /// <summary>
        /// Logs an error message and records the exception in the current span.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The log message.</param>
        /// <param name="args">The message arguments.</param>
        public void LogError(Exception exception, string message, params object[] args)
        {
            try
            {
                // Format the message with the provided arguments
                string formattedMessage = string.Format(message, args);
                
                // Log using the standard logger
                _logger.LogError(exception, formattedMessage);
                
                // Record the exception in the current span if one exists
                var currentActivity = Activity.Current;
                if (currentActivity != null)
                {
                    _tracingService.RecordException(currentActivity.Id ?? string.Empty, exception);
                }
            }
            catch (Exception ex)
            {
                // If there's an error in the logging process, log it directly
                _logger.LogError(ex, "Error in logging service");
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="args">The message arguments.</param>
        public void LogDebug(string message, params object[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="args">The message arguments.</param>
        public void LogInformation(string message, params object[] args)
        {
            Log(LogLevel.Information, message, args);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="args">The message arguments.</param>
        public void LogWarning(string message, params object[] args)
        {
            Log(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// Logs a critical message.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="args">The message arguments.</param>
        public void LogCritical(string message, params object[] args)
        {
            Log(LogLevel.Critical, message, args);
        }
    }
}
