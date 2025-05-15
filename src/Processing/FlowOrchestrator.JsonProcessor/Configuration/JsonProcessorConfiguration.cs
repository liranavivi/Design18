using System.Text.Json;

namespace FlowOrchestrator.Processing.Json.Configuration
{
    /// <summary>
    /// Configuration options for the JSON processor service.
    /// </summary>
    public class JsonProcessorConfiguration
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = "JSON-PROCESSOR-001";

        /// <summary>
        /// Gets or sets the service type.
        /// </summary>
        public string ServiceType { get; set; } = "JsonProcessor";

        /// <summary>
        /// Gets or sets the maximum depth for JSON processing.
        /// </summary>
        public int MaxDepth { get; set; } = 64;

        /// <summary>
        /// Gets or sets the maximum string length for JSON processing.
        /// </summary>
        public int MaxStringLength { get; set; } = 1024 * 1024; // 1MB

        /// <summary>
        /// Gets or sets the JSON serializer options.
        /// </summary>
        public JsonSerializerOptions? SerializerOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow comments in JSON.
        /// </summary>
        public bool AllowComments { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to allow trailing commas in JSON.
        /// </summary>
        public bool AllowTrailingCommas { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to write indented JSON.
        /// </summary>
        public bool WriteIndented { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore null values when serializing.
        /// </summary>
        public bool IgnoreNullValues { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to use camel case naming policy.
        /// </summary>
        public bool UseCamelCase { get; set; } = true;

        /// <summary>
        /// Gets or sets the operation timeout in seconds.
        /// </summary>
        public int OperationTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the maximum retry count.
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry delay in milliseconds.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Creates JSON serializer options from the configuration.
        /// </summary>
        /// <returns>The JSON serializer options.</returns>
        public JsonSerializerOptions CreateSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                MaxDepth = MaxDepth,
                ReadCommentHandling = AllowComments ? JsonCommentHandling.Skip : JsonCommentHandling.Disallow,
                AllowTrailingCommas = AllowTrailingCommas,
                WriteIndented = WriteIndented,
                DefaultIgnoreCondition = IgnoreNullValues ? 
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull : 
                    System.Text.Json.Serialization.JsonIgnoreCondition.Never
            };

            if (UseCamelCase)
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            }

            return options;
        }
    }
}
