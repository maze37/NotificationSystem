using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Infrastructure.Grpc;
using NotificationSystem.Infrastructure.Health;
using NotificationSystem.Infrastructure.Messaging;
using NotificationSystem.Infrastructure.Options;
using NotificationSystem.Infrastructure.Persistence;
using NotificationSystem.Infrastructure.Repositories;
using NotificationSystem.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace NotificationSystem.Infrastructure;

/// <summary>
/// Регистрация инфраструктурных зависимостей.
/// </summary>
public static class Inject
{
    /// <summary>
    /// Подключает БД, репозитории, RabbitMQ, gRPC-клиенты и health checks.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Строка подключения 'Postgres' не настроена.");

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IValidateOptions<RabbitMqOptions>, RabbitMqOptionsValidator>();
        services.Configure<GrpcEndpointsOptions>(configuration.GetSection(GrpcEndpointsOptions.SectionName));

        services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NotificationDbContext>());
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeliveryAttemptRepository, DeliveryAttemptRepository>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddSingleton<RabbitMqTopologyInitializer>();
        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

        services.AddGrpcClient<NotificationSystem.Contracts.Grpc.TemplateGrpc.TemplateGrpcClient>((sp, options) =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GrpcEndpointsOptions>>().Value;
            options.Address = new Uri(settings.TemplateServiceUrl);
        });

        services.AddGrpcClient<NotificationSystem.Contracts.Grpc.DeliveryGrpc.DeliveryGrpcClient>((sp, options) =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GrpcEndpointsOptions>>().Value;
            options.Address = new Uri(settings.DeliveryServiceUrl);
        });

        services.AddScoped<ITemplateServiceClient, TemplateServiceGrpcClient>();
        services.AddScoped<IDeliveryServiceClient, DeliveryServiceGrpcClient>();

        services.AddHealthChecks()
            .AddDbContextCheck<NotificationDbContext>("postgres", failureStatus: HealthStatus.Unhealthy)
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", failureStatus: HealthStatus.Unhealthy);

        return services;
    }
}
