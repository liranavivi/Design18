namespace FlowOrchestrator.Processing.Json.Models
{
    /// <summary>
    /// Options for JSON transformation operations.
    /// </summary>
    public class JsonTransformationOptions
    {
        /// <summary>
        /// Gets or sets the transformation type.
        /// </summary>
        public TransformationType TransformationType { get; set; } = TransformationType.Map;

        /// <summary>
        /// Gets or sets the field mappings for mapping transformations.
        /// </summary>
        public Dictionary<string, string> FieldMappings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the fields to include for filtering transformations.
        /// </summary>
        public List<string> IncludeFields { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the fields to exclude for filtering transformations.
        /// </summary>
        public List<string> ExcludeFields { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the JSON path expressions for extraction transformations.
        /// </summary>
        public Dictionary<string, string> PathExpressions { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the default values for fields that are missing or null.
        /// </summary>
        public Dictionary<string, object> DefaultValues { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the field transformations for custom transformations.
        /// </summary>
        public Dictionary<string, string> FieldTransformations { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets a value indicating whether to flatten nested objects.
        /// </summary>
        public bool FlattenObjects { get; set; } = false;

        /// <summary>
        /// Gets or sets the delimiter for flattened field names.
        /// </summary>
        public string FlattenDelimiter { get; set; } = ".";

        /// <summary>
        /// Gets or sets a value indicating whether to preserve null values.
        /// </summary>
        public bool PreserveNullValues { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to preserve empty arrays.
        /// </summary>
        public bool PreserveEmptyArrays { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to preserve empty objects.
        /// </summary>
        public bool PreserveEmptyObjects { get; set; } = true;

        /// <summary>
        /// Gets or sets the array handling mode.
        /// </summary>
        public ArrayHandlingMode ArrayHandlingMode { get; set; } = ArrayHandlingMode.Preserve;
    }

    /// <summary>
    /// Defines the type of transformation to perform.
    /// </summary>
    public enum TransformationType
    {
        /// <summary>
        /// Map fields from source to destination.
        /// </summary>
        Map,

        /// <summary>
        /// Filter fields from the source.
        /// </summary>
        Filter,

        /// <summary>
        /// Extract specific values using JSON path expressions.
        /// </summary>
        Extract,

        /// <summary>
        /// Merge multiple JSON objects.
        /// </summary>
        Merge,

        /// <summary>
        /// Flatten nested objects.
        /// </summary>
        Flatten,

        /// <summary>
        /// Apply custom transformations to fields.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Defines how arrays should be handled during transformation.
    /// </summary>
    public enum ArrayHandlingMode
    {
        /// <summary>
        /// Preserve arrays as-is.
        /// </summary>
        Preserve,

        /// <summary>
        /// Apply transformations to each array element.
        /// </summary>
        Transform,

        /// <summary>
        /// Flatten arrays into separate fields.
        /// </summary>
        Flatten
    }
}
