using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Common.Utilities;
using FlowOrchestrator.Observability.Analytics.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Analytics.Services
{
    /// <summary>
    /// Client for interacting with the Statistics Service.
    /// </summary>
    public class StatisticsServiceClient : IStatisticsServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StatisticsServiceClient> _logger;
        private readonly AnalyticsEngineOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsServiceClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The options.</param>
        public StatisticsServiceClient(
            HttpClient httpClient,
            ILogger<StatisticsServiceClient> logger,
            IOptions<AnalyticsEngineOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            _httpClient.BaseAddress = new Uri(_options.StatisticsServiceUrl);
        }

        /// <inheritdoc/>
        public async Task<List<MetricDefinition>> GetAvailableMetricsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<MetricDefinition>>("api/statistics/metrics", _jsonOptions);
                return response ?? new List<MetricDefinition>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available metrics from Statistics Service");
                return new List<MetricDefinition>();
            }
        }

        /// <inheritdoc/>
        public async Task<QueryResult> QueryMetricsAsync(MetricQuery query)
        {
            Guard.AgainstNull(query, nameof(query));

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/statistics/query", query, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<QueryResult>(_jsonOptions);
                return result ?? new QueryResult { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying metrics from Statistics Service");
                return new QueryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Query = query
                };
            }
        }

        /// <inheritdoc/>
        public async Task<List<AlertDefinition>> GetConfiguredAlertsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AlertDefinition>>("api/statistics/alerts", _jsonOptions);
                return response ?? new List<AlertDefinition>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configured alerts from Statistics Service");
                return new List<AlertDefinition>();
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> GetStatisticsAsync(string providerId)
        {
            Guard.AgainstNullOrEmpty(providerId, nameof(providerId));

            try
            {
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>($"api/statistics/providers/{providerId}", _jsonOptions);
                return response ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics from provider {ProviderId}", providerId);
                return new Dictionary<string, object>();
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetStatisticsProvidersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<string>>("api/statistics/providers", _jsonOptions);
                return response ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics providers from Statistics Service");
                return new List<string>();
            }
        }
    }

    /// <summary>
    /// Interface for the Statistics Service client.
    /// </summary>
    public interface IStatisticsServiceClient
    {
        /// <summary>
        /// Gets the available metrics from the Statistics Service.
        /// </summary>
        /// <returns>The list of available metrics.</returns>
        Task<List<MetricDefinition>> GetAvailableMetricsAsync();

        /// <summary>
        /// Queries metrics from the Statistics Service.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The query result.</returns>
        Task<QueryResult> QueryMetricsAsync(MetricQuery query);

        /// <summary>
        /// Gets the configured alerts from the Statistics Service.
        /// </summary>
        /// <returns>The list of configured alerts.</returns>
        Task<List<AlertDefinition>> GetConfiguredAlertsAsync();

        /// <summary>
        /// Gets statistics from a specific provider.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        /// <returns>The statistics.</returns>
        Task<Dictionary<string, object>> GetStatisticsAsync(string providerId);

        /// <summary>
        /// Gets the list of statistics providers.
        /// </summary>
        /// <returns>The list of provider identifiers.</returns>
        Task<List<string>> GetStatisticsProvidersAsync();
    }
}
