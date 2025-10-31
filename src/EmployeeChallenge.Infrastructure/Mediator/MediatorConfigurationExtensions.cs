using System.Reflection;
using EmployeeChallenge.Infrastructure.Mediator.Pipelines;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EmployeeChallenge.Infrastructure.Mediator;

/// <summary>
/// Extension methods for configuring mediator services
/// </summary>
public static class MediatorConfigurationExtensions
{
    /// <summary>
    /// Adds mediator services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers and validators</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies is not { Length: 0 })
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        services.TryAddScoped<IDispatcher, Dispatcher>();

        RegisterHandlers(services, typeof(ICommandHandler<,>), assemblies);
        RegisterHandlers(services, typeof(IQueryHandler<,>), assemblies);

        foreach (var assembly in assemblies)
        {
            services.AddValidatorsFromAssembly(assembly);
        }

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }

    /// <summary>
    /// Adds a custom pipeline behavior
    /// </summary>
    /// <typeparam name="TBehavior">The behavior type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPipelineBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : class
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TBehavior));
        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Type handlerInterfaceType, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .SelectMany(t => t.GetInterfaces(), (type, interfaceType) => new { type, interfaceType })
                .Where(x => x.interfaceType.IsGenericType &&
                           x.interfaceType.GetGenericTypeDefinition() == handlerInterfaceType)
                .ToList();

            foreach (var handler in handlerTypes)
            {
                services.TryAddScoped(handler.interfaceType, handler.type);
            }
        }
    }
}
