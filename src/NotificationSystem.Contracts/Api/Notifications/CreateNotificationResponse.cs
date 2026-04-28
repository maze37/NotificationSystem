namespace NotificationSystem.Contracts.Api.Notifications;

/// <summary>
/// Результат создания уведомления для внешнего контракта.
/// </summary>
public sealed record CreateNotificationResponse(NotificationResponse Notification, bool IsDuplicate);
