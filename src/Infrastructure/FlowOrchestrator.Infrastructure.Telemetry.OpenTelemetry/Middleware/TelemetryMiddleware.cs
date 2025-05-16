using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Middleware
{
    /// <summary>
    /// Middleware for adding telemetry to HTTP requests.
    /// </summary>
    public class TelemetryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IStatisticsProvider _statisticsProvider;
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="statisticsProvider">The statistics provider.</param>
        /// <param name="loggingService">The logging service.</param>
        public TelemetryMiddleware(
            RequestDelegate next,
            IStatisticsProvider statisticsProvider,
            LoggingService loggingService)
        {
            _next = next;
            _statisticsProvider = statisticsProvider;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint()?.DisplayName ?? "unknown";
            var operationName = $"http.{context.Request.Method.ToLowerInvariant()}.{endpoint}";

            _statisticsProvider.StartOperation(operationName);
            _loggingService.LogInformation("Request started: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);

                stopwatch.Stop();

                _statisticsProvider.RecordMetric("http.status_code", context.Response.StatusCode);
                _statisticsProvider.RecordDuration("http.duration", stopwatch.ElapsedMilliseconds);

                if (context.Response.StatusCode >= 400)
                {
                    _loggingService.LogWarning("Request completed with error status code: {StatusCode} for {Method} {Path}",
                        context.Response.StatusCode, context.Request.Method, context.Request.Path);
                    _statisticsProvider.EndOperation(operationName, OperationResult.FAILURE);
                }
                else
                {
                    _loggingService.LogInformation("Request completed successfully: {StatusCode} for {Method} {Path} in {Duration}ms",
                        context.Response.StatusCode, context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
                    _statisticsProvider.EndOperation(operationName, OperationResult.SUCCESS);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _loggingService.LogError(ex, "Request failed for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                _statisticsProvider.RecordDuration("http.duration", stopwatch.ElapsedMilliseconds);
                _statisticsProvider.EndOperation(operationName, OperationResult.FAILURE);

                throw;
            }
        }
    }

    /// <summary>
    /// Extension methods for the telemetry middleware.
    /// </summary>
    public static class TelemetryMiddlewareExtensions
    {
        /// <summary>
        /// Adds the telemetry middleware to the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseFlowOrchestratorTelemetry(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TelemetryMiddleware>();
        }
    }
}
