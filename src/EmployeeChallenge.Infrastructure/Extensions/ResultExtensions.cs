using EmployeeChallenge.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeChallenge.Infrastructure.Extensions;

/// <summary>
/// Extension methods for converting Result objects to HTTP responses
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an IResult HTTP response (200 OK or appropriate error response)
    /// </summary>
    public static IResult ToHttpResult(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? Results.Ok()
            : CreateProblemDetails(result);
    }

    /// <summary>
    /// Converts a Result to an IResult HTTP response (200 OK with value or appropriate error response)
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : CreateProblemDetails(result);
    }

    /// <summary>
    /// Converts a Result to an IResult HTTP response (200 OK or appropriate error response)
    /// </summary>
    public static IResult ToHttpResultEmpty(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? Results.NoContent()
            : CreateProblemDetails(result);
    }

    /// <summary>
    /// Converts a Result to a CreatedAtRoute response (201 Created with route and value or appropriate error response)
    /// </summary>
    public static IResult ToCreatedAtRouteResult<T>(this Result<T> result, string routeName, object? routeValues = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? Results.CreatedAtRoute(routeName, routeValues, result.Value)
            : CreateProblemDetails(result);
    }

    private static IResult CreateProblemDetails(Result result)
    {
        var (code, errorDefinition, description, exception) = result.Error;

        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency Conflict"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            _ when errorDefinition == ErrorDefinition.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
            _ when errorDefinition == ErrorDefinition.NotFound => (StatusCodes.Status404NotFound, "Resource Not Found"),
            _ when errorDefinition == ErrorDefinition.Concurrency => (StatusCodes.Status409Conflict, "Concurrency Conflict"),
            _ when errorDefinition == ErrorDefinition.Unauthorized => (StatusCodes.Status401Unauthorized, "Concurrency Conflict"),
            _ when errorDefinition == ErrorDefinition.Conflict => (StatusCodes.Status409Conflict, "Data Conflict"),
            _ => (StatusCodes.Status400BadRequest, "Bad Request")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Detail = description
        };

        // Add validation errors if present
        if (exception is ValidationException validationException && validationException.Properties.Count > 0)
        {
            problemDetails.Extensions.Add("errors", validationException.Properties);
        }

        // Add additional error information
        problemDetails.Extensions.Add("errorCode", code);
        problemDetails.Extensions.Add("errorDefinition", errorDefinition.ToString());

        return Results.Problem(problemDetails);
    }
}
