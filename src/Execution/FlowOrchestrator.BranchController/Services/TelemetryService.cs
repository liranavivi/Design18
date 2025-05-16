using FlowOrchestrator.BranchController.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FlowOrchestrator.BranchController.Services
{
    /// <summary>
    /// Service for managing telemetry.
    /// </summary>
    public class TelemetryService
    {
        private readonly ILogger<TelemetryService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TelemetryService(ILogger<TelemetryService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Records the start of a branch execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="parentActivity">The parent activity.</param>
        /// <returns>The activity.</returns>
        public Activity? RecordBranchExecutionStart(string executionId, string branchId, Activity? parentActivity = null)
        {
            var activity = new Activity("BranchExecution");

            if (parentActivity != null && parentActivity.Id != null)
            {
                activity.SetParentId(parentActivity.Id);
            }

            activity.SetTag("executionId", executionId);
            activity.SetTag("branchId", branchId);
            activity.SetTag("component", "BranchController");
            activity.Start();

            _logger.LogInformation("Started branch execution tracking for branch {BranchId} of execution {ExecutionId}",
                branchId, executionId);

            return activity;
        }

        /// <summary>
        /// Records the end of a branch execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="status">The branch status.</param>
        /// <param name="activity">The activity.</param>
        public void RecordBranchExecutionEnd(string executionId, string branchId, BranchStatus status, Activity activity)
        {
            activity.SetTag("status", status.ToString());
            activity.SetTag("endTime", DateTime.UtcNow);
            activity.Stop();

            _logger.LogInformation("Ended branch execution tracking for branch {BranchId} of execution {ExecutionId} with status {Status}",
                branchId, executionId, status);
        }

        /// <summary>
        /// Creates a step activity.
        /// </summary>
        /// <param name="stepType">The step type.</param>
        /// <param name="stepName">The step name.</param>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentName">The component name.</param>
        /// <returns>The activity.</returns>
        public Activity CreateStepActivity(string stepType, string stepName, string componentType, string componentName)
        {
            var activity = new Activity($"{componentType}.{stepName}");
            activity.SetTag("stepType", stepType);
            activity.SetTag("stepName", stepName);
            activity.SetTag("componentType", componentType);
            activity.SetTag("componentName", componentName);
            activity.Start();

            return activity;
        }
    }
}
