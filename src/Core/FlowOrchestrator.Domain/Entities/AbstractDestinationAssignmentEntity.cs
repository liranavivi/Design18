using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Base abstract implementation for destination assignment entities.
    /// Defines the relationship between a destination entity and an exporter service.
    /// </summary>
    public abstract class AbstractDestinationAssignmentEntity : AbstractEntity
    {
        /// <summary>
        /// Gets or sets the destination assignment identifier.
        /// </summary>
        public string AssignmentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the destination assignment.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the destination assignment.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination entity identifier.
        /// </summary>
        public string DestinationEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exporter service identifier.
        /// </summary>
        public string ExporterServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination assignment configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the destination assignment metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the merge strategy for the destination assignment.
        /// </summary>
        public string? MergeStrategy { get; set; }

        /// <summary>
        /// Gets or sets the merge configuration for the destination assignment.
        /// </summary>
        public Dictionary<string, object> MergeConfiguration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a value indicating whether the destination assignment is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public override string GetEntityId()
        {
            return AssignmentId;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public override string GetEntityType()
        {
            return "DestinationAssignmentEntity";
        }

        /// <summary>
        /// Validates the destination assignment entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required fields
            if (string.IsNullOrWhiteSpace(AssignmentId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "ASSIGNMENT_ID_REQUIRED", Message = "Destination assignment ID is required." });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "ASSIGNMENT_NAME_REQUIRED", Message = "Destination assignment name is required." });
            }

            if (string.IsNullOrWhiteSpace(DestinationEntityId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DESTINATION_ENTITY_ID_REQUIRED", Message = "Destination entity ID is required." });
            }

            if (string.IsNullOrWhiteSpace(ExporterServiceId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "EXPORTER_SERVICE_ID_REQUIRED", Message = "Exporter service ID is required." });
            }

            // Validate compatibility between destination and exporter
            var compatibilityValidation = ValidateCompatibility();
            if (!compatibilityValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(compatibilityValidation.Errors);
            }

            // Validate merge strategy if specified
            if (!string.IsNullOrWhiteSpace(MergeStrategy))
            {
                var mergeStrategyValidation = ValidateMergeStrategy();
                if (!mergeStrategyValidation.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(mergeStrategyValidation.Errors);
                }
            }

            return result;
        }

        /// <summary>
        /// Validates the compatibility between the destination entity and the exporter service.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateCompatibility();

        /// <summary>
        /// Validates the merge strategy configuration.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateMergeStrategy();

        /// <summary>
        /// Copies the properties of this entity to another entity.
        /// </summary>
        /// <param name="target">The target entity.</param>
        protected override void CopyPropertiesTo(AbstractEntity target)
        {
            base.CopyPropertiesTo(target);

            if (target is AbstractDestinationAssignmentEntity assignmentEntity)
            {
                assignmentEntity.AssignmentId = AssignmentId;
                assignmentEntity.Name = Name;
                assignmentEntity.Description = Description;
                assignmentEntity.DestinationEntityId = DestinationEntityId;
                assignmentEntity.ExporterServiceId = ExporterServiceId;
                assignmentEntity.Configuration = new Dictionary<string, object>(Configuration);
                assignmentEntity.Metadata = new Dictionary<string, object>(Metadata);
                assignmentEntity.MergeStrategy = MergeStrategy;
                assignmentEntity.MergeConfiguration = new Dictionary<string, object>(MergeConfiguration);
                assignmentEntity.IsEnabled = IsEnabled;
            }
        }
    }
}
