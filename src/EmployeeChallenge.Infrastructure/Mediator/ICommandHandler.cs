namespace EmployeeChallenge.Infrastructure.Mediator;

/// <summary>
/// Defines a handler for a command
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles the command
    /// </summary>
    /// <param name="command">The command to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the command</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}
