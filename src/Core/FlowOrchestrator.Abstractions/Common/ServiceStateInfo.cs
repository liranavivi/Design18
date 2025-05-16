using System;
using System.Collections.Generic;

namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the state information of a service.
    /// </summary>
    public class ServiceStateInfo
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public ServiceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the last updated timestamp.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
    }
}
