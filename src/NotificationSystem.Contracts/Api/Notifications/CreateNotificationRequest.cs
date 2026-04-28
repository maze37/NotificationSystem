using System.Text.Json;

namespace NotificationSystem.Contracts.Api.Notifications;

/// <summary>
/// Запрос на создание уведомления.
/// </summary>
public sealed record CreateNotificationRequest(
    string Channel,
    string Recipient,
    string TemplateCode,
    JsonElement Payload,
    string? CorrelationId);
