namespace FlowOrchestrator.Common.Exceptions;

/// <summary>
/// Exception thrown when configuration is invalid.
/// </summary>
public class ConfigurationException : FlowOrchestratorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    public ConfigurationException() 
        : this("Configuration is invalid.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConfigurationException(string message) 
        : base(message, ErrorClassification.CONFIGURATION_ERROR)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConfigurationException(string message, Exception innerException) 
        : base(message, ErrorClassification.CONFIGURATION_ERROR, innerException)
    {
    }
}
