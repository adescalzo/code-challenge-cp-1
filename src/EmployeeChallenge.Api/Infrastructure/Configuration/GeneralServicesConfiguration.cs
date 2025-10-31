using Asp.Versioning;
using EmployeeChallenge.Api.Core;
using EmployeeChallenge.Infrastructure.Data;
using EmployeeChallenge.Infrastructure.General;
using Microsoft.EntityFrameworkCore;

namespace EmployeeChallenge.Api.Infrastructure.Configuration;

internal static class GeneralServicesConfiguration
{
    public static IServiceCollection AddGeneralServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, Clock>();

        return services;
    }

    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IDbSeeder, ApplicationDbContextSeeder>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseInMemoryDatabase("EmployeeChallengeDb");

            options.UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                var seeder = serviceProvider.GetRequiredService<IDbSeeder>();
                await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);
            });
        });

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    public static IServiceCollection AddApiVersionConfiguration(this IServiceCollection services)
    {
        // Configure API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
