using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Processing.Validation;
using System.Text.Json;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Processing.Examples
{
    /// <summary>
    /// Sample implementation of a processor service.
    /// </summary>
    public class SampleProcessor : AbstractProcessorService
    {
        private readonly ILogger<SampleProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleProcessor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public SampleProcessor(ILogger<SampleProcessor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "sample-processor";

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
                SupportedTransformationTypes = new List<string> { "data-transformation" },
                SupportedSchemaFormats = new List<SchemaFormat> { SchemaFormat.JSON },
                SupportsValidation = true,
                SupportsStreaming = false,
                SupportsBatching = true
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
                Id = "sample-processor-input",
                Name = "SampleProcessorInput",
                Version = "1.0.0",
                Description = "Input schema for the sample processor",
                Fields = new List<SchemaField>
                {
                    new SchemaField
                    {
                        Name = "name",
                        Type = "string",
                        Required = true,
                        ValidationRules = new Dictionary<string, object>
                        {
                            { "minLength", 1 },
                            { "maxLength", 100 }
                        }
                    },
                    new SchemaField
                    {
                        Name = "age",
                        Type = "integer",
                        Required = true,
                        ValidationRules = new Dictionary<string, object>
                        {
                            { "min", 0 },
                            { "max", 120 }
                        }
                    },
                    new SchemaField
                    {
                        Name = "email",
                        Type = "string",
                        Required = false,
                        ValidationRules = new Dictionary<string, object>
                        {
                            { "pattern", @"^[^@\s]+@[^@\s]+\.[^@\s]+$" }
                        }
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
                Id = "sample-processor-output",
                Name = "SampleProcessorOutput",
                Version = "1.0.0",
                Description = "Output schema for the sample processor",
                Fields = new List<SchemaField>
                {
                    new SchemaField
                    {
                        Name = "fullName",
                        Type = "string",
                        Required = true
                    },
                    new SchemaField
                    {
                        Name = "ageGroup",
                        Type = "string",
                        Required = true
                    },
                    new SchemaField
                    {
                        Name = "contactInfo",
                        Type = "object",
                        Required = false
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
            if (parameters == null)
            {
                return ValidationResult.Error("Configuration parameters cannot be null.");
            }

            var result = new ValidationResult { IsValid = true };

            // Add any specific configuration validation logic here

            return result;
        }

        /// <summary>
        /// Consumes a process command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Consume(ConsumeContext<ProcessCommand> context)
        {
            if (context == null || context.Message == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _logger.LogInformation("Received process command: {MessageId}", context.MessageId);

            try
            {
                var command = context.Message;
                var result = Process(command.Parameters, command.Context);

                // In a real implementation, you would publish the result to a message broker
                _logger.LogInformation("Processing completed successfully");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing command");
                throw;
            }
        }

        /// <summary>
        /// Performs the actual processing.
        /// </summary>
        /// <param name="parameters">The process parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The processing result.</returns>
        protected override ProcessingResult PerformProcessing(ProcessParameters parameters, ExecutionContext context)
        {
            _logger.LogInformation("Processing data for execution: {ExecutionId}", context.ExecutionId);

            var result = new ProcessingResult();

            try
            {
                // Extract input data
                var inputData = parameters.InputData;
                var inputContent = JsonSerializer.Deserialize<JsonElement>(inputData.Content.ToString());

                // Transform data
                var transformedData = new Dictionary<string, object>();

                // Extract name and age
                string name = inputContent.GetProperty("name").GetString();
                int age = inputContent.GetProperty("age").GetInt32();

                // Transform data
                transformedData["fullName"] = name;
                transformedData["ageGroup"] = GetAgeGroup(age);

                // Add contact info if email is present
                if (inputContent.TryGetProperty("email", out var emailElement))
                {
                    transformedData["contactInfo"] = new Dictionary<string, object>
                    {
                        { "email", emailElement.GetString() }
                    };
                }

                // Create output data package
                result.TransformedData = new DataPackage
                {
                    Content = transformedData,
                    ContentType = "application/json",
                    SchemaId = GetOutputSchema().Id,
                    SchemaVersion = GetOutputSchema().Version,
                    CreatedTimestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "processorId", ServiceId },
                        { "processorVersion", Version }
                    }
                };

                result.Success = true;
                result.TransformationMetadata = new Dictionary<string, object>
                {
                    { "transformationTime", DateTime.UtcNow },
                    { "transformationType", "data-transformation" }
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data");
                throw;
            }
        }

        /// <summary>
        /// Validates data against a schema definition.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <param name="schema">The schema to validate against.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateDataAgainstSchema(DataPackage data, SchemaDefinition schema)
        {
            return SchemaValidator.ValidateDataAgainstSchema(data, schema);
        }

        /// <summary>
        /// Classifies an exception into an error code.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The error code.</returns>
        protected override string ClassifyException(Exception ex)
        {
            if (ex is JsonException)
            {
                return "JSON_PARSING_ERROR";
            }
            else if (ex is ArgumentException)
            {
                return "INVALID_ARGUMENT";
            }
            else if (ex is InvalidOperationException)
            {
                return "INVALID_OPERATION";
            }
            else
            {
                return "GENERAL_ERROR";
            }
        }

        /// <summary>
        /// Gets detailed information about an exception.
        /// </summary>
        /// <param name="ex">The exception to get details for.</param>
        /// <returns>The error details.</returns>
        protected override Dictionary<string, object> GetErrorDetails(Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "message", ex.Message },
                { "stackTrace", ex.StackTrace },
                { "source", ex.Source },
                { "type", ex.GetType().Name }
            };
        }

        /// <summary>
        /// Attempts to recover from an error state.
        /// </summary>
        protected override void TryRecover()
        {
            _logger.LogInformation("Attempting to recover from error state");

            // In a real implementation, you would add recovery logic here

            _state = ServiceState.READY;
        }

        /// <summary>
        /// Called when the service is being initialized.
        /// </summary>
        protected override void OnInitialize()
        {
            _logger.LogInformation("Initializing sample processor");
        }

        /// <summary>
        /// Called when the service is ready.
        /// </summary>
        protected override void OnReady()
        {
            _logger.LogInformation("Sample processor is ready");
        }

        /// <summary>
        /// Called when the service is processing data.
        /// </summary>
        protected override void OnProcessing()
        {
            _logger.LogInformation("Sample processor is processing data");
        }

        /// <summary>
        /// Called when the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        protected override void OnError(Exception ex)
        {
            _logger.LogError(ex, "Sample processor encountered an error");
        }

        /// <summary>
        /// Called when the service is being terminated.
        /// </summary>
        protected override void OnTerminate()
        {
            _logger.LogInformation("Terminating sample processor");
        }

        /// <summary>
        /// Gets the age group for a given age.
        /// </summary>
        /// <param name="age">The age.</param>
        /// <returns>The age group.</returns>
        private string GetAgeGroup(int age)
        {
            if (age < 18)
            {
                return "Minor";
            }
            else if (age < 65)
            {
                return "Adult";
            }
            else
            {
                return "Senior";
            }
        }
    }

    /// <summary>
    /// Logger interface for the sample processor.
    /// </summary>
    /// <typeparam name="T">The type to log for.</typeparam>
    public interface ILogger<T>
    {
        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">The message arguments.</param>
        void LogInformation(string message, params object[] args);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">The message arguments.</param>
        void LogError(Exception ex, string message, params object[] args);
    }
}
