namespace NotificationSystem.Contracts.Api.Notifications;

/// <summary>
/// Результат обработки уведомления.
/// </summary>
public sealed record ProcessNotificationResponse(
    NotificationResponse Notification,
    DeliveryAttemptResponse? DeliveryAttempt,
    bool WasDelivered,
    bool WasRequeued,
    bool WasDeadLettered,
    string? FailureReason);
