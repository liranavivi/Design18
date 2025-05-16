using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Observability.Monitoring.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Implementation of the health check service.
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<MonitoringOptions> _options;
        private readonly IResourceUtilizationMonitorService _resourceUtilizationMonitorService;
        private readonly Dictionary<string, ServiceHealthStatus> _registeredServices = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="options">The monitoring options.</param>
        /// <param name="resourceUtilizationMonitorService">The resource utilization monitor service.</param>
        public HealthCheckService(
            ILogger<HealthCheckService> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<MonitoringOptions> options,
            IResourceUtilizationMonitorService resourceUtilizationMonitorService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _options = options;
            _resourceUtilizationMonitorService = resourceUtilizationMonitorService;
        }

        /// <inheritdoc/>
        public async Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var healthEntries = new Dictionary<string, HealthReportEntry>();
            var startTime = DateTime.UtcNow;

            try
            {
                // Check monitoring framework health
                healthEntries.Add("MonitoringFramework", new HealthReportEntry(
                    HealthStatus.Healthy,
                    "Monitoring Framework is running",
                    TimeSpan.Zero,
                    null,
                    null));

                // Check resource utilization
                var resourceUtilization = await _resourceUtilizationMonitorService.GetResourceUtilizationAsync();
                var resourceStatus = HealthStatus.Healthy;
                var resourceDescription = "Resource utilization is within normal limits";

                if (resourceUtilization.Cpu.TotalUsagePercent > _options.Value.CpuUsageThresholdPercent)
                {
                    resourceStatus = HealthStatus.Degraded;
                    resourceDescription = $"CPU usage ({resourceUtilization.Cpu.TotalUsagePercent:F1}%) exceeds threshold ({_options.Value.CpuUsageThresholdPercent}%)";
                }

                if (resourceUtilization.Memory.UsagePercent > _options.Value.MemoryUsageThresholdPercent)
                {
                    resourceStatus = HealthStatus.Degraded;
                    resourceDescription = $"Memory usage ({resourceUtilization.Memory.UsagePercent:F1}%) exceeds threshold ({_options.Value.MemoryUsageThresholdPercent}%)";
                }

                healthEntries.Add("ResourceUtilization", new HealthReportEntry(
                    resourceStatus,
                    resourceDescription,
                    DateTime.UtcNow - startTime,
                    null,
                    null));

                // Check registered services
                foreach (var service in _registeredServices.Values)
                {
                    try
                    {
                        var serviceStatus = await CheckServiceHealthAsync(service.ServiceId, service.Endpoint, cancellationToken);
                        healthEntries.Add(service.ServiceId, new HealthReportEntry(
                            serviceStatus.HealthStatus == "Healthy" ? HealthStatus.Healthy :
                            serviceStatus.HealthStatus == "Degraded" ? HealthStatus.Degraded : HealthStatus.Unhealthy,
                            serviceStatus.HealthDetails,
                            DateTime.UtcNow - startTime,
                            null,
                            null));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error checking health for service {ServiceId}", service.ServiceId);
                        healthEntries.Add(service.ServiceId, new HealthReportEntry(
                            HealthStatus.Unhealthy,
                            $"Error checking health: {ex.Message}",
                            DateTime.UtcNow - startTime,
                            ex,
                            null));
                    }
                }

                return new HealthReport(healthEntries, DateTime.UtcNow - startTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing health check");
                healthEntries.Add("MonitoringFramework", new HealthReportEntry(
                    HealthStatus.Unhealthy,
                    $"Error performing health check: {ex.Message}",
                    DateTime.UtcNow - startTime,
                    ex,
                    null));
                return new HealthReport(healthEntries, DateTime.UtcNow - startTime);
            }
        }

        /// <inheritdoc/>
        public async Task<List<ServiceHealthStatus>> DiscoverServicesAsync(CancellationToken cancellationToken = default)
        {
            var services = new List<ServiceHealthStatus>();

            try
            {
                if (_options.Value.EnableAutoDiscovery)
                {
                    foreach (var endpoint in _options.Value.ServiceDiscoveryEndpoints)
                    {
                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            var response = await client.GetFromJsonAsync<List<ServiceHealthStatus>>(endpoint, cancellationToken);

                            if (response != null)
                            {
                                services.AddRange(response);

                                // Register discovered services
                                foreach (var service in response)
                                {
                                    RegisterService(service.ServiceId, service.ServiceType, service.Endpoint);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error discovering services from endpoint {Endpoint}", endpoint);
                        }
                    }
                }

                // Add registered services that weren't discovered
                foreach (var service in _registeredServices.Values)
                {
                    if (!services.Any(s => s.ServiceId == service.ServiceId))
                    {
                        services.Add(service);
                    }
                }

                return services;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering services");
                return _registeredServices.Values.ToList();
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceHealthStatus> GetServiceHealthStatusAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            if (_registeredServices.TryGetValue(serviceId, out var service))
            {
                try
                {
                    return await CheckServiceHealthAsync(serviceId, service.Endpoint, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting health status for service {ServiceId}", serviceId);
                    service.HealthStatus = "Unhealthy";
                    service.HealthDetails = $"Error checking health: {ex.Message}";
                    service.LastCheckTimestamp = DateTime.UtcNow;
                    return service;
                }
            }

            throw new KeyNotFoundException($"Service with ID {serviceId} is not registered");
        }

        /// <inheritdoc/>
        public async Task<SystemStatusResponse> GetSystemHealthStatusAsync(CancellationToken cancellationToken = default)
        {
            var response = new SystemStatusResponse
            {
                Timestamp = DateTime.UtcNow
            };

            try
            {
                // Get resource utilization
                response.ResourceUtilization = await _resourceUtilizationMonitorService.GetResourceUtilizationAsync();

                // Get service health statuses
                var services = await DiscoverServicesAsync(cancellationToken);

                foreach (var service in services)
                {
                    try
                    {
                        var serviceStatus = await CheckServiceHealthAsync(service.ServiceId, service.Endpoint, cancellationToken);
                        response.Services.Add(serviceStatus);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error checking health for service {ServiceId}", service.ServiceId);
                        service.HealthStatus = "Unhealthy";
                        service.HealthDetails = $"Error checking health: {ex.Message}";
                        service.LastCheckTimestamp = DateTime.UtcNow;
                        response.Services.Add(service);
                    }
                }

                // Determine overall system status
                if (response.Services.Any(s => s.HealthStatus == "Unhealthy"))
                {
                    response.Status = "Unhealthy";
                }
                else if (response.Services.Any(s => s.HealthStatus == "Degraded"))
                {
                    response.Status = "Degraded";
                }
                else
                {
                    response.Status = "Healthy";
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health status");
                response.Status = "Unhealthy";
                return response;
            }
        }

        /// <inheritdoc/>
        public bool RegisterService(string serviceId, string serviceType, string endpoint)
        {
            try
            {
                var service = new ServiceHealthStatus
                {
                    ServiceId = serviceId,
                    ServiceType = serviceType,
                    Endpoint = endpoint,
                    HealthStatus = "Unknown",
                    LastCheckTimestamp = DateTime.UtcNow
                };

                _registeredServices[serviceId] = service;
                _logger.LogInformation("Registered service {ServiceId} of type {ServiceType} at endpoint {Endpoint}",
                    serviceId, serviceType, endpoint);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering service {ServiceId}", serviceId);
                return false;
            }
        }

        /// <inheritdoc/>
        public bool UnregisterService(string serviceId)
        {
            try
            {
                if (_registeredServices.Remove(serviceId))
                {
                    _logger.LogInformation("Unregistered service {ServiceId}", serviceId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering service {ServiceId}", serviceId);
                return false;
            }
        }

        private async Task<ServiceHealthStatus> CheckServiceHealthAsync(string serviceId, string endpoint, CancellationToken cancellationToken)
        {
            var service = _registeredServices.TryGetValue(serviceId, out var existingService)
                ? existingService
                : new ServiceHealthStatus { ServiceId = serviceId, Endpoint = endpoint };

            try
            {
                var client = _httpClientFactory.CreateClient();
                var healthEndpoint = endpoint.TrimEnd('/') + "/health";

                var stopwatch = Stopwatch.StartNew();
                var response = await client.GetAsync(healthEndpoint, cancellationToken);
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>(cancellationToken: cancellationToken);

                    service.HealthStatus = healthResponse?.Status ?? "Healthy";
                    service.HealthDetails = $"Health check completed in {stopwatch.ElapsedMilliseconds}ms";
                    service.State = ServiceState.READY;
                    service.Status = "ACTIVE";
                }
                else
                {
                    service.HealthStatus = "Unhealthy";
                    service.HealthDetails = $"Health check failed with status code {response.StatusCode}";
                    service.State = ServiceState.ERROR;
                    service.Status = "INACTIVE";
                }
            }
            catch (Exception ex)
            {
                service.HealthStatus = "Unhealthy";
                service.HealthDetails = $"Error checking health: {ex.Message}";
                service.State = ServiceState.ERROR;
                service.Status = "INACTIVE";
            }

            service.LastCheckTimestamp = DateTime.UtcNow;
            return service;
        }
    }
}
