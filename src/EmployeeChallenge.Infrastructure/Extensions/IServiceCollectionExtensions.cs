using System.IO.Compression;
using System.Reflection;
using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeChallenge.Infrastructure.Extensions;

public static class IServiceCollectionExtensions
{
        /// <summary>
    /// Registers correlation ID options with the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration section</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCorrelationId(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<CorrelationIdOptions>(configuration.GetSection("CorrelationId"));
        return services;
    }

    /// <summary>
    /// Registers correlation ID options with the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCorrelationId(
        this IServiceCollection services,
        Action<CorrelationIdOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        return services;
    }

    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        if (assembly is null)
        {
            throw new ArgumentException("At least one assembly must be provided.", nameof(assembly));
        }

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo<IEndpoint>(), publicOnly: false) // Include internal classes
            .As<IEndpoint>()
            .WithTransientLifetime());

        return services;
    }

    /// <summary>
    /// Configures Problem Details responses with correlation ID and trace information
    /// </summary>
    public static IServiceCollection AddProblemDetailsConfiguration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<ProblemDetailsOptions>(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Type = $"https://httpstatuses.com/{ctx.ProblemDetails.Status}";
            };
        });

        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
            {
                var correlationId = CorrelationIdMiddleware.GetCorrelationId(ctx.HttpContext);

                ctx.ProblemDetails.Extensions.Add("correlation-id", correlationId ?? ctx.HttpContext.TraceIdentifier);
                ctx.ProblemDetails.Extensions.Add("trace-id", ctx.HttpContext.TraceIdentifier);
                ctx.ProblemDetails.Extensions.Add("instance",
                    $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}"
                );
            });

        return services;
    }

    /// <summary>
    /// Configures response compression with Brotli and Gzip
    /// </summary>
    public static IServiceCollection AddResponseCompressionConfiguration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddResponseCompression(options =>
        {
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        return services;
    }

    /// <summary>
    /// Configures exception handling with custom status code mapping
    /// </summary>
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();

        return services;
    }
}
