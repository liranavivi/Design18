namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents a package of data that flows through the system.
    /// </summary>
    public class DataPackage
    {
        /// <summary>
        /// Gets or sets the unique identifier for the data package.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the data content.
        /// </summary>
        public object Content { get; set; } = new object();

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema identifier for the data.
        /// </summary>
        public string? SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the schema version for the data.
        /// </summary>
        public string? SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the data package was created.
        /// </summary>
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the source information for the data.
        /// </summary>
        public SourceInformation? Source { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the data package.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the validation results for the data package.
        /// </summary>
        public ValidationResult? ValidationResults { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="DataPackage"/> class with the specified content and content type.
        /// </summary>
        /// <param name="content">The data content.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>A new data package.</returns>
        public static DataPackage Create(object content, string contentType)
        {
            return new DataPackage
            {
                Content = content,
                ContentType = contentType
            };
        }

        /// <summary>
        /// Gets the content as the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast the content to.</typeparam>
        /// <returns>The content as the specified type.</returns>
        /// <exception cref="InvalidCastException">Thrown when the content cannot be cast to the specified type.</exception>
        public T GetContent<T>()
        {
            try
            {
                return (T)Content;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Content cannot be cast to type {typeof(T).Name}.");
            }
        }

        /// <summary>
        /// Tries to get the content as the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast the content to.</typeparam>
        /// <param name="content">The content as the specified type if successful; otherwise, the default value for the type.</param>
        /// <returns>true if the content was successfully cast to the specified type; otherwise, false.</returns>
        public bool TryGetContent<T>(out T content)
        {
            content = default!;

            try
            {
                content = (T)Content;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Represents information about the source of data.
    /// </summary>
    public class SourceInformation
    {
        /// <summary>
        /// Gets or sets the source identifier.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source type.
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source location.
        /// </summary>
        public string SourceLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source configuration.
        /// </summary>
        public ConfigurationParameters? Configuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformation"/> class.
        /// </summary>
        public SourceInformation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformation"/> class with the specified configuration.
        /// </summary>
        /// <param name="configuration">The source configuration.</param>
        public SourceInformation(ConfigurationParameters configuration)
        {
            Configuration = configuration;
            
            if (configuration.TryGetParameter<string>("SourceId", out var sourceId))
            {
                SourceId = sourceId;
            }
            
            if (configuration.TryGetParameter<string>("SourceType", out var sourceType))
            {
                SourceType = sourceType;
            }
            
            if (configuration.TryGetParameter<string>("SourceLocation", out var sourceLocation))
            {
                SourceLocation = sourceLocation;
            }
        }
    }
}
