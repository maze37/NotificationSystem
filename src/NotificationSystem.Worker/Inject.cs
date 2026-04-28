using System.Globalization;
using NotificationSystem.Application;
using NotificationSystem.Infrastructure;
using NotificationSystem.Worker.Services;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace NotificationSystem.Worker;

/// <summary>
/// Централизованная конфигурация Worker-слоя.
/// </summary>
internal static class Inject
{
    /// <summary>
    /// Регистрирует зависимости воркера и фоновые процессы.
    /// </summary>
    internal static IServiceCollection ConfigureApp(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddHostedService<RabbitMqNotificationConsumerService>()
            .AddHostedService<RecoveryService>();

        return services;
    }

    /// <summary>
    /// Настраивает Serilog для воркера.
    /// </summary>
    internal static HostApplicationBuilder ConfigureLogging(this HostApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, config) =>
        {
            var seqUrl = builder.Configuration["Seq"];

            config
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/notification-worker-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                config.WriteTo.Seq(seqUrl);
            }
        });

        return builder;
    }
}
