using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Base abstract implementation for destination entities.
    /// Defines a data destination location and delivery protocol.
    /// </summary>
    public abstract class AbstractDestinationEntity : AbstractEntity
    {
        /// <summary>
        /// Gets or sets the destination identifier.
        /// </summary>
        public string DestinationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the destination.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the destination.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the protocol used by the destination.
        /// </summary>
        public string Protocol { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the destination.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the destination metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the credentials for the destination.
        /// </summary>
        public DestinationCredentials? Credentials { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the destination is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public override string GetEntityId()
        {
            return DestinationId;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public override string GetEntityType()
        {
            return "DestinationEntity";
        }

        /// <summary>
        /// Validates the destination entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required fields
            if (string.IsNullOrWhiteSpace(DestinationId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DESTINATION_ID_REQUIRED", Message = "Destination ID is required." });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DESTINATION_NAME_REQUIRED", Message = "Destination name is required." });
            }

            if (string.IsNullOrWhiteSpace(Protocol))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "PROTOCOL_REQUIRED", Message = "Protocol is required." });
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "ADDRESS_REQUIRED", Message = "Address is required." });
            }

            // Validate protocol-specific configuration
            var protocolValidation = ValidateProtocolConfiguration();
            if (!protocolValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(protocolValidation.Errors);
            }

            return result;
        }

        /// <summary>
        /// Validates the protocol-specific configuration.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateProtocolConfiguration();

        /// <summary>
        /// Copies the properties of this entity to another entity.
        /// </summary>
        /// <param name="target">The target entity.</param>
        protected override void CopyPropertiesTo(AbstractEntity target)
        {
            base.CopyPropertiesTo(target);

            if (target is AbstractDestinationEntity destinationEntity)
            {
                destinationEntity.DestinationId = DestinationId;
                destinationEntity.Name = Name;
                destinationEntity.Description = Description;
                destinationEntity.Protocol = Protocol;
                destinationEntity.Address = Address;
                destinationEntity.Configuration = new Dictionary<string, object>(Configuration);
                destinationEntity.Metadata = new Dictionary<string, object>(Metadata);
                destinationEntity.IsEnabled = IsEnabled;
                
                // Deep copy of credentials if present
                if (Credentials != null)
                {
                    destinationEntity.Credentials = new DestinationCredentials
                    {
                        CredentialType = Credentials.CredentialType,
                        Username = Credentials.Username,
                        Password = Credentials.Password,
                        ApiKey = Credentials.ApiKey,
                        Certificate = Credentials.Certificate,
                        Properties = new Dictionary<string, string>(Credentials.Properties)
                    };
                }
            }
        }
    }

    /// <summary>
    /// Represents credentials for a destination entity.
    /// </summary>
    public class DestinationCredentials
    {
        /// <summary>
        /// Gets or sets the type of credentials.
        /// </summary>
        public string CredentialType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the certificate.
        /// </summary>
        public string? Certificate { get; set; }

        /// <summary>
        /// Gets or sets additional credential properties.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
