using Microsoft.EntityFrameworkCore;
using NotificationSystem.Api.GrpcServices;
using NotificationSystem.Api.Middleware;
using NotificationSystem.Infrastructure.Messaging;
using NotificationSystem.Infrastructure.Persistence;
using Serilog;

namespace NotificationSystem.Api.Configuration;

public static class AppExtensions
{
    public static async Task<WebApplication> ConfigureExtensions(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await dbContext.Database.MigrateAsync();

        var topologyInitializer = scope.ServiceProvider.GetRequiredService<RabbitMqTopologyInitializer>();
        await topologyInitializer.InitializeAsync(CancellationToken.None);

        app.MapGrpcService<TemplateGrpcService>();
        app.MapGrpcService<DeliveryGrpcService>();
        app.MapHealthChecks("/health");

        return app;
    }
}
