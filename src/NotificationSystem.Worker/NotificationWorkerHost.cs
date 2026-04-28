using Microsoft.EntityFrameworkCore;
using NotificationSystem.Infrastructure.Messaging;
using NotificationSystem.Infrastructure.Persistence;

namespace NotificationSystem.Worker;

public static class NotificationWorkerHost
{
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

    public static async Task InitializeAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await dbContext.Database.MigrateAsync();

        var topologyInitializer = scope.ServiceProvider.GetRequiredService<RabbitMqTopologyInitializer>();
        await topologyInitializer.InitializeAsync(CancellationToken.None);
    }
}
