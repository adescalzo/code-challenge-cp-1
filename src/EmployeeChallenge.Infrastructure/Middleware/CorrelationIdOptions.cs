namespace EmployeeChallenge.Infrastructure.Middleware;

/// <summary>
/// Configuration options for correlation ID middleware
/// </summary>
public sealed class CorrelationIdOptions
{
    /// <summary>
    /// Header name for incoming correlation ID
    /// Default: X-Correlation-Id
    /// </summary>
    public string RequestHeaderName { get; set; } = "X-Correlation-Id";

    /// <summary>
    /// Header name for outgoing correlation ID
    /// Default: X-Correlation-Id
    /// </summary>
    public string ResponseHeaderName { get; set; } = "X-Correlation-Id";

    /// <summary>
    /// Include correlation ID in response headers
    /// Default: true
    /// </summary>
    public bool IncludeInResponse { get; set; } = true;

    /// <summary>
    /// Use HttpContext.TraceIdentifier as fallback if no correlation ID is provided
    /// Default: false (generates new GUID instead)
    /// </summary>
    public bool UseTraceIdentifierAsFallback { get; set; }

    /// <summary>
    /// Update Activity.Current with correlation ID for distributed tracing
    /// Default: true
    /// </summary>
    public bool UpdateActivity { get; set; } = true;
}
