using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Domain.Entities;

namespace FlowOrchestrator.Management.Scheduling.Adapters
{
    /// <summary>
    /// Adapter for TaskSchedulerEntity to implement ITaskSchedulerEntity.
    /// </summary>
    public class TaskSchedulerEntityAdapter : ITaskSchedulerEntity
    {
        private readonly TaskSchedulerEntity _entity;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerEntityAdapter"/> class.
        /// </summary>
        /// <param name="entity">The task scheduler entity.</param>
        public TaskSchedulerEntityAdapter(TaskSchedulerEntity entity)
        {
            _entity = entity;
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public string ServiceId => _entity.SchedulerId;

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public string ServiceName => _entity.Name;

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public string ServiceType => _entity.GetEntityType();

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public string ServiceVersion => "1.0.0";

        /// <summary>
        /// Gets the service status.
        /// </summary>
        public string ServiceStatus => _entity.State.ToString();

        /// <summary>
        /// Gets the scheduler name.
        /// </summary>
        public string SchedulerName => _entity.Name;

        /// <summary>
        /// Gets the scheduler type.
        /// </summary>
        public string SchedulerType => _entity.ScheduleType.ToString();

        /// <summary>
        /// Gets the schedule expression.
        /// </summary>
        public string ScheduleExpression => _entity.ScheduleExpression;

        /// <summary>
        /// Gets the schedule parameters.
        /// </summary>
        public Dictionary<string, string> ScheduleParameters => _entity.Configuration
            .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty);

        /// <summary>
        /// Gets a value indicating whether the scheduler is enabled.
        /// </summary>
        public bool? Enabled => _entity.IsEnabled;

        /// <summary>
        /// Gets or sets the version of the entity.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the entity was last modified.
        /// </summary>
        public DateTime LastModifiedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the description of the version.
        /// </summary>
        public string VersionDescription { get; set; } = "Initial version";

        /// <summary>
        /// Gets or sets the identifier of the previous version.
        /// </summary>
        public string PreviousVersionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the version.
        /// </summary>
        public VersionStatus VersionStatus { get; set; } = VersionStatus.ACTIVE;

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public string GetEntityId() => _entity.GetEntityId();

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public string GetEntityType() => _entity.GetEntityType();

        /// <summary>
        /// Validates the entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public ValidationResult Validate() => _entity.Validate();

        /// <summary>
        /// Initializes the service with the specified configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public void Initialize(ConfigurationParameters parameters)
        {
            // No-op for adapter
        }

        /// <summary>
        /// Terminates the service.
        /// </summary>
        public void Terminate()
        {
            // No-op for adapter
        }

        /// <summary>
        /// Gets the current state of the service.
        /// </summary>
        /// <returns>The current service state.</returns>
        public ServiceState GetState()
        {
            return _entity.State switch
            {
                SchedulerState.ACTIVE => ServiceState.PROCESSING,
                SchedulerState.PAUSED => ServiceState.READY,
                SchedulerState.FAILED => ServiceState.ERROR,
                SchedulerState.TERMINATED => ServiceState.TERMINATED,
                _ => ServiceState.UNINITIALIZED
            };
        }
    }
}
