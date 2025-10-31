using Microsoft.Extensions.DependencyInjection;

namespace EmployeeChallenge.Infrastructure.Mediator;

/// <summary>
/// Dispatcher interface for sending commands and queries through a pipeline
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends a command through the pipeline
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="command">The command to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result from the command handler</returns>
    Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    /// <summary>
    /// Sends a query through the pipeline
    /// </summary>
    /// <typeparam name="TQuery">The query type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="query">The query to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result from the query handler</returns>
    Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}

/// <summary>
/// Dispatcher implementation
/// </summary>
public sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    public async Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = serviceProvider.GetService<ICommandHandler<TCommand, TResult>>()
                      ?? throw new InvalidOperationException($"No handler registered for {typeof(TCommand).Name}");

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, TResult>>().ToList();

        if (behaviors.Count == 0)
        {
            return await handler.Handle(command, cancellationToken).ConfigureAwait(false);
        }

        Func<TCommand, CancellationToken, Task<TResult>> pipeline = handler.Handle;

        for (var i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;
            pipeline = (req, ct) => behavior.Handle(req, next, ct);
        }

        return await pipeline(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        var handler = serviceProvider.GetService<IQueryHandler<TQuery, TResult>>()
                      ?? throw new InvalidOperationException($"No handler registered for {typeof(TQuery).Name}");

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TQuery, TResult>>().ToList();

        if (behaviors.Count == 0)
        {
            return await handler.Handle(query, cancellationToken).ConfigureAwait(false);
        }

        Func<TQuery, CancellationToken, Task<TResult>> pipeline = handler.Handle;

        for (var i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;
            pipeline = (req, ct) => behavior.Handle(req, next, ct);
        }

        return await pipeline(query, cancellationToken).ConfigureAwait(false);
    }
}
