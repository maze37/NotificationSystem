using NotificationSystem.Application.Abstractions;
using NotificationSystem.Contracts.Api.Notifications;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.RetryFailedNotifications;

public sealed record RetryFailedNotificationsCommand(int BatchSize = 100, int OlderThanMinutes = 5) : ICommand<RetryFailedNotificationsResponse>;
