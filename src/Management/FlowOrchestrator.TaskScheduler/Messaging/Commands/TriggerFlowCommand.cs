using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Management.Scheduling.Messaging.Commands
{
    /// <summary>
    /// Represents a command to trigger a flow execution immediately.
    /// </summary>
    public class TriggerFlowCommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the scheduled flow entity identifier.
        /// </summary>
        public string ScheduledFlowEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow entity identifier.
        /// </summary>
        public string FlowEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the task scheduler entity identifier.
        /// </summary>
        public string TaskSchedulerEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow parameters.
        /// </summary>
        public Dictionary<string, string> FlowParameters { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public ExecutionContext Context { get; set; } = new ExecutionContext();

        /// <summary>
        /// Gets or sets the timestamp when the command was created.
        /// </summary>
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerFlowCommand"/> class.
        /// </summary>
        public TriggerFlowCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerFlowCommand"/> class.
        /// </summary>
        /// <param name="scheduledFlowEntityId">The scheduled flow entity identifier.</param>
        /// <param name="flowEntityId">The flow entity identifier.</param>
        /// <param name="taskSchedulerEntityId">The task scheduler entity identifier.</param>
        /// <param name="flowParameters">The flow parameters.</param>
        /// <param name="context">The execution context.</param>
        public TriggerFlowCommand(
            string scheduledFlowEntityId,
            string flowEntityId,
            string taskSchedulerEntityId,
            Dictionary<string, string> flowParameters,
            ExecutionContext context)
        {
            ScheduledFlowEntityId = scheduledFlowEntityId;
            FlowEntityId = flowEntityId;
            TaskSchedulerEntityId = taskSchedulerEntityId;
            FlowParameters = flowParameters;
            Context = context;
        }
    }
}
