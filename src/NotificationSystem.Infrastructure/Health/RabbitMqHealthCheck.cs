using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationSystem.Infrastructure.Messaging;

namespace NotificationSystem.Infrastructure.Health;

public sealed class RabbitMqHealthCheck(IRabbitMqConnectionProvider connectionProvider) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var channel = connectionProvider.GetConnection().CreateModel();
            return Task.FromResult(channel.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ channel is open.")
                : HealthCheckResult.Unhealthy("RabbitMQ channel is closed."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ is unavailable.", ex));
        }
    }
}
