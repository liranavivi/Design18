using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Logging
{
    /// <summary>
    /// Adapter for using LoggingService with ILogger interface.
    /// </summary>
    public class LoggerAdapter : ILogger
    {
        private readonly LoggingService _loggingService;
        private readonly string _categoryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerAdapter"/> class.
        /// </summary>
        /// <param name="loggingService">The logging service.</param>
        /// <param name="categoryName">The category name.</param>
        public LoggerAdapter(LoggingService loggingService, string categoryName)
        {
            _loggingService = loggingService;
            _categoryName = categoryName;
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null; // Scopes not supported in this adapter
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true; // Always enabled
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);

            if (exception != null)
            {
                _loggingService.LogError(exception, $"[{_categoryName}] {message}");
                return;
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    _loggingService.LogDebug($"[{_categoryName}] {message}");
                    break;
                case LogLevel.Information:
                    _loggingService.LogInformation($"[{_categoryName}] {message}");
                    break;
                case LogLevel.Warning:
                    _loggingService.LogWarning($"[{_categoryName}] {message}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    _loggingService.LogCritical($"[{_categoryName}] {message}");
                    break;
                default:
                    _loggingService.LogInformation($"[{_categoryName}] {message}");
                    break;
            }
        }
    }

    /// <summary>
    /// Factory for creating LoggerAdapter instances.
    /// </summary>
    public class LoggerAdapterFactory : ILoggerFactory
    {
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerAdapterFactory"/> class.
        /// </summary>
        /// <param name="loggingService">The logging service.</param>
        public LoggerAdapterFactory(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        /// <inheritdoc/>
        public void AddProvider(ILoggerProvider provider)
        {
            // Not used in this implementation
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new LoggerAdapter(_loggingService, categoryName);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
