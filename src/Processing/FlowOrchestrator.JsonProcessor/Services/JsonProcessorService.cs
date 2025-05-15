using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Processing.Json.Configuration;
using FlowOrchestrator.Processing.Json.Models;
using FlowOrchestrator.Processing.Json.Utilities;
using FlowOrchestrator.Processing.Json.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;
using ValidationError = FlowOrchestrator.Abstractions.Common.ValidationError;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Processing.Json.Services
{
    /// <summary>
    /// Service for processing and transforming JSON data.
    /// </summary>
    public class JsonProcessorService : AbstractProcessorService
    {
        private readonly ILogger<JsonProcessorService> _logger;
        private readonly JsonProcessorConfiguration _jsonConfiguration;
        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonProcessorService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public JsonProcessorService(
            ILogger<JsonProcessorService> logger,
            IOptions<JsonProcessorConfiguration> options)
        {
            _logger = logger;
            _jsonConfiguration = options.Value;
            _serializerOptions = _jsonConfiguration.CreateSerializerOptions();
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => _jsonConfiguration.ServiceId;

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the capabilities of the processor service.
        /// </summary>
        /// <returns>The processor capabilities.</returns>
        public override ProcessorCapabilities GetCapabilities()
        {
            return new ProcessorCapabilities
            {
                SupportedInputFormats = new List<string> { "application/json" },
                SupportedOutputFormats = new List<string> { "application/json" },
                SupportedTransformationTypes = new List<string>
                {
                    "json-mapping",
                    "json-filtering",
                    "json-extraction",
                    "json-merging",
                    "json-flattening",
                    "json-custom"
                },
                SupportedSchemaFormats = new List<SchemaFormat> { SchemaFormat.JSON },
                SupportsValidation = true,
                SupportsFiltering = true,
                SupportsEnrichment = true,
                MaxBatchSize = 100
            };
        }

        /// <summary>
        /// Gets the input schema for the processor.
        /// </summary>
        /// <returns>The input schema definition.</returns>
        public override SchemaDefinition GetInputSchema()
        {
            return new SchemaDefinition
            {
                Id = "json-processor-input",
                Name = "JsonProcessorInput",
                Version = "1.0.0",
                Description = "Input schema for the JSON processor",
                Format = SchemaFormat.JSON,
                Fields = new List<SchemaField>
                {
                    new SchemaField
                    {
                        Name = "data",
                        Type = "object",
                        Required = true,
                        Description = "The JSON data to process"
                    }
                }
            };
        }

        /// <summary>
        /// Gets the output schema for the processor.
        /// </summary>
        /// <returns>The output schema definition.</returns>
        public override SchemaDefinition GetOutputSchema()
        {
            return new SchemaDefinition
            {
                Id = "json-processor-output",
                Name = "JsonProcessorOutput",
                Version = "1.0.0",
                Description = "Output schema for the JSON processor",
                Format = SchemaFormat.JSON,
                Fields = new List<SchemaField>
                {
                    new SchemaField
                    {
                        Name = "data",
                        Type = "object",
                        Required = true,
                        Description = "The processed JSON data"
                    },
                    new SchemaField
                    {
                        Name = "metadata",
                        Type = "object",
                        Required = false,
                        Description = "Metadata about the transformation"
                    }
                }
            };
        }

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateConfiguration(ConfigurationParameters parameters)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate service ID
            if (string.IsNullOrWhiteSpace(parameters.GetParameter<string>("ServiceId")))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "MISSING_SERVICE_ID",
                    Message = "Service ID is required."
                });
            }

            // Validate max depth
            if (parameters.Parameters.ContainsKey("MaxDepth") && parameters.GetParameter<int>("MaxDepth") <= 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_MAX_DEPTH",
                    Message = "Max depth must be greater than 0."
                });
            }

            // Validate max string length
            if (parameters.Parameters.ContainsKey("MaxStringLength") && parameters.GetParameter<int>("MaxStringLength") <= 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_MAX_STRING_LENGTH",
                    Message = "Max string length must be greater than 0."
                });
            }

            return result;
        }

        /// <summary>
        /// Validates the process parameters.
        /// </summary>
        /// <param name="parameters">The process parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateParameters(ProcessParameters parameters)
        {
            var result = base.ValidateParameters(parameters);

            if (!result.IsValid)
            {
                return result;
            }

            // Validate content type
            if (parameters.InputData != null &&
                !string.IsNullOrEmpty(parameters.InputData.ContentType) &&
                !parameters.InputData.ContentType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "UNSUPPORTED_CONTENT_TYPE",
                    Message = $"Content type '{parameters.InputData.ContentType}' is not supported. Expected 'application/json'."
                });
            }

            // Validate transformation options
            if (parameters.Options.TryGetValue("TransformationType", out var transformationType) && transformationType is string typeStr)
            {
                if (!Enum.TryParse<TransformationType>(typeStr, true, out var _))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        Code = "INVALID_TRANSFORMATION_TYPE",
                        Message = $"Transformation type '{typeStr}' is not supported."
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Performs the actual processing.
        /// </summary>
        /// <param name="parameters">The process parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The processing result.</returns>
        protected override ProcessingResult PerformProcessing(ProcessParameters parameters, ExecutionContext context)
        {
            _logger.LogInformation("Processing JSON data");
            var result = new ProcessingResult();

            try
            {
                // Extract input data
                var inputData = parameters.InputData;
                JsonNode? jsonData;

                // Parse the input data
                if (inputData.Content is JsonNode node)
                {
                    jsonData = node;
                }
                else if (inputData.Content is string jsonString)
                {
                    jsonData = JsonNode.Parse(jsonString);
                }
                else
                {
                    // Serialize to JSON and parse
                    var serialized = JsonSerializer.Serialize(inputData.Content, _serializerOptions);
                    jsonData = JsonNode.Parse(serialized);
                }

                if (jsonData == null)
                {
                    throw new InvalidOperationException("Failed to parse input data as JSON.");
                }

                // Extract transformation options from parameters
                var transformationOptions = ExtractTransformationOptions(parameters.Options);

                // Transform the data
                JsonNode transformedData = TransformJson(jsonData, transformationOptions);

                // Create the result data package
                result.TransformedData = new DataPackage
                {
                    Content = transformedData,
                    ContentType = "application/json",
                    SchemaId = GetOutputSchema().Id,
                    SchemaVersion = GetOutputSchema().Version,
                    CreatedTimestamp = DateTime.UtcNow
                };

                // Add transformation metadata
                result.TransformationMetadata = new Dictionary<string, object>
                {
                    { "TransformationType", transformationOptions.TransformationType.ToString() },
                    { "ProcessedAt", DateTime.UtcNow },
                    { "ProcessorId", ServiceId },
                    { "ProcessorVersion", Version }
                };

                result.Success = true;
                _logger.LogInformation("JSON processing completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing JSON data: {ErrorMessage}", ex.Message);
                result.Success = false;
                result.ValidationResults = ValidationResult.Error(
                    ClassifyException(ex),
                    $"Error processing JSON data: {ex.Message}"
                );

                if (result.ValidationResults.Errors.Count > 0 && result.ValidationResults.Errors[0] is ValidationError error)
                {
                    error.Details = GetErrorDetails(ex);
                }
            }

            return result;
        }

        /// <summary>
        /// Transforms JSON data based on the specified options.
        /// </summary>
        /// <param name="jsonData">The JSON data to transform.</param>
        /// <param name="options">The transformation options.</param>
        /// <returns>The transformed JSON data.</returns>
        private JsonNode TransformJson(JsonNode jsonData, JsonTransformationOptions options)
        {
            _logger.LogDebug("Transforming JSON data with transformation type: {TransformationType}", options.TransformationType);

            // Use the JsonTransformationEngine to transform the data
            return JsonTransformationEngine.TransformJson(jsonData, options);
        }

        /// <summary>
        /// Extracts transformation options from the process parameters.
        /// </summary>
        /// <param name="options">The process options.</param>
        /// <returns>The transformation options.</returns>
        private JsonTransformationOptions ExtractTransformationOptions(Dictionary<string, object> options)
        {
            var transformationOptions = new JsonTransformationOptions();

            // Extract transformation type
            if (options.TryGetValue("TransformationType", out var transformationType) && transformationType is string typeStr)
            {
                if (Enum.TryParse<TransformationType>(typeStr, true, out var type))
                {
                    transformationOptions.TransformationType = type;
                }
            }

            // Extract field mappings
            if (options.TryGetValue("FieldMappings", out var fieldMappings) && fieldMappings is Dictionary<string, string> mappings)
            {
                transformationOptions.FieldMappings = mappings;
            }

            // Extract include fields
            if (options.TryGetValue("IncludeFields", out var includeFields) && includeFields is List<string> includes)
            {
                transformationOptions.IncludeFields = includes;
            }

            // Extract exclude fields
            if (options.TryGetValue("ExcludeFields", out var excludeFields) && excludeFields is List<string> excludes)
            {
                transformationOptions.ExcludeFields = excludes;
            }

            // Extract path expressions
            if (options.TryGetValue("PathExpressions", out var pathExpressions) && pathExpressions is Dictionary<string, string> paths)
            {
                transformationOptions.PathExpressions = paths;
            }

            // Extract default values
            if (options.TryGetValue("DefaultValues", out var defaultValues) && defaultValues is Dictionary<string, object> defaults)
            {
                transformationOptions.DefaultValues = defaults;
            }

            // Extract field transformations
            if (options.TryGetValue("FieldTransformations", out var fieldTransformations) && fieldTransformations is Dictionary<string, string> transforms)
            {
                transformationOptions.FieldTransformations = transforms;
            }

            // Extract flatten options
            if (options.TryGetValue("FlattenObjects", out var flattenObjects) && flattenObjects is bool flatten)
            {
                transformationOptions.FlattenObjects = flatten;
            }

            if (options.TryGetValue("FlattenDelimiter", out var flattenDelimiter) && flattenDelimiter is string delimiter)
            {
                transformationOptions.FlattenDelimiter = delimiter;
            }

            // Extract preservation options
            if (options.TryGetValue("PreserveNullValues", out var preserveNullValues) && preserveNullValues is bool preserveNull)
            {
                transformationOptions.PreserveNullValues = preserveNull;
            }

            if (options.TryGetValue("PreserveEmptyArrays", out var preserveEmptyArrays) && preserveEmptyArrays is bool preserveArrays)
            {
                transformationOptions.PreserveEmptyArrays = preserveArrays;
            }

            if (options.TryGetValue("PreserveEmptyObjects", out var preserveEmptyObjects) && preserveEmptyObjects is bool preserveObjects)
            {
                transformationOptions.PreserveEmptyObjects = preserveObjects;
            }

            // Extract array handling mode
            if (options.TryGetValue("ArrayHandlingMode", out var arrayHandlingMode) && arrayHandlingMode is string modeStr)
            {
                if (Enum.TryParse<ArrayHandlingMode>(modeStr, true, out var mode))
                {
                    transformationOptions.ArrayHandlingMode = mode;
                }
            }

            return transformationOptions;
        }

        /// <summary>
        /// Consumes a process command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task Consume(ConsumeContext<ProcessCommand> context)
        {
            _logger.LogInformation("Received process command: {CommandId}", context.Message.CommandId);

            try
            {
                // Verify that this command is intended for this service
                if (context.Message.ProcessorServiceId != ServiceId || context.Message.ProcessorServiceVersion != Version)
                {
                    _logger.LogWarning("Received process command for different service: {ServiceId} (version {Version})",
                        context.Message.ProcessorServiceId, context.Message.ProcessorServiceVersion);
                    return Task.CompletedTask;
                }

                // Process the command
                var result = Process(context.Message.Parameters, context.Message.Context);

                // TODO: Publish the result to the message bus
                _logger.LogInformation("Process command completed: {CommandId}, Success: {Success}",
                    context.Message.CommandId, result.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing command: {CommandId}, Error: {ErrorMessage}",
                    context.Message.CommandId, ex.Message);
                // TODO: Publish error to the message bus
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Classifies an exception.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The exception classification.</returns>
        protected override string ClassifyException(Exception ex)
        {
            return ex switch
            {
                JsonException => "JSON_PARSING_ERROR",
                InvalidOperationException => "INVALID_OPERATION",
                ArgumentException => "INVALID_ARGUMENT",
                _ => "GENERAL_ERROR"
            };
        }

        /// <summary>
        /// Gets detailed information about an exception.
        /// </summary>
        /// <param name="ex">The exception to get details for.</param>
        /// <returns>The error details.</returns>
        protected override Dictionary<string, object> GetErrorDetails(Exception ex)
        {
            var details = new Dictionary<string, object>
            {
                { "ErrorType", ex.GetType().Name },
                { "ErrorMessage", ex.Message },
                { "ErrorClassification", ClassifyException(ex) },
                { "Timestamp", DateTime.UtcNow }
            };

            if (ex is JsonException jsonEx)
            {
                details["LineNumber"] = jsonEx.LineNumber;
                details["BytePositionInLine"] = jsonEx.BytePositionInLine;
                details["Path"] = jsonEx.Path ?? string.Empty;
            }

            return details;
        }

        /// <summary>
        /// Attempts to recover from an error state.
        /// </summary>
        protected override void TryRecover()
        {
            _logger.LogInformation("Attempting to recover from error state");
            // Reset any internal state if needed
            _state = ServiceState.READY;
        }

        /// <summary>
        /// Validates data against a schema definition.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <param name="schema">The schema to validate against.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateDataAgainstSchema(DataPackage data, SchemaDefinition schema)
        {
            if (data == null)
            {
                return ValidationResult.Error("Data package cannot be null.");
            }

            if (schema == null)
            {
                return ValidationResult.Error("Schema definition cannot be null.");
            }

            try
            {
                // Convert data content to JsonNode if needed
                JsonNode? jsonNode;
                if (data.Content is JsonNode node)
                {
                    jsonNode = node;
                }
                else if (data.Content is string jsonString)
                {
                    jsonNode = JsonNode.Parse(jsonString);
                }
                else
                {
                    // Serialize to JSON and parse
                    var serialized = JsonSerializer.Serialize(data.Content, _serializerOptions);
                    jsonNode = JsonNode.Parse(serialized);
                }

                if (jsonNode == null)
                {
                    return ValidationResult.Error("Failed to parse data as JSON.");
                }

                // Use the JsonSchemaValidator to validate the data
                return JsonSchemaValidator.ValidateJson(jsonNode, schema);
            }
            catch (JsonException ex)
            {
                return ValidationResult.Error($"JSON validation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ValidationResult.Error($"Validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the service is being initialized.
        /// </summary>
        protected override void OnInitialize()
        {
            _logger.LogInformation("Initializing JSON processor service");
            // No additional initialization needed
        }

        /// <summary>
        /// Called when the service is ready.
        /// </summary>
        protected override void OnReady()
        {
            _logger.LogInformation("JSON processor service is ready");
        }

        /// <summary>
        /// Called when the service is processing data.
        /// </summary>
        protected override void OnProcessing()
        {
            _logger.LogDebug("JSON processor service is processing data");
        }

        /// <summary>
        /// Called when the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        protected override void OnError(Exception ex)
        {
            _logger.LogError(ex, "JSON processor service encountered an error: {ErrorMessage}", ex.Message);
        }

        /// <summary>
        /// Called when the service is being terminated.
        /// </summary>
        protected override void OnTerminate()
        {
            _logger.LogInformation("Terminating JSON processor service");
            // No additional cleanup needed
        }
    }
}
