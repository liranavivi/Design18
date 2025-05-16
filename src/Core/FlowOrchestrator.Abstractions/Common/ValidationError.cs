using System.Collections.Generic;

namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the severity of a validation error.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// The validation error is an information.
        /// </summary>
        INFO,

        /// <summary>
        /// The validation error is a warning.
        /// </summary>
        WARNING,

        /// <summary>
        /// The validation error is an error.
        /// </summary>
        ERROR,

        /// <summary>
        /// The validation error is a critical error.
        /// </summary>
        CRITICAL
    }
}
