using FlowOrchestrator.Observability.Monitoring.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Implementation of the resource utilization monitor service.
    /// </summary>
    public class ResourceUtilizationMonitorService : IResourceUtilizationMonitorService
    {
        private readonly ILogger<ResourceUtilizationMonitorService> _logger;
        private readonly IOptions<MonitoringOptions> _options;
        private readonly List<ResourceUtilizationResponse> _history = new();
        private readonly Dictionary<string, double> _thresholds = new();
        private readonly Process _currentProcess;
        private readonly PerformanceCounter? _cpuCounter;
        private readonly PerformanceCounter? _memCounter;
        private readonly PerformanceCounter? _diskReadCounter;
        private readonly PerformanceCounter? _diskWriteCounter;
        private readonly PerformanceCounter? _networkSentCounter;
        private readonly PerformanceCounter? _networkReceivedCounter;
        private DateTime _lastHistoryCleanup = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceUtilizationMonitorService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The monitoring options.</param>
        public ResourceUtilizationMonitorService(
            ILogger<ResourceUtilizationMonitorService> logger,
            IOptions<MonitoringOptions> options)
        {
            _logger = logger;
            _options = options;
            _currentProcess = Process.GetCurrentProcess();

            // Initialize thresholds
            _thresholds["CPU"] = options.Value.CpuUsageThresholdPercent;
            _thresholds["Memory"] = options.Value.MemoryUsageThresholdPercent;
            _thresholds["Disk"] = options.Value.DiskUsageThresholdPercent;

            // Initialize performance counters if on Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
                    _diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
                    _diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
                    _networkSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", "_Total");
                    _networkReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", "_Total");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error initializing performance counters");
                }
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CheckResourceThresholdsAsync()
        {
            var utilization = await GetResourceUtilizationAsync();

            if (utilization.Cpu.TotalUsagePercent > _thresholds["CPU"])
            {
                _logger.LogWarning("CPU usage ({CpuUsage}%) exceeds threshold ({Threshold}%)",
                    utilization.Cpu.TotalUsagePercent, _thresholds["CPU"]);
                return true;
            }

            if (utilization.Memory.UsagePercent > _thresholds["Memory"])
            {
                _logger.LogWarning("Memory usage ({MemoryUsage}%) exceeds threshold ({Threshold}%)",
                    utilization.Memory.UsagePercent, _thresholds["Memory"]);
                return true;
            }

            foreach (var disk in utilization.Disks)
            {
                if (disk.UsagePercent > _thresholds["Disk"])
                {
                    _logger.LogWarning("Disk usage for {DriveName} ({DiskUsage}%) exceeds threshold ({Threshold}%)",
                        disk.DriveName, disk.UsagePercent, _thresholds["Disk"]);
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public Dictionary<string, double> GetResourceThresholds()
        {
            return new Dictionary<string, double>(_thresholds);
        }

        /// <inheritdoc/>
        public async Task<ResourceUtilizationResponse> GetResourceUtilizationAsync()
        {
            var response = new ResourceUtilizationResponse
            {
                Timestamp = DateTime.UtcNow
            };

            try
            {
                // Get CPU utilization
                response.Cpu = GetCpuUtilization();

                // Get memory utilization
                response.Memory = GetMemoryUtilization();

                // Get disk utilization
                response.Disks = GetDiskUtilization();

                // Get network utilization
                response.Network = GetNetworkUtilization();

                // Get host information
                response.Host = GetHostInfo();

                // Add to history
                _history.Add(response);

                // Clean up old history entries
                CleanupHistory();

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource utilization");
                return response;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ResourceUtilizationResponse>> GetResourceUtilizationHistoryAsync(TimeSpan timeSpan, int intervalSeconds = 60)
        {
            var cutoffTime = DateTime.UtcNow.Subtract(timeSpan);
            var filteredHistory = _history.Where(h => h.Timestamp >= cutoffTime).ToList();

            // If we don't have enough history, get current utilization
            if (!filteredHistory.Any())
            {
                filteredHistory.Add(await GetResourceUtilizationAsync());
            }

            // If interval is specified, sample the history
            if (intervalSeconds > 0 && filteredHistory.Count > 1)
            {
                var sampledHistory = new List<ResourceUtilizationResponse>();
                var intervalTimeSpan = TimeSpan.FromSeconds(intervalSeconds);
                var currentTime = filteredHistory.Min(h => h.Timestamp);
                var endTime = filteredHistory.Max(h => h.Timestamp);

                while (currentTime <= endTime)
                {
                    var nextTime = currentTime.Add(intervalTimeSpan);
                    var sample = filteredHistory
                        .Where(h => h.Timestamp >= currentTime && h.Timestamp < nextTime)
                        .OrderBy(h => h.Timestamp)
                        .FirstOrDefault();

                    if (sample != null)
                    {
                        sampledHistory.Add(sample);
                    }

                    currentTime = nextTime;
                }

                return sampledHistory;
            }

            return filteredHistory;
        }

        /// <inheritdoc/>
        public async Task<ServiceResourceUtilization> GetServiceResourceUtilizationAsync(string serviceId)
        {
            // For now, we only monitor the current process
            if (serviceId == _options.Value.ServiceId)
            {
                return new ServiceResourceUtilization
                {
                    CpuUsagePercent = GetProcessCpuUsage(),
                    MemoryUsage = _currentProcess.WorkingSet64,
                    MemoryUsagePercent = GetProcessMemoryUsagePercent(),
                    ThreadCount = _currentProcess.Threads.Count,
                    HandleCount = _currentProcess.HandleCount
                };
            }

            // For other services, we would need to query them directly
            return new ServiceResourceUtilization();
        }

        /// <inheritdoc/>
        public void SetResourceThreshold(string resourceName, double thresholdPercent)
        {
            if (thresholdPercent < 0 || thresholdPercent > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(thresholdPercent), "Threshold must be between 0 and 100");
            }

            _thresholds[resourceName] = thresholdPercent;
            _logger.LogInformation("Set {ResourceName} threshold to {Threshold}%", resourceName, thresholdPercent);
        }

        private CpuUtilization GetCpuUtilization()
        {
            var cpuUtilization = new CpuUtilization
            {
                ProcessorCount = Environment.ProcessorCount
            };

            try
            {
                if (_cpuCounter != null)
                {
                    // Use performance counter on Windows
                    cpuUtilization.TotalUsagePercent = _cpuCounter.NextValue();
                }
                else
                {
                    // Fallback to process CPU usage
                    cpuUtilization.TotalUsagePercent = GetProcessCpuUsage();
                }

                // Get per-core usage if available
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    cpuUtilization.CoreUsagePercent.Add(cpuUtilization.TotalUsagePercent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CPU utilization");
            }

            return cpuUtilization;
        }

        private MemoryUtilization GetMemoryUtilization()
        {
            var memoryUtilization = new MemoryUtilization();

            try
            {
                if (_memCounter != null)
                {
                    // Use performance counter on Windows
                    memoryUtilization.UsagePercent = _memCounter.NextValue();
                }

                // Get memory info
                memoryUtilization.TotalPhysicalMemory = GetTotalPhysicalMemory();
                memoryUtilization.AvailablePhysicalMemory = GetAvailablePhysicalMemory();
                memoryUtilization.UsedPhysicalMemory = memoryUtilization.TotalPhysicalMemory - memoryUtilization.AvailablePhysicalMemory;

                if (memoryUtilization.UsagePercent == 0 && memoryUtilization.TotalPhysicalMemory > 0)
                {
                    memoryUtilization.UsagePercent = (double)memoryUtilization.UsedPhysicalMemory / memoryUtilization.TotalPhysicalMemory * 100;
                }

                // Get virtual memory info
                memoryUtilization.TotalVirtualMemory = GetTotalVirtualMemory();
                memoryUtilization.AvailableVirtualMemory = GetAvailableVirtualMemory();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memory utilization");
            }

            return memoryUtilization;
        }

        private List<DiskUtilization> GetDiskUtilization()
        {
            var diskUtilization = new List<DiskUtilization>();

            try
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                {
                    var disk = new DiskUtilization
                    {
                        DriveName = drive.Name,
                        DriveFormat = drive.DriveFormat,
                        DriveType = drive.DriveType.ToString(),
                        TotalSize = drive.TotalSize,
                        FreeSpace = drive.AvailableFreeSpace,
                        UsedSpace = drive.TotalSize - drive.AvailableFreeSpace
                    };

                    disk.UsagePercent = (double)disk.UsedSpace / disk.TotalSize * 100;
                    diskUtilization.Add(disk);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting disk utilization");
            }

            return diskUtilization;
        }

        private NetworkUtilization GetNetworkUtilization()
        {
            var networkUtilization = new NetworkUtilization();

            try
            {
                if (_networkSentCounter != null && _networkReceivedCounter != null)
                {
                    networkUtilization.BytesSentPerSecond = (long)_networkSentCounter.NextValue();
                    networkUtilization.BytesReceivedPerSecond = (long)_networkReceivedCounter.NextValue();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting network utilization");
            }

            return networkUtilization;
        }

        private HostInfo GetHostInfo()
        {
            var hostInfo = new HostInfo
            {
                MachineName = Environment.MachineName,
                OperatingSystem = Environment.OSVersion.ToString(),
                RuntimeVersion = Environment.Version.ToString(),
                ProcessUptime = DateTime.Now - _currentProcess.StartTime,
                SystemUptime = GetSystemUptime()
            };

            return hostInfo;
        }

        private double GetProcessCpuUsage()
        {
            try
            {
                _currentProcess.Refresh();
                return _currentProcess.TotalProcessorTime.TotalMilliseconds /
                       (Environment.ProcessorCount * _currentProcess.UserProcessorTime.TotalMilliseconds) * 100;
            }
            catch
            {
                return 0;
            }
        }

        private double GetProcessMemoryUsagePercent()
        {
            try
            {
                var totalMemory = GetTotalPhysicalMemory();
                if (totalMemory > 0)
                {
                    return (double)_currentProcess.WorkingSet64 / totalMemory * 100;
                }
            }
            catch { }

            return 0;
        }

        private long GetTotalPhysicalMemory()
        {
            try
            {
                return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            }
            catch
            {
                return 0;
            }
        }

        private long GetAvailablePhysicalMemory()
        {
            try
            {
                var memoryInfo = GC.GetGCMemoryInfo();
                return memoryInfo.TotalAvailableMemoryBytes - memoryInfo.MemoryLoadBytes;
            }
            catch
            {
                return 0;
            }
        }

        private long GetTotalVirtualMemory()
        {
            try
            {
                return Environment.WorkingSet;
            }
            catch
            {
                return 0;
            }
        }

        private long GetAvailableVirtualMemory()
        {
            try
            {
                // This is an approximation
                var memoryInfo = GC.GetGCMemoryInfo();
                return memoryInfo.TotalAvailableMemoryBytes - memoryInfo.MemoryLoadBytes;
            }
            catch
            {
                return 0;
            }
        }

        private TimeSpan GetSystemUptime()
        {
            try
            {
                return TimeSpan.FromMilliseconds(Environment.TickCount64);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        private void CleanupHistory()
        {
            // Only clean up once per hour
            if ((DateTime.UtcNow - _lastHistoryCleanup).TotalHours < 1)
            {
                return;
            }

            try
            {
                var cutoffTime = DateTime.UtcNow.AddDays(-_options.Value.DataRetentionDays);
                var oldEntries = _history.Where(h => h.Timestamp < cutoffTime).ToList();

                foreach (var entry in oldEntries)
                {
                    _history.Remove(entry);
                }

                _lastHistoryCleanup = DateTime.UtcNow;
                _logger.LogInformation("Cleaned up {Count} old resource utilization history entries", oldEntries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up resource utilization history");
            }
        }
    }
}
