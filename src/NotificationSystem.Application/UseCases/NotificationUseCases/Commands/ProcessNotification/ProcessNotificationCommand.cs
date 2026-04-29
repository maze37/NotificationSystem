using NotificationSystem.Application.Abstractions;
using NotificationSystem.Contracts.Api.Notifications;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.ProcessNotification;

public sealed record ProcessNotificationCommand(Guid NotificationId, string CorrelationId) : ICommand<ProcessNotificationResponse>;
