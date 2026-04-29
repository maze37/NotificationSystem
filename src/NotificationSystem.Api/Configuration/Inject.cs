using System.Globalization;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application;
using NotificationSystem.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace NotificationSystem.Api.Configuration;

/// <summary>
/// Централизованная конфигурация API-слоя.
/// </summary>
internal static class CompositionExtensions
{
    /// <summary>
    /// Регистрирует зависимости API, Application и Infrastructure.
    /// </summary>
    internal static IServiceCollection ConfigureApp(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddApiPresentation();

        return services;
    }

    /// <summary>
    /// Настраивает Serilog (console/file/seq).
    /// </summary>
    internal static IHostBuilder ConfigureLogging(this IHostBuilder host)
    {
        return host.UseSerilog((context, config) =>
        {
            var seqUrl = context.Configuration["Seq"];

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
                    path: "logs/notification-api-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                config.WriteTo.Seq(seqUrl);
            }
        });
    }
}
