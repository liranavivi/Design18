using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Processing.Validation;
using System.Text.Json;
using System.Text.Json.Nodes;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;

namespace FlowOrchestrator.Processing.Json.Validation
{
    /// <summary>
    /// Provides JSON schema validation functionality.
    /// </summary>
    public class JsonSchemaValidator
    {
        /// <summary>
        /// Validates JSON data against a schema definition.
        /// </summary>
        /// <param name="jsonData">The JSON data to validate.</param>
        /// <param name="schema">The schema to validate against.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult ValidateJson(JsonNode jsonData, SchemaDefinition schema)
        {
            if (jsonData == null)
            {
                return ValidationResult.Error("JSON data cannot be null.");
            }

            if (schema == null)
            {
                return ValidationResult.Error("Schema definition cannot be null.");
            }

            // Convert JsonNode to DataPackage for validation
            var dataPackage = new DataPackage
            {
                Content = jsonData,
                ContentType = "application/json",
                SchemaId = schema.Id,
                SchemaVersion = schema.Version
            };

            // Use the SchemaValidator from ProcessorBase
            return SchemaValidator.ValidateDataAgainstSchema(dataPackage, schema);
        }

        /// <summary>
        /// Validates a JSON string against a schema definition.
        /// </summary>
        /// <param name="jsonString">The JSON string to validate.</param>
        /// <param name="schema">The schema to validate against.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult ValidateJsonString(string jsonString, SchemaDefinition schema)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return ValidationResult.Error("JSON string cannot be null or empty.");
            }

            try
            {
                var jsonNode = JsonNode.Parse(jsonString);
                return ValidateJson(jsonNode, schema);
            }
            catch (JsonException ex)
            {
                return ValidationResult.Error($"Invalid JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a schema definition from a JSON schema string.
        /// </summary>
        /// <param name="jsonSchema">The JSON schema string.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="schemaName">The schema name.</param>
        /// <param name="schemaVersion">The schema version.</param>
        /// <returns>The schema definition.</returns>
        public static SchemaDefinition CreateSchemaFromJson(string jsonSchema, string schemaId, string schemaName, string schemaVersion)
        {
            if (string.IsNullOrWhiteSpace(jsonSchema))
            {
                throw new ArgumentException("JSON schema cannot be null or empty.", nameof(jsonSchema));
            }

            try
            {
                var jsonNode = JsonNode.Parse(jsonSchema);
                return CreateSchemaFromJsonNode(jsonNode, schemaId, schemaName, schemaVersion);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON schema: {ex.Message}", nameof(jsonSchema), ex);
            }
        }

        /// <summary>
        /// Creates a schema definition from a JSON schema node.
        /// </summary>
        /// <param name="jsonSchema">The JSON schema node.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="schemaName">The schema name.</param>
        /// <param name="schemaVersion">The schema version.</param>
        /// <returns>The schema definition.</returns>
        public static SchemaDefinition CreateSchemaFromJsonNode(JsonNode jsonSchema, string schemaId, string schemaName, string schemaVersion)
        {
            if (jsonSchema == null)
            {
                throw new ArgumentNullException(nameof(jsonSchema));
            }

            var schema = new SchemaDefinition
            {
                Id = schemaId,
                Name = schemaName,
                Version = schemaVersion,
                Description = jsonSchema["description"]?.GetValue<string>() ?? string.Empty,
                Content = jsonSchema.ToJsonString(),
                Format = SchemaFormat.JSON,
                CreatedTimestamp = DateTime.UtcNow,
                LastModifiedTimestamp = DateTime.UtcNow
            };

            // Extract fields from the properties object
            if (jsonSchema["properties"] is JsonObject properties)
            {
                var fields = new List<SchemaField>();
                var requiredFields = jsonSchema["required"] is JsonArray required
                    ? required.Select(r => r.GetValue<string>()).ToList()
                    : new List<string>();

                foreach (var property in properties)
                {
                    var fieldName = property.Key;
                    var fieldDef = property.Value as JsonObject;

                    if (fieldDef != null)
                    {
                        var field = new SchemaField
                        {
                            Name = fieldName,
                            Type = fieldDef["type"]?.GetValue<string>() ?? "string",
                            Required = requiredFields.Contains(fieldName),
                            Description = fieldDef["description"]?.GetValue<string>() ?? string.Empty
                        };

                        // Extract validation rules
                        var validationRules = new Dictionary<string, object>();
                        foreach (var rule in fieldDef)
                        {
                            if (rule.Key != "type" && rule.Key != "description")
                            {
                                validationRules[rule.Key] = rule.Value?.GetValue<object>() ?? string.Empty;
                            }
                        }

                        field.ValidationRules = validationRules;
                        fields.Add(field);
                    }
                }

                schema.Fields = fields;
            }

            return schema;
        }
    }
}
