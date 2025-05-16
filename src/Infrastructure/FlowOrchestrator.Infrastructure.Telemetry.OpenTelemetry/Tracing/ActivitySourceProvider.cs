using System.Diagnostics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Tracing
{
    /// <summary>
    /// Provider for OpenTelemetry ActivitySource instances.
    /// </summary>
    public class ActivitySourceProvider
    {
        private readonly ActivitySource _activitySource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivitySourceProvider"/> class.
        /// </summary>
        /// <param name="options">The OpenTelemetry options.</param>
        public ActivitySourceProvider(IOptions<OpenTelemetryOptions> options)
        {
            var telemetryOptions = options.Value;
            _activitySource = new ActivitySource(telemetryOptions.ServiceName, telemetryOptions.ServiceVersion);
        }

        /// <summary>
        /// Gets the activity source.
        /// </summary>
        public ActivitySource ActivitySource => _activitySource;

        /// <summary>
        /// Creates a new activity.
        /// </summary>
        /// <param name="name">The activity name.</param>
        /// <param name="kind">The activity kind.</param>
        /// <param name="parentId">The optional parent activity ID.</param>
        /// <param name="tags">The optional tags.</param>
        /// <returns>The created activity.</returns>
        public Activity? StartActivity(
            string name,
            ActivityKind kind = ActivityKind.Internal,
            string? parentId = null,
            Dictionary<string, object>? tags = null)
        {
            var activityContext = default(ActivityContext);
            
            if (!string.IsNullOrEmpty(parentId))
            {
                // Try to parse the parent ID into an ActivityContext
                ActivityContext.TryParse(parentId, null, out activityContext);
            }

            var activity = _activitySource.StartActivity(
                name,
                kind,
                parentId != null ? activityContext : default,
                tags?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value));

            return activity;
        }
    }
}
