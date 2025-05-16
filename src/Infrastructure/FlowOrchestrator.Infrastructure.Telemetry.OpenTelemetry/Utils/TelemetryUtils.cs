using System.Diagnostics;
using FlowOrchestrator.Abstractions.Statistics;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Utils
{
    /// <summary>
    /// Utility methods for telemetry.
    /// </summary>
    public static class TelemetryUtils
    {
        /// <summary>
        /// Converts an operation result to a string.
        /// </summary>
        /// <param name="result">The operation result.</param>
        /// <returns>The string representation.</returns>
        public static string OperationResultToString(OperationResult result)
        {
            return result switch
            {
                OperationResult.SUCCESS => "success",
                OperationResult.FAILURE => "failure",
                OperationResult.CANCELLED => "cancelled",
                OperationResult.TIMEOUT => "timeout",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Converts a statistics collection status to a string.
        /// </summary>
        /// <param name="status">The statistics collection status.</param>
        /// <returns>The string representation.</returns>
        public static string CollectionStatusToString(StatisticsCollectionStatus status)
        {
            return status switch
            {
                StatisticsCollectionStatus.UNINITIALIZED => "uninitialized",
                StatisticsCollectionStatus.INITIALIZED => "initialized",
                StatisticsCollectionStatus.ACTIVE => "active",
                StatisticsCollectionStatus.PAUSED => "paused",
                StatisticsCollectionStatus.STOPPED => "stopped",
                StatisticsCollectionStatus.ERROR => "error",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Converts a dictionary to a tag list.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The tag list.</returns>
        public static ActivityTagsCollection DictionaryToTags(Dictionary<string, object> dictionary)
        {
            return new ActivityTagsCollection(dictionary.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value));
        }

        /// <summary>
        /// Sanitizes a metric name to ensure it follows OpenTelemetry conventions.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <returns>The sanitized name.</returns>
        public static string SanitizeMetricName(string name)
        {
            // Replace invalid characters with underscores
            return name.Replace(' ', '_')
                .Replace('-', '_')
                .Replace('.', '_')
                .ToLowerInvariant();
        }

        /// <summary>
        /// Creates a dictionary of common attributes for telemetry.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceVersion">The service version.</param>
        /// <param name="serviceInstanceId">The service instance ID.</param>
        /// <returns>The common attributes.</returns>
        public static Dictionary<string, object> CreateCommonAttributes(
            string serviceName,
            string serviceVersion,
            string serviceInstanceId)
        {
            return new Dictionary<string, object>
            {
                ["service.name"] = serviceName,
                ["service.version"] = serviceVersion,
                ["service.instance.id"] = serviceInstanceId,
                ["host.name"] = Environment.MachineName,
                ["os.type"] = Environment.OSVersion.Platform.ToString(),
                ["os.version"] = Environment.OSVersion.VersionString,
                ["process.id"] = Environment.ProcessId,
                ["process.runtime.name"] = ".NET",
                ["process.runtime.version"] = Environment.Version.ToString()
            };
        }

        /// <summary>
        /// Creates a dictionary of flow attributes for telemetry.
        /// </summary>
        /// <param name="flowId">The flow ID.</param>
        /// <param name="branchPath">The branch path.</param>
        /// <param name="stepId">The step ID.</param>
        /// <returns>The flow attributes.</returns>
        public static Dictionary<string, object> CreateFlowAttributes(
            string flowId,
            string? branchPath = null,
            string? stepId = null)
        {
            var attributes = new Dictionary<string, object>
            {
                ["flow.id"] = flowId
            };

            if (!string.IsNullOrEmpty(branchPath))
            {
                attributes["flow.branch_path"] = branchPath;
            }

            if (!string.IsNullOrEmpty(stepId))
            {
                attributes["flow.step_id"] = stepId;
            }

            return attributes;
        }
    }
}
