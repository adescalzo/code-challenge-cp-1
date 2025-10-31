namespace EmployeeChallenge.Infrastructure.Mediator;

/// <summary>
/// Pipeline behavior to wrap around request handlers and add cross-cutting concerns
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface IPipelineBehavior<TRequest, TResult>
{
    Task<TResult> Handle(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResult>> nextAction,
        CancellationToken cancellationToken);
}
