using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Concrete implementation of the AbstractDestinationAssignmentEntity class.
    /// Defines the relationship between a destination entity and an exporter service.
    /// </summary>
    public class DestinationAssignmentEntity : AbstractDestinationAssignmentEntity
    {
        /// <summary>
        /// Gets or sets the destination type.
        /// </summary>
        public string DestinationType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exporter type.
        /// </summary>
        public string ExporterType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data format.
        /// </summary>
        public string DataFormat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema definition.
        /// </summary>
        public string? SchemaDefinition { get; set; }

        /// <summary>
        /// Gets or sets the transformation rules.
        /// </summary>
        public Dictionary<string, object> TransformationRules { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the batch size.
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether to use transactions.
        /// </summary>
        public bool UseTransactions { get; set; } = true;

        /// <summary>
        /// Gets or sets the error handling configuration.
        /// </summary>
        public ErrorHandlingConfig ErrorHandling { get; set; } = new ErrorHandlingConfig();

        /// <summary>
        /// Gets or sets the conflict resolution strategy.
        /// </summary>
        public ConflictResolutionStrategy ConflictResolution { get; set; } = new ConflictResolutionStrategy();

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationAssignmentEntity"/> class.
        /// </summary>
        public DestinationAssignmentEntity()
        {
            // Default constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationAssignmentEntity"/> class with the specified assignment ID.
        /// </summary>
        /// <param name="assignmentId">The assignment identifier.</param>
        public DestinationAssignmentEntity(string assignmentId)
        {
            AssignmentId = assignmentId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationAssignmentEntity"/> class with the specified assignment ID and name.
        /// </summary>
        /// <param name="assignmentId">The assignment identifier.</param>
        /// <param name="name">The name of the assignment.</param>
        public DestinationAssignmentEntity(string assignmentId, string name) : this(assignmentId)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationAssignmentEntity"/> class with the specified assignment ID, name, destination entity ID, and exporter service ID.
        /// </summary>
        /// <param name="assignmentId">The assignment identifier.</param>
        /// <param name="name">The name of the assignment.</param>
        /// <param name="destinationEntityId">The destination entity identifier.</param>
        /// <param name="exporterServiceId">The exporter service identifier.</param>
        public DestinationAssignmentEntity(string assignmentId, string name, string destinationEntityId, string exporterServiceId) : this(assignmentId, name)
        {
            DestinationEntityId = destinationEntityId;
            ExporterServiceId = exporterServiceId;
        }

        /// <summary>
        /// Validates the compatibility between the destination entity and the exporter service.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateCompatibility()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate destination type and exporter type compatibility
            if (string.IsNullOrWhiteSpace(DestinationType))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DESTINATION_TYPE_REQUIRED", Message = "Destination type is required." });
            }

            if (string.IsNullOrWhiteSpace(ExporterType))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "EXPORTER_TYPE_REQUIRED", Message = "Exporter type is required." });
            }

            // Check if destination type and exporter type are compatible
            if (!string.IsNullOrWhiteSpace(DestinationType) && !string.IsNullOrWhiteSpace(ExporterType))
            {
                if (!IsDestinationTypeCompatibleWithExporterType(DestinationType, ExporterType))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INCOMPATIBLE_TYPES", Message = $"Destination type '{DestinationType}' is not compatible with exporter type '{ExporterType}'." });
                }
            }

            // Validate data format
            if (string.IsNullOrWhiteSpace(DataFormat))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DATA_FORMAT_REQUIRED", Message = "Data format is required." });
            }

            // Validate batch size
            if (BatchSize <= 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "INVALID_BATCH_SIZE", Message = "Batch size must be greater than zero." });
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
        /// Validates the merge strategy configuration.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateMergeStrategy()
        {
            var result = new ValidationResult { IsValid = true };

            // If no merge strategy is specified, return valid result
            if (string.IsNullOrWhiteSpace(MergeStrategy))
            {
                return result;
            }

            // Validate merge strategy
            switch (MergeStrategy.ToUpperInvariant())
            {
                case "LAST_WRITE_WINS":
                case "FIRST_WRITE_WINS":
                case "PRIORITY_BASED":
                case "FIELD_LEVEL":
                case "CUSTOM":
                    // Valid merge strategies
                    break;
                default:
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INVALID_MERGE_STRATEGY", Message = $"Merge strategy '{MergeStrategy}' is not supported." });
                    break;
            }

            // Validate merge configuration based on the strategy
            if (MergeStrategy.Equals("PRIORITY_BASED", StringComparison.OrdinalIgnoreCase))
            {
                if (!MergeConfiguration.ContainsKey("branchPriorities"))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "MISSING_BRANCH_PRIORITIES", Message = "Branch priorities are required for PRIORITY_BASED merge strategy." });
                }
            }
            else if (MergeStrategy.Equals("FIELD_LEVEL", StringComparison.OrdinalIgnoreCase))
            {
                if (!MergeConfiguration.ContainsKey("fieldMappings"))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "MISSING_FIELD_MAPPINGS", Message = "Field mappings are required for FIELD_LEVEL merge strategy." });
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified destination type is compatible with the specified exporter type.
        /// </summary>
        /// <param name="destinationType">The destination type.</param>
        /// <param name="exporterType">The exporter type.</param>
        /// <returns>True if the destination type is compatible with the exporter type, otherwise false.</returns>
        private bool IsDestinationTypeCompatibleWithExporterType(string destinationType, string exporterType)
        {
            // This is a simplified compatibility check
            // In a real implementation, this would be more sophisticated
            
            // Check for direct match
            if (destinationType.Equals(exporterType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check for known compatible pairs
            if (destinationType.Equals("file", StringComparison.OrdinalIgnoreCase) && 
                (exporterType.Equals("file", StringComparison.OrdinalIgnoreCase) || 
                 exporterType.Equals("csv", StringComparison.OrdinalIgnoreCase) || 
                 exporterType.Equals("json", StringComparison.OrdinalIgnoreCase) || 
                 exporterType.Equals("xml", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (destinationType.Equals("database", StringComparison.OrdinalIgnoreCase) && 
                exporterType.Equals("database", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (destinationType.Equals("rest", StringComparison.OrdinalIgnoreCase) && 
                exporterType.Equals("rest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Represents the conflict resolution strategy for a destination assignment.
    /// </summary>
    public class ConflictResolutionStrategy
    {
        /// <summary>
        /// Gets or sets the conflict resolution mode.
        /// </summary>
        public string Mode { get; set; } = "LAST_WRITE_WINS";

        /// <summary>
        /// Gets or sets the conflict detection fields.
        /// </summary>
        public List<string> ConflictDetectionFields { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to log conflicts.
        /// </summary>
        public bool LogConflicts { get; set; } = true;

        /// <summary>
        /// Gets or sets the conflict notification email.
        /// </summary>
        public string? ConflictNotificationEmail { get; set; }
    }
}
