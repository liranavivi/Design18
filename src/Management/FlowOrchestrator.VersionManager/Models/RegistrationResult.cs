using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Versioning.Models
{
    /// <summary>
    /// Represents the result of a version registration operation.
    /// </summary>
    public class RegistrationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the registration was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the registration result.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the component identifier.
        /// </summary>
        [JsonPropertyName("componentId")]
        public string ComponentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the errors associated with the registration result.
        /// </summary>
        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationResult"/> class.
        /// </summary>
        public RegistrationResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationResult"/> class.
        /// </summary>
        /// <param name="success">A value indicating whether the registration was successful.</param>
        /// <param name="message">The message associated with the registration result.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        public RegistrationResult(bool success, string message, string componentId, string version)
        {
            Success = success;
            Message = message;
            ComponentId = componentId;
            Version = version;
        }

        /// <summary>
        /// Creates a successful registration result.
        /// </summary>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="message">The message associated with the registration result.</param>
        /// <returns>A successful registration result.</returns>
        public static RegistrationResult CreateSuccess(string componentId, string version, string message = "Registration successful")
        {
            return new RegistrationResult(true, message, componentId, version);
        }

        /// <summary>
        /// Creates a failed registration result.
        /// </summary>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="message">The message associated with the registration result.</param>
        /// <param name="errors">The errors associated with the registration result.</param>
        /// <returns>A failed registration result.</returns>
        public static RegistrationResult CreateFailure(string componentId, string version, string message, params string[] errors)
        {
            var result = new RegistrationResult(false, message, componentId, version);
            if (errors != null && errors.Length > 0)
            {
                result.Errors.AddRange(errors);
            }
            return result;
        }
    }
}
