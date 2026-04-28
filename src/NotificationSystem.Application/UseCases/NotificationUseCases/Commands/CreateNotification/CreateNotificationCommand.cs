using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.CreateNotification;

public sealed record CreateNotificationCommand(
    NotificationChannel Channel,
    string Recipient,
    string TemplateCode,
    string PayloadJson,
    string CorrelationId);
