namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.ProcessNotification;

public sealed record ProcessNotificationCommand(Guid NotificationId, string CorrelationId);
