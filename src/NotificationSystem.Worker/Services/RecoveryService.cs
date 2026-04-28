using NotificationSystem.Application.UseCases.NotificationUseCases.Commands.RetryFailedNotifications;

namespace NotificationSystem.Worker.Services;

public sealed class RecoveryService(
    IServiceScopeFactory scopeFactory,
    ILogger<RecoveryService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<RetryFailedNotificationsCommandHandler>();
                var result = await handler.HandleAsync(new RetryFailedNotificationsCommand(), stoppingToken);

                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "Сервис восстановления вернул ошибку. Code: {Code}, Message: {Message}",
                        result.Error.Code,
                        result.Error.Message);
                    continue;
                }

                if (result.Value.RequeuedCount > 0)
                {
                    logger.LogInformation("Сервис восстановления повторно поставил в очередь {Count} уведомлений", result.Value.RequeuedCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка цикла восстановления уведомлений");
            }
        }
    }
}
