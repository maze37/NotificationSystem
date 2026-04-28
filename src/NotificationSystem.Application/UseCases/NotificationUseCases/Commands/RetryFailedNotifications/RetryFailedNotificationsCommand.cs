namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.RetryFailedNotifications;

public sealed record RetryFailedNotificationsCommand(int BatchSize = 100, int OlderThanMinutes = 5);
