namespace NotificationSystem.Contracts.Api.Notifications;

/// <summary>
/// Данные уведомления для внешнего контракта.
/// </summary>
public sealed record NotificationResponse(
    Guid Id,
    string Channel,
    string Recipient,
    string TemplateCode,
    string PayloadJson,
    string Status,
    int Attempts,
    DateTimeOffset CreatedWhen,
    DateTimeOffset UpdatedWhen,
    string? ErrorMessage,
    string CorrelationId);
