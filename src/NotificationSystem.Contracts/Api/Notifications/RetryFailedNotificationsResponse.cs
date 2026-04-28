namespace NotificationSystem.Contracts.Api.Notifications;

/// <summary>
/// Результат повторной постановки уведомлений в очередь.
/// </summary>
public sealed record RetryFailedNotificationsResponse(int RequeuedCount, IReadOnlyCollection<Guid> NotificationIds);
