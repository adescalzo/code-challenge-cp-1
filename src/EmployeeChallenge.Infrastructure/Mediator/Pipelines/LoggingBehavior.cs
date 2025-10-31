using Microsoft.Extensions.Logging;

namespace EmployeeChallenge.Infrastructure.Mediator.Pipelines;

/// <summary>
/// Pipeline behavior that logs request handling start and completion
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public sealed partial class LoggingBehavior<TRequest, TResult>(ILogger<LoggingBehavior<TRequest, TResult>> logger)
    : IPipelineBehavior<TRequest, TResult>
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Handling request: {RequestName}")]
    private partial void LogHandlingRequest(string requestName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Finished handling request: {RequestName}")]
    private partial void LogFinishedHandlingRequest(string requestName);

    public async Task<TResult> Handle(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResult>> nextAction,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nextAction);

        LogHandlingRequest(typeof(TRequest).Name);

        var response = await nextAction(request, cancellationToken).ConfigureAwait(false);

        LogFinishedHandlingRequest(typeof(TRequest).Name);

        return response;
    }
}
