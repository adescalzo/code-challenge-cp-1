using Microsoft.Extensions.DependencyInjection;

namespace EmployeeChallenge.Infrastructure.Extensions;

public static class SharedIServiceCollectionExtensions
{
    public static  IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, Clock>();

        return services;
    }
}
