using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Tracing
{
    /// <summary>
    /// Service for distributed tracing.
    /// </summary>
    public class TracingService
    {
        private readonly ActivitySourceProvider _activitySourceProvider;
        private readonly ILogger<TracingService> _logger;
        private readonly Dictionary<string, Activity> _activeActivities = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TracingService"/> class.
        /// </summary>
        /// <param name="activitySourceProvider">The activity source provider.</param>
        /// <param name="logger">The logger.</param>
        public TracingService(
            ActivitySourceProvider activitySourceProvider,
            ILogger<TracingService> logger)
        {
            _activitySourceProvider = activitySourceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Starts a new trace span.
        /// </summary>
        /// <param name="name">The span name.</param>
        /// <param name="kind">The span kind.</param>
        /// <param name="parentId">The optional parent span ID.</param>
        /// <param name="attributes">The optional span attributes.</param>
        /// <returns>The span ID.</returns>
        public string StartSpan(
            string name,
            ActivityKind kind = ActivityKind.Internal,
            string? parentId = null,
            Dictionary<string, object>? attributes = null)
        {
            try
            {
                var activity = _activitySourceProvider.StartActivity(name, kind, parentId, attributes);

                if (activity != null)
                {
                    _activeActivities[activity.Id ?? string.Empty] = activity;
                    return activity.Id ?? string.Empty;
                }

                _logger.LogWarning("Failed to start span {SpanName}", name);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting span {SpanName}", name);
                return string.Empty;
            }
        }

        /// <summary>
        /// Adds an event to an active span.
        /// </summary>
        /// <param name="spanId">The span ID.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="attributes">The optional event attributes.</param>
        public void AddEvent(string spanId, string eventName, Dictionary<string, object>? attributes = null)
        {
            try
            {
                if (_activeActivities.TryGetValue(spanId, out var activity))
                {
                    var tagCollection = attributes != null
                        ? new ActivityTagsCollection(attributes.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value))
                        : new ActivityTagsCollection();

                    activity.AddEvent(new ActivityEvent(eventName, DateTime.UtcNow, tagCollection));
                }
                else
                {
                    _logger.LogWarning("Attempted to add event to non-existent span {SpanId}", spanId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding event {EventName} to span {SpanId}", eventName, spanId);
            }
        }

        /// <summary>
        /// Adds attributes to an active span.
        /// </summary>
        /// <param name="spanId">The span ID.</param>
        /// <param name="attributes">The attributes to add.</param>
        public void AddAttributes(string spanId, Dictionary<string, object> attributes)
        {
            try
            {
                if (_activeActivities.TryGetValue(spanId, out var activity))
                {
                    foreach (var attribute in attributes)
                    {
                        activity.SetTag(attribute.Key, attribute.Value);
                    }
                }
                else
                {
                    _logger.LogWarning("Attempted to add attributes to non-existent span {SpanId}", spanId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding attributes to span {SpanId}", spanId);
            }
        }

        /// <summary>
        /// Sets the status of an active span.
        /// </summary>
        /// <param name="spanId">The span ID.</param>
        /// <param name="status">The status.</param>
        /// <param name="description">The optional status description.</param>
        public void SetStatus(string spanId, ActivityStatusCode status, string? description = null)
        {
            try
            {
                if (_activeActivities.TryGetValue(spanId, out var activity))
                {
                    activity.SetStatus(status, description);
                }
                else
                {
                    _logger.LogWarning("Attempted to set status on non-existent span {SpanId}", spanId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting status on span {SpanId}", spanId);
            }
        }

        /// <summary>
        /// Ends an active span.
        /// </summary>
        /// <param name="spanId">The span ID.</param>
        public void EndSpan(string spanId)
        {
            try
            {
                if (_activeActivities.TryGetValue(spanId, out var activity))
                {
                    activity.Dispose();
                    _activeActivities.Remove(spanId);
                }
                else
                {
                    _logger.LogWarning("Attempted to end non-existent span {SpanId}", spanId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending span {SpanId}", spanId);
            }
        }

        /// <summary>
        /// Records an exception in an active span.
        /// </summary>
        /// <param name="spanId">The span ID.</param>
        /// <param name="exception">The exception.</param>
        public void RecordException(string spanId, Exception exception)
        {
            try
            {
                if (_activeActivities.TryGetValue(spanId, out var activity))
                {
                    activity.SetStatus(ActivityStatusCode.Error, exception.Message);

                    var attributes = new Dictionary<string, object>
                    {
                        ["exception.type"] = exception.GetType().FullName ?? "Unknown",
                        ["exception.message"] = exception.Message,
                        ["exception.stacktrace"] = exception.StackTrace ?? string.Empty
                    };

                    var tagCollection = new ActivityTagsCollection(attributes.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value));
                    activity.AddEvent(new ActivityEvent("exception", DateTime.UtcNow, tagCollection));
                }
                else
                {
                    _logger.LogWarning("Attempted to record exception on non-existent span {SpanId}", spanId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording exception on span {SpanId}", spanId);
            }
        }
    }
}
