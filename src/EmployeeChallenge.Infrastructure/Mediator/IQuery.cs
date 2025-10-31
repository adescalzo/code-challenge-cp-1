namespace EmployeeChallenge.Infrastructure.Mediator;

/// <summary>
/// Marker interface for queries (CQRS read operations)
/// </summary>
/// <typeparam name="TResult">The result type returned by the query handler</typeparam>
public interface IQuery<out TResult>
{
}
