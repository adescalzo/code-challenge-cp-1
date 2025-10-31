using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EmployeeChallenge.Infrastructure.Middleware;

/// <summary>
/// Middleware that creates a logging scope with correlation ID and request information
/// </summary>
public  sealed class LoggingScopeMiddleware(RequestDelegate next, ILogger<LoggingScopeMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Get correlation ID from HttpContext (set by CorrelationIdMiddleware)
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);

        // Create a logging scope with correlation ID and request information
        // Using Dictionary<string, object> for structured logging support
        var scopeState = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["CorrelationId"] = correlationId ?? "unknown",
            ["RequestPath"] = context.Request.Path.Value ?? string.Empty,
            ["RequestMethod"] = context.Request.Method,
            ["TraceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
        };

        // BeginScope ensures all log messages within this request include these properties
        using (logger.BeginScope(scopeState))
        {
            await next(context).ConfigureAwait(false);
        }
    }
}
