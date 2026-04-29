using Microsoft.EntityFrameworkCore;
using NotificationSystem.Infrastructure.Messaging;
using NotificationSystem.Infrastructure.Persistence;

namespace NotificationSystem.Worker;

/// <summary>
/// Точка сборки и начальной инициализации хоста воркера.
/// </summary>
public static class NotificationWorkerHost
{
    /// <summary>
    /// Создает и настраивает host с DI, логированием и конфигурацией.
    /// </summary>
    public static IHost CreateHost(string[]? args = null, IDictionary<string, string?>? overrides = null)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var builder = Host.CreateApplicationBuilder(args ?? []);
        if (overrides is not null)
        {
            builder.Configuration.AddInMemoryCollection(overrides);
        }

        builder.ConfigureLogging();
        builder.Services.ConfigureApp(builder.Configuration);

        return builder.Build();
    }

    /// <summary>
    /// Выполняет стартовые действия: миграции БД и инициализацию topology в RabbitMQ.
    /// </summary>
    public static async Task InitializeAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await dbContext.Database.MigrateAsync();

        var topologyInitializer = scope.ServiceProvider.GetRequiredService<RabbitMqTopologyInitializer>();
        await topologyInitializer.InitializeAsync(CancellationToken.None);
    }
}
