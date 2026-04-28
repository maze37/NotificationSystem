using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationSystem.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiPresentation(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddGrpc();
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
