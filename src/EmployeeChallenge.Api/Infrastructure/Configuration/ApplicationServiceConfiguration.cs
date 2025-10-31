using EmployeeChallenge.Api.Application;
using EmployeeChallenge.Api.Application.Services;

namespace EmployeeChallenge.Api.Infrastructure.Configuration;

internal static class ApplicationServiceConfiguration
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register AuthService explicitly
        services.AddScoped<IAuthService, AuthService>();

        // Scan for other services
        services.Scan(scan => scan
            .FromAssemblyOf<IAuthService>()
            .AddClasses(classes => classes.AssignableTo<IAuthService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
