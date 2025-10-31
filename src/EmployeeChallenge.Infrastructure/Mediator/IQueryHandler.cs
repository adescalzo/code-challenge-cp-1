namespace EmployeeChallenge.Infrastructure.Mediator;

/// <summary>
/// Defines a handler for a query
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the query
    /// </summary>
    /// <param name="query">The query to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the query</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
