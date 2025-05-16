using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Defines the interface for processing chain entities in the FlowOrchestrator system.
    /// </summary>
    public interface IProcessingChainEntity : IEntity, IService
    {
        /// <summary>
        /// Gets or sets the processing chain identifier.
        /// </summary>
        string ChainId { get; set; }

        /// <summary>
        /// Gets or sets the name of the processing chain.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the processing chain.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the owner of the processing chain.
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// Gets or sets the tags for the processing chain.
        /// </summary>
        List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the processor service identifiers.
        /// </summary>
        List<string> ProcessorServiceIds { get; set; }

        /// <summary>
        /// Gets or sets the processing chain configuration.
        /// </summary>
        Dictionary<string, object> Configuration { get; set; }

        /// <summary>
        /// Gets or sets the processing chain metadata.
        /// </summary>
        Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the processing chain is enabled.
        /// </summary>
        bool IsEnabled { get; set; }
    }
}
