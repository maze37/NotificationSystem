namespace NotificationSystem.Contracts.Api.Notifications;

/// <summary>
/// Данные попытки доставки для внешнего контракта.
/// </summary>
public sealed record DeliveryAttemptResponse(
    Guid Id,
    Guid NotificationId,
    int AttemptNumber,
    string Status,
    string? ErrorMessage,
    DateTimeOffset CreatedWhen);
