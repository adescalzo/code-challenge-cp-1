using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace EmployeeChallenge.Infrastructure.Middleware;

/// <summary>
/// Middleware that ensures every request has a correlation ID for tracking across services
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next, IOptions<CorrelationIdOptions> optionsAccessor)
{
    private readonly CorrelationIdOptions _options =
        optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));

    /// <summary>
    /// HttpContext.Items key for storing correlation ID
    /// </summary>
    private const string CorrelationIdItemKey = "CorrelationId";

    // Public Methods

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Try to get correlation ID from the request header
        var correlationId = GetCorrelationIdFromRequest(context);

        // Store in HttpContext.Items for access throughout the request pipeline
        context.Items[CorrelationIdItemKey] = correlationId;

        // Update Activity for distributed tracing (OpenTelemetry, Application Insights, etc.)
        if (_options.UpdateActivity && Activity.Current != null)
        {
            Activity.Current.SetTag("correlation_id", correlationId);
        }

        // Add to response headers if configured
        if (_options.IncludeInResponse)
        {
            // Use OnStarting to ensure a header is added even if the response has already started
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(_options.ResponseHeaderName))
                {
                    context.Response.Headers.Append(_options.ResponseHeaderName, correlationId);
                }
                return Task.CompletedTask;
            });
        }

        await next(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Helper method to get correlation ID from HttpContext
    /// </summary>
    public static string? GetCorrelationId(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items.TryGetValue(CorrelationIdItemKey, out var correlationId)
            ? correlationId as string
            : null;
    }

    // Private Methods

    private string GetCorrelationIdFromRequest(HttpContext context)
    {
        // Try to get from the request header
        if (TryGetValidHeaderValue(context.Request.Headers, _options.RequestHeaderName, out var correlationId))
        {
            return correlationId;
        }

        // Fallback: use TraceIdentifier or generate new GUID
        return _options.UseTraceIdentifierAsFallback
            ? context.TraceIdentifier
            : Guid.NewGuid().ToString("D"); // "D" format is the most compact and widely used
    }

    /// <summary>
    /// Attempts to retrieve a valid (non-empty, non-whitespace) header value
    /// </summary>
    /// <param name="headers">The request headers collection</param>
    /// <param name="headerName">The name of the header to retrieve</param>
    /// <param name="value">The retrieved header value if found and valid</param>
    /// <returns>True if a valid header value exists; otherwise false</returns>
    private static bool TryGetValidHeaderValue(
        IHeaderDictionary headers,
        string headerName,
        out string value)
    {
        if (headers.TryGetValue(headerName, out var headerValues) && headerValues.Count > 0)
        {
            var headerValue = headerValues[0];
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                value = headerValue;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }
}
