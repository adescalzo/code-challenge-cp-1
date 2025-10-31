using EmployeeChallenge.Api.Core.Repositories;

namespace EmployeeChallenge.Api.Infrastructure.Configuration;

internal static class RepositoryConfiguration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
