using FlowOrchestrator.Observability.Alerting.Models;

namespace FlowOrchestrator.Observability.Alerting.Services;

/// <summary>
/// Interface for the notification service.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Initializes the service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Sends notifications for an alert.
    /// </summary>
    /// <param name="alert">The alert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendNotificationsAsync(AlertModel alert);

    /// <summary>
    /// Sends an email notification.
    /// </summary>
    /// <param name="alert">The alert.</param>
    /// <param name="recipients">The recipients.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailNotificationAsync(AlertModel alert, List<string> recipients);

    /// <summary>
    /// Sends a webhook notification.
    /// </summary>
    /// <param name="alert">The alert.</param>
    /// <param name="endpoints">The webhook endpoints.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendWebhookNotificationAsync(AlertModel alert, List<string> endpoints);

    /// <summary>
    /// Sends an SMS notification.
    /// </summary>
    /// <param name="alert">The alert.</param>
    /// <param name="recipients">The recipients.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendSmsNotificationAsync(AlertModel alert, List<string> recipients);
}
