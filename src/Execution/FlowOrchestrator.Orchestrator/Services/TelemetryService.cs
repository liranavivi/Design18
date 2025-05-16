using FlowOrchestrator.Abstractions.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FlowOrchestrator.Orchestrator.Services
{
    /// <summary>
    /// Service for collecting telemetry data.
    /// </summary>
    public class TelemetryService
    {
        private readonly ILogger<TelemetryService> _logger;
        private readonly Meter _meter;
        private readonly Counter<long> _flowExecutionsStarted;
        private readonly Counter<long> _flowExecutionsCompleted;
        private readonly Counter<long> _flowExecutionsFailed;
        private readonly Counter<long> _branchesCreated;
        private readonly Counter<long> _branchesCompleted;
        private readonly Counter<long> _branchesFailed;
        private readonly Histogram<double> _flowExecutionDuration;
        private readonly Histogram<double> _branchExecutionDuration;
        private readonly ConcurrentDictionary<string, Stopwatch> _executionTimers;
        private readonly ConcurrentDictionary<string, Stopwatch> _branchTimers;
        private readonly ActivitySource _activitySource;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TelemetryService(ILogger<TelemetryService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _meter = new Meter("FlowOrchestrator.Orchestrator", "1.0.0");
            _activitySource = new ActivitySource("FlowOrchestrator.Orchestrator", "1.0.0");
            
            // Create counters
            _flowExecutionsStarted = _meter.CreateCounter<long>("flow_executions_started", "executions", "Number of flow executions started");
            _flowExecutionsCompleted = _meter.CreateCounter<long>("flow_executions_completed", "executions", "Number of flow executions completed");
            _flowExecutionsFailed = _meter.CreateCounter<long>("flow_executions_failed", "executions", "Number of flow executions failed");
            _branchesCreated = _meter.CreateCounter<long>("branches_created", "branches", "Number of branches created");
            _branchesCompleted = _meter.CreateCounter<long>("branches_completed", "branches", "Number of branches completed");
            _branchesFailed = _meter.CreateCounter<long>("branches_failed", "branches", "Number of branches failed");
            
            // Create histograms
            _flowExecutionDuration = _meter.CreateHistogram<double>("flow_execution_duration", "ms", "Duration of flow executions");
            _branchExecutionDuration = _meter.CreateHistogram<double>("branch_execution_duration", "ms", "Duration of branch executions");
            
            // Create dictionaries for timers
            _executionTimers = new ConcurrentDictionary<string, Stopwatch>();
            _branchTimers = new ConcurrentDictionary<string, Stopwatch>();
        }

        /// <summary>
        /// Records the start of a flow execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="flowId">The flow identifier.</param>
        /// <returns>The activity for distributed tracing.</returns>
        public Activity? RecordFlowExecutionStart(string executionId, string flowId)
        {
            _logger.LogInformation("Recording start of flow execution {ExecutionId} for flow {FlowId}", executionId, flowId);
            
            // Increment counter
            _flowExecutionsStarted.Add(1, new KeyValuePair<string, object?>("flowId", flowId));
            
            // Start timer
            var timer = new Stopwatch();
            timer.Start();
            _executionTimers[executionId] = timer;
            
            // Create activity for distributed tracing
            var activity = _activitySource.StartActivity("FlowExecution", ActivityKind.Internal);
            if (activity != null)
            {
                activity.SetTag("executionId", executionId);
                activity.SetTag("flowId", flowId);
                activity.SetTag("component", "Orchestrator");
            }
            
            return activity;
        }

        /// <summary>
        /// Records the completion of a flow execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="status">The execution status.</param>
        /// <param name="activity">The activity for distributed tracing.</param>
        public void RecordFlowExecutionEnd(string executionId, string flowId, ExecutionStatus status, Activity? activity = null)
        {
            _logger.LogInformation("Recording end of flow execution {ExecutionId} for flow {FlowId} with status {Status}",
                executionId, flowId, status);
            
            // Increment counter
            if (status == ExecutionStatus.COMPLETED)
            {
                _flowExecutionsCompleted.Add(1, new KeyValuePair<string, object?>("flowId", flowId));
            }
            else if (status == ExecutionStatus.FAILED)
            {
                _flowExecutionsFailed.Add(1, new KeyValuePair<string, object?>("flowId", flowId));
            }
            
            // Stop timer and record duration
            if (_executionTimers.TryRemove(executionId, out var timer))
            {
                timer.Stop();
                _flowExecutionDuration.Record(timer.ElapsedMilliseconds, 
                    new KeyValuePair<string, object?>("flowId", flowId),
                    new KeyValuePair<string, object?>("status", status.ToString()));
            }
            
            // End activity
            activity?.SetTag("status", status.ToString());
            activity?.Dispose();
        }

        /// <summary>
        /// Records the start of a branch execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="parentActivity">The parent activity for distributed tracing.</param>
        /// <returns>The activity for distributed tracing.</returns>
        public Activity? RecordBranchExecutionStart(string executionId, string branchId, Activity? parentActivity = null)
        {
            _logger.LogInformation("Recording start of branch execution {BranchId} for execution {ExecutionId}", branchId, executionId);
            
            // Increment counter
            _branchesCreated.Add(1, new KeyValuePair<string, object?>("executionId", executionId));
            
            // Start timer
            var timer = new Stopwatch();
            timer.Start();
            _branchTimers[$"{executionId}:{branchId}"] = timer;
            
            // Create activity for distributed tracing
            var activity = _activitySource.StartActivity("BranchExecution", ActivityKind.Internal, parentActivity?.Context ?? default);
            if (activity != null)
            {
                activity.SetTag("executionId", executionId);
                activity.SetTag("branchId", branchId);
                activity.SetTag("component", "Orchestrator");
            }
            
            return activity;
        }

        /// <summary>
        /// Records the completion of a branch execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="status">The branch status.</param>
        /// <param name="activity">The activity for distributed tracing.</param>
        public void RecordBranchExecutionEnd(string executionId, string branchId, BranchStatus status, Activity? activity = null)
        {
            _logger.LogInformation("Recording end of branch execution {BranchId} for execution {ExecutionId} with status {Status}",
                branchId, executionId, status);
            
            // Increment counter
            if (status == BranchStatus.COMPLETED)
            {
                _branchesCompleted.Add(1, new KeyValuePair<string, object?>("executionId", executionId));
            }
            else if (status == BranchStatus.FAILED)
            {
                _branchesFailed.Add(1, new KeyValuePair<string, object?>("executionId", executionId));
            }
            
            // Stop timer and record duration
            if (_branchTimers.TryRemove($"{executionId}:{branchId}", out var timer))
            {
                timer.Stop();
                _branchExecutionDuration.Record(timer.ElapsedMilliseconds, 
                    new KeyValuePair<string, object?>("executionId", executionId),
                    new KeyValuePair<string, object?>("branchId", branchId),
                    new KeyValuePair<string, object?>("status", status.ToString()));
            }
            
            // End activity
            activity?.SetTag("status", status.ToString());
            activity?.Dispose();
        }

        /// <summary>
        /// Creates a new activity for a step in a flow execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="stepType">The step type.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <param name="parentActivity">The parent activity for distributed tracing.</param>
        /// <returns>The activity for distributed tracing.</returns>
        public Activity? CreateStepActivity(string executionId, string branchId, string stepType, string stepId, Activity? parentActivity = null)
        {
            var activity = _activitySource.StartActivity($"{stepType}Step", ActivityKind.Internal, parentActivity?.Context ?? default);
            if (activity != null)
            {
                activity.SetTag("executionId", executionId);
                activity.SetTag("branchId", branchId);
                activity.SetTag("stepType", stepType);
                activity.SetTag("stepId", stepId);
                activity.SetTag("component", "Orchestrator");
            }
            
            return activity;
        }
    }
}
