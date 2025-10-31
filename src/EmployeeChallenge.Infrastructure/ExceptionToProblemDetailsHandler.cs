using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeChallenge.Infrastructure;

public sealed class ExceptionToProblemDetailsHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ExceptionToProblemDetailsHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "An unhandled exception occurred. Request Path {RequestPath}, Request Method {RequestMethod}",
            httpContext.Request.Path,
            httpContext.Request.Method
        );

        var problemDetails = new ProblemDetails
        {
            Status = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            },
            Title = "An error occurred",
            Type = exception.GetType().Name,
            Detail = exception.Message,
        };

        problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}
