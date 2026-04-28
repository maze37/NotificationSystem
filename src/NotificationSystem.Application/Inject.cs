using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationSystem.Application;

/// <summary>
/// Регистрация зависимостей Application-слоя.
/// </summary>
public static class Inject
{
    /// <summary>
    /// Подключает use-cases, query-handlers и валидаторы.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Handler")))
            .AsSelf()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
