namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the status of a version in the FlowOrchestrator system.
    /// </summary>
    public enum VersionStatus
    {
        /// <summary>
        /// Version is in draft state and not ready for use.
        /// </summary>
        DRAFT,

        /// <summary>
        /// Version is active and can be used.
        /// </summary>
        ACTIVE,

        /// <summary>
        /// Version is deprecated but still usable.
        /// </summary>
        DEPRECATED,

        /// <summary>
        /// Version is archived and no longer usable.
        /// </summary>
        ARCHIVED,

        /// <summary>
        /// Version is disabled and cannot be used.
        /// </summary>
        DISABLED
    }
}
