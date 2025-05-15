namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents a schema definition for data validation.
    /// </summary>
    public class SchemaDefinition
    {
        /// <summary>
        /// Gets or sets the schema identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema format.
        /// </summary>
        public SchemaFormat Format { get; set; } = SchemaFormat.JSON;

        /// <summary>
        /// Gets or sets the fields in the schema.
        /// </summary>
        public List<SchemaField> Fields { get; set; } = new List<SchemaField>();

        /// <summary>
        /// Gets or sets the nested schemas.
        /// </summary>
        public Dictionary<string, SchemaDefinition> NestedSchemas { get; set; } = new Dictionary<string, SchemaDefinition>();

        /// <summary>
        /// Gets or sets the timestamp when the schema was created.
        /// </summary>
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the schema was last modified.
        /// </summary>
        public DateTime LastModifiedTimestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents the format of a schema.
    /// </summary>
    public enum SchemaFormat
    {
        /// <summary>
        /// JSON Schema format.
        /// </summary>
        JSON,

        /// <summary>
        /// XML Schema Definition (XSD) format.
        /// </summary>
        XSD,

        /// <summary>
        /// Avro Schema format.
        /// </summary>
        AVRO,

        /// <summary>
        /// Protocol Buffers (Protobuf) format.
        /// </summary>
        PROTOBUF,

        /// <summary>
        /// Custom schema format.
        /// </summary>
        CUSTOM
    }
}
