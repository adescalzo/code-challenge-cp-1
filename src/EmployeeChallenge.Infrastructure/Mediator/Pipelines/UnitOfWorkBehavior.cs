using EmployeeChallenge.Infrastructure.Data;

namespace EmployeeChallenge.Infrastructure.Mediator.Pipelines;

/// <summary>
/// Pipeline behavior that automatically persists changes after command execution
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public sealed class UnitOfWorkBehavior<TRequest, TResult>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    public async Task<TResult> Handle(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResult>> nextAction,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nextAction);

        var result = await nextAction(request, cancellationToken).ConfigureAwait(false);

        if (unitOfWork.HasPendingChanges())
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return result;
    }
}
