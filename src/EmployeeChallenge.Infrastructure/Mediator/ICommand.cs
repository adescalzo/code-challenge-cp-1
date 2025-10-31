namespace EmployeeChallenge.Infrastructure.Mediator;

/// <summary>
/// Marker interface for commands (CQRS write operations)
/// </summary>
/// <typeparam name="TResult">The result type returned by the command handler</typeparam>
public interface ICommand<out TResult>
{
}
