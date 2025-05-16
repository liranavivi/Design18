using FlowOrchestrator.Observability.Alerting.Configuration;
using FlowOrchestrator.Observability.Alerting.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FlowOrchestrator.Observability.Alerting.Services;

/// <summary>
/// Service for sending notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IOptions<AlertingSystemOptions> _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public NotificationService(
        ILogger<NotificationService> logger,
        IOptions<AlertingSystemOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options;
        _httpClient = httpClientFactory.CreateClient("NotificationService");
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing notification service");
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task SendNotificationsAsync(AlertModel alert)
    {
        using var activity = new ActivitySource("FlowOrchestrator.AlertingSystem").StartActivity("SendNotifications");
        
        _logger.LogInformation("Sending notifications for alert: {AlertId}, {Name}", alert.AlertId, alert.Name);

        var tasks = new List<Task>();

        // Send email notifications
        if (_options.Value.NotificationChannels.Email.Enabled)
        {
            tasks.Add(SendEmailNotificationAsync(alert, _options.Value.NotificationChannels.Email.DefaultRecipients));
        }

        // Send webhook notifications
        if (_options.Value.NotificationChannels.Webhook.Enabled)
        {
            tasks.Add(SendWebhookNotificationAsync(alert, _options.Value.NotificationChannels.Webhook.Endpoints));
        }

        // Send SMS notifications
        if (_options.Value.NotificationChannels.Sms.Enabled)
        {
            tasks.Add(SendSmsNotificationAsync(alert, _options.Value.NotificationChannels.Sms.DefaultRecipients));
        }

        await Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public async Task SendEmailNotificationAsync(AlertModel alert, List<string> recipients)
    {
        if (!recipients.Any())
        {
            _logger.LogWarning("No email recipients specified for alert: {AlertId}", alert.AlertId);
            return;
        }

        try
        {
            _logger.LogInformation("Sending email notification for alert {AlertId} to {RecipientCount} recipients", 
                alert.AlertId, recipients.Count);

            // In a real implementation, this would use an email service
            // For now, we'll just log the email details

            var subject = $"Alert: {alert.Name} - {alert.Severity}";
            var body = $"Alert: {alert.Name}\n" +
                       $"Severity: {alert.Severity}\n" +
                       $"Description: {alert.Description}\n" +
                       $"Metric: {alert.MetricName}\n" +
                       $"Value: {alert.MetricValue}\n" +
                       $"Threshold: {alert.Threshold}\n" +
                       $"Timestamp: {alert.Timestamp:yyyy-MM-dd HH:mm:ss}";

            _logger.LogDebug("Email notification details:\n" +
                             "From: {FromAddress}\n" +
                             "To: {Recipients}\n" +
                             "Subject: {Subject}\n" +
                             "Body: {Body}",
                _options.Value.NotificationChannels.Email.FromAddress,
                string.Join(", ", recipients),
                subject,
                body);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notification for alert {AlertId}", alert.AlertId);
        }
    }

    /// <inheritdoc/>
    public async Task SendWebhookNotificationAsync(AlertModel alert, List<string> endpoints)
    {
        if (!endpoints.Any())
        {
            _logger.LogWarning("No webhook endpoints specified for alert: {AlertId}", alert.AlertId);
            return;
        }

        try
        {
            _logger.LogInformation("Sending webhook notification for alert {AlertId} to {EndpointCount} endpoints", 
                alert.AlertId, endpoints.Count);

            var tasks = new List<Task>();

            foreach (var endpoint in endpoints)
            {
                tasks.Add(SendWebhookToEndpointAsync(alert, endpoint));
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook notifications for alert {AlertId}", alert.AlertId);
        }
    }

    /// <inheritdoc/>
    public async Task SendSmsNotificationAsync(AlertModel alert, List<string> recipients)
    {
        if (!recipients.Any())
        {
            _logger.LogWarning("No SMS recipients specified for alert: {AlertId}", alert.AlertId);
            return;
        }

        try
        {
            _logger.LogInformation("Sending SMS notification for alert {AlertId} to {RecipientCount} recipients", 
                alert.AlertId, recipients.Count);

            // In a real implementation, this would use an SMS service
            // For now, we'll just log the SMS details

            var message = $"Alert: {alert.Name} - {alert.Severity} - {alert.MetricName}: {alert.MetricValue} {alert.Operator} {alert.Threshold}";

            _logger.LogDebug("SMS notification details:\n" +
                             "From: {FromNumber}\n" +
                             "To: {Recipients}\n" +
                             "Message: {Message}",
                _options.Value.NotificationChannels.Sms.FromNumber,
                string.Join(", ", recipients),
                message);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS notification for alert {AlertId}", alert.AlertId);
        }
    }

    private async Task SendWebhookToEndpointAsync(AlertModel alert, string endpoint)
    {
        try
        {
            _logger.LogDebug("Sending webhook notification to endpoint: {Endpoint}", endpoint);

            var content = JsonContent.Create(alert);
            var response = await _httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Webhook notification sent successfully to {Endpoint}", endpoint);
            }
            else
            {
                _logger.LogWarning("Failed to send webhook notification to {Endpoint}. Status code: {StatusCode}", 
                    endpoint, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook notification to {Endpoint}", endpoint);
        }
    }
}
