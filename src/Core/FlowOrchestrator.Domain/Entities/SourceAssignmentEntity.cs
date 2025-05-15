using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Concrete implementation of the AbstractSourceAssignmentEntity class.
    /// Defines the relationship between a source entity and an importer service.
    /// </summary>
    public class SourceAssignmentEntity : AbstractSourceAssignmentEntity
    {
        /// <summary>
        /// Gets or sets the source type.
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the importer type.
        /// </summary>
        public string ImporterType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data format.
        /// </summary>
        public string DataFormat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema definition.
        /// </summary>
        public string? SchemaDefinition { get; set; }

        /// <summary>
        /// Gets or sets the validation rules.
        /// </summary>
        public Dictionary<string, object> ValidationRules { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the transformation rules.
        /// </summary>
        public Dictionary<string, object> TransformationRules { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the error handling configuration.
        /// </summary>
        public ErrorHandlingConfig ErrorHandling { get; set; } = new ErrorHandlingConfig();

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAssignmentEntity"/> class.
        /// </summary>
        public SourceAssignmentEntity()
        {
            // Default constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAssignmentEntity"/> class with the specified assignment ID.
        /// </summary>
        /// <param name="assignmentId">The assignment identifier.</param>
        public SourceAssignmentEntity(string assignmentId)
        {
            AssignmentId = assignmentId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAssignmentEntity"/> class with the specified assignment ID and name.
        /// </summary>
        /// <param name="assignmentId">The assignment identifier.</param>
        /// <param name="name">The name of the assignment.</param>
        public SourceAssignmentEntity(string assignmentId, string name) : this(assignmentId)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAssignmentEntity"/> class with the specified assignment ID, name, source entity ID, and importer service ID.
        /// </summary>
        /// <param name="assignmentId">The assignment identifier.</param>
        /// <param name="name">The name of the assignment.</param>
        /// <param name="sourceEntityId">The source entity identifier.</param>
        /// <param name="importerServiceId">The importer service identifier.</param>
        public SourceAssignmentEntity(string assignmentId, string name, string sourceEntityId, string importerServiceId) : this(assignmentId, name)
        {
            SourceEntityId = sourceEntityId;
            ImporterServiceId = importerServiceId;
        }

        /// <summary>
        /// Validates the compatibility between the source entity and the importer service.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateCompatibility()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate source type and importer type compatibility
            if (string.IsNullOrWhiteSpace(SourceType))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "SOURCE_TYPE_REQUIRED", Message = "Source type is required." });
            }

            if (string.IsNullOrWhiteSpace(ImporterType))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "IMPORTER_TYPE_REQUIRED", Message = "Importer type is required." });
            }

            // Check if source type and importer type are compatible
            if (!string.IsNullOrWhiteSpace(SourceType) && !string.IsNullOrWhiteSpace(ImporterType))
            {
                if (!IsSourceTypeCompatibleWithImporterType(SourceType, ImporterType))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INCOMPATIBLE_TYPES", Message = $"Source type '{SourceType}' is not compatible with importer type '{ImporterType}'." });
                }
            }

            // Validate data format
            if (string.IsNullOrWhiteSpace(DataFormat))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DATA_FORMAT_REQUIRED", Message = "Data format is required." });
            }

            // Validate schema definition if provided
            if (!string.IsNullOrWhiteSpace(SchemaDefinition))
            {
                // Here we would validate the schema definition format
                // This is a simplified check
                if (!SchemaDefinition.Contains("{") || !SchemaDefinition.Contains("}"))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INVALID_SCHEMA_DEFINITION", Message = "Schema definition is not in a valid format." });
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified source type is compatible with the specified importer type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="importerType">The importer type.</param>
        /// <returns>True if the source type is compatible with the importer type, otherwise false.</returns>
        private bool IsSourceTypeCompatibleWithImporterType(string sourceType, string importerType)
        {
            // This is a simplified compatibility check
            // In a real implementation, this would be more sophisticated
            
            // Check for direct match
            if (sourceType.Equals(importerType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check for known compatible pairs
            if (sourceType.Equals("file", StringComparison.OrdinalIgnoreCase) && 
                (importerType.Equals("file", StringComparison.OrdinalIgnoreCase) || 
                 importerType.Equals("csv", StringComparison.OrdinalIgnoreCase) || 
                 importerType.Equals("json", StringComparison.OrdinalIgnoreCase) || 
                 importerType.Equals("xml", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (sourceType.Equals("database", StringComparison.OrdinalIgnoreCase) && 
                importerType.Equals("database", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (sourceType.Equals("rest", StringComparison.OrdinalIgnoreCase) && 
                importerType.Equals("rest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Represents the error handling configuration for a source assignment.
    /// </summary>
    public class ErrorHandlingConfig
    {
        /// <summary>
        /// Gets or sets the maximum number of retries.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry interval in seconds.
        /// </summary>
        public int RetryIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets a value indicating whether to skip errors.
        /// </summary>
        public bool SkipErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets the error threshold percentage.
        /// </summary>
        public double ErrorThresholdPercentage { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the error notification email.
        /// </summary>
        public string? ErrorNotificationEmail { get; set; }
    }
}
