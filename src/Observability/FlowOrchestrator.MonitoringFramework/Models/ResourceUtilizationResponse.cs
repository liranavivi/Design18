namespace FlowOrchestrator.Observability.Monitoring.Models
{
    /// <summary>
    /// Response model for resource utilization endpoints.
    /// </summary>
    public class ResourceUtilizationResponse
    {
        /// <summary>
        /// Gets or sets the CPU utilization.
        /// </summary>
        public CpuUtilization Cpu { get; set; } = new CpuUtilization();

        /// <summary>
        /// Gets or sets the memory utilization.
        /// </summary>
        public MemoryUtilization Memory { get; set; } = new MemoryUtilization();

        /// <summary>
        /// Gets or sets the disk utilization.
        /// </summary>
        public List<DiskUtilization> Disks { get; set; } = new List<DiskUtilization>();

        /// <summary>
        /// Gets or sets the network utilization.
        /// </summary>
        public NetworkUtilization Network { get; set; } = new NetworkUtilization();

        /// <summary>
        /// Gets or sets the timestamp of the resource utilization check.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the host information.
        /// </summary>
        public HostInfo Host { get; set; } = new HostInfo();
    }

    /// <summary>
    /// CPU utilization information.
    /// </summary>
    public class CpuUtilization
    {
        /// <summary>
        /// Gets or sets the total CPU usage percentage.
        /// </summary>
        public double TotalUsagePercent { get; set; }

        /// <summary>
        /// Gets or sets the CPU usage per core.
        /// </summary>
        public List<double> CoreUsagePercent { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the number of processor cores.
        /// </summary>
        public int ProcessorCount { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets the system load average.
        /// </summary>
        public double SystemLoadAverage { get; set; }
    }

    /// <summary>
    /// Memory utilization information.
    /// </summary>
    public class MemoryUtilization
    {
        /// <summary>
        /// Gets or sets the total physical memory in bytes.
        /// </summary>
        public long TotalPhysicalMemory { get; set; }

        /// <summary>
        /// Gets or sets the available physical memory in bytes.
        /// </summary>
        public long AvailablePhysicalMemory { get; set; }

        /// <summary>
        /// Gets or sets the used physical memory in bytes.
        /// </summary>
        public long UsedPhysicalMemory { get; set; }

        /// <summary>
        /// Gets or sets the memory usage percentage.
        /// </summary>
        public double UsagePercent { get; set; }

        /// <summary>
        /// Gets or sets the total virtual memory in bytes.
        /// </summary>
        public long TotalVirtualMemory { get; set; }

        /// <summary>
        /// Gets or sets the available virtual memory in bytes.
        /// </summary>
        public long AvailableVirtualMemory { get; set; }
    }

    /// <summary>
    /// Disk utilization information.
    /// </summary>
    public class DiskUtilization
    {
        /// <summary>
        /// Gets or sets the drive name.
        /// </summary>
        public string DriveName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the drive format.
        /// </summary>
        public string DriveFormat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the drive type.
        /// </summary>
        public string DriveType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total size in bytes.
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Gets or sets the free space in bytes.
        /// </summary>
        public long FreeSpace { get; set; }

        /// <summary>
        /// Gets or sets the used space in bytes.
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// Gets or sets the usage percentage.
        /// </summary>
        public double UsagePercent { get; set; }
    }

    /// <summary>
    /// Network utilization information.
    /// </summary>
    public class NetworkUtilization
    {
        /// <summary>
        /// Gets or sets the bytes sent per second.
        /// </summary>
        public long BytesSentPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the bytes received per second.
        /// </summary>
        public long BytesReceivedPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the total bytes sent.
        /// </summary>
        public long TotalBytesSent { get; set; }

        /// <summary>
        /// Gets or sets the total bytes received.
        /// </summary>
        public long TotalBytesReceived { get; set; }
    }

    /// <summary>
    /// Host information.
    /// </summary>
    public class HostInfo
    {
        /// <summary>
        /// Gets or sets the machine name.
        /// </summary>
        public string MachineName { get; set; } = Environment.MachineName;

        /// <summary>
        /// Gets or sets the operating system.
        /// </summary>
        public string OperatingSystem { get; set; } = Environment.OSVersion.ToString();

        /// <summary>
        /// Gets or sets the runtime version.
        /// </summary>
        public string RuntimeVersion { get; set; } = Environment.Version.ToString();

        /// <summary>
        /// Gets or sets the process uptime.
        /// </summary>
        public TimeSpan ProcessUptime { get; set; }

        /// <summary>
        /// Gets or sets the system uptime.
        /// </summary>
        public TimeSpan SystemUptime { get; set; }
    }
}
