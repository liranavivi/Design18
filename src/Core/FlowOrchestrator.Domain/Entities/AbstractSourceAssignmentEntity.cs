using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Base abstract implementation for source assignment entities.
    /// Defines the relationship between a source entity and an importer service.
    /// </summary>
    public abstract class AbstractSourceAssignmentEntity : AbstractEntity
    {
        /// <summary>
        /// Gets or sets the source assignment identifier.
        /// </summary>
        public string AssignmentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the source assignment.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the source assignment.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source entity identifier.
        /// </summary>
        public string SourceEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the importer service identifier.
        /// </summary>
        public string ImporterServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source assignment configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the source assignment metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a value indicating whether the source assignment is enabled.
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
            return "SourceAssignmentEntity";
        }

        /// <summary>
        /// Validates the source assignment entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required fields
            if (string.IsNullOrWhiteSpace(AssignmentId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "ASSIGNMENT_ID_REQUIRED", Message = "Source assignment ID is required." });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "ASSIGNMENT_NAME_REQUIRED", Message = "Source assignment name is required." });
            }

            if (string.IsNullOrWhiteSpace(SourceEntityId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "SOURCE_ENTITY_ID_REQUIRED", Message = "Source entity ID is required." });
            }

            if (string.IsNullOrWhiteSpace(ImporterServiceId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "IMPORTER_SERVICE_ID_REQUIRED", Message = "Importer service ID is required." });
            }

            // Validate compatibility between source and importer
            var compatibilityValidation = ValidateCompatibility();
            if (!compatibilityValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(compatibilityValidation.Errors);
            }

            return result;
        }

        /// <summary>
        /// Validates the compatibility between the source entity and the importer service.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateCompatibility();

        /// <summary>
        /// Copies the properties of this entity to another entity.
        /// </summary>
        /// <param name="target">The target entity.</param>
        protected override void CopyPropertiesTo(AbstractEntity target)
        {
            base.CopyPropertiesTo(target);

            if (target is AbstractSourceAssignmentEntity assignmentEntity)
            {
                assignmentEntity.AssignmentId = AssignmentId;
                assignmentEntity.Name = Name;
                assignmentEntity.Description = Description;
                assignmentEntity.SourceEntityId = SourceEntityId;
                assignmentEntity.ImporterServiceId = ImporterServiceId;
                assignmentEntity.Configuration = new Dictionary<string, object>(Configuration);
                assignmentEntity.Metadata = new Dictionary<string, object>(Metadata);
                assignmentEntity.IsEnabled = IsEnabled;
            }
        }
    }
}
