namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents a field in a schema definition.
    /// </summary>
    public class SchemaField
    {
        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the field is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the field description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default value for the field.
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the validation rules for the field.
        /// </summary>
        public Dictionary<string, object> ValidationRules { get; set; } = new Dictionary<string, object>();
    }
}
