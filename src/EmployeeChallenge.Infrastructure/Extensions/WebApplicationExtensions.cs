using EmployeeChallenge.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeChallenge.Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null
    )
    {
        ArgumentNullException.ThrowIfNull(app);
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    /// <summary>
    /// Adds both correlation ID and logging scope middleware in the correct order
    /// This is the recommended way to add request tracking
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="configureOptions">Optional correlation ID configuration</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRequestTracking(
        this IApplicationBuilder app,
        Action<CorrelationIdOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Order matters: CorrelationId must be set before LoggingScope uses it
        app.UseCorrelationId(configureOptions);
        app.UseLoggingScope();

        return app;
    }

    /// <summary>
    /// Adds correlation ID middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="configureOptions">Optional configuration action</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseCorrelationId(
        this IApplicationBuilder app,
        Action<CorrelationIdOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (configureOptions == null)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }

        var options = new CorrelationIdOptions();
        configureOptions(options);

        return app.UseMiddleware<CorrelationIdMiddleware>(Microsoft.Extensions.Options.Options.Create(options));

    }

    /// <summary>
    /// Adds logging scope middleware to the application pipeline
    /// Must be called AFTER UseCorrelationId to include correlation ID in logs
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseLoggingScope(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<LoggingScopeMiddleware>();
    }
}
