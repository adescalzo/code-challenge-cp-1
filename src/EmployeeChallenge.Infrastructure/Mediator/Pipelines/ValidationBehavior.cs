using FluentValidation;

namespace EmployeeChallenge.Infrastructure.Mediator.Pipelines;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public sealed class ValidationBehavior<TRequest, TResult>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResult>
{
    public async Task<TResult> Handle(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResult>> nextAction,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nextAction);

        if (!validators.Any())
        {
            return await nextAction(request, cancellationToken).ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        ).ConfigureAwait(false);

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
        {
            return await nextAction(request, cancellationToken).ConfigureAwait(false);
        }

        if (!IsResultType())
        {
            throw new ValidationException(failures);
        }

        var validationErrors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(e => e.ErrorMessage)));

        var errorResult = ErrorResult.Validation(typeof(TRequest).Name, validationErrors);

        return CreateFailedResult(errorResult);
    }

    private static bool IsResultType()
    {
        var resultType = typeof(TResult);

        return resultType == typeof(Result) ||
               (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>));
    }

    private static TResult CreateFailedResult(ErrorResult errorResult)
    {
        var resultType = typeof(TResult);

        if (resultType == typeof(Result))
        {
            return (TResult)(object)Result.Failure(errorResult);
        }

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = resultType.GetGenericArguments()[0];
            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, [typeof(ErrorResult), valueType])!
                .MakeGenericMethod(valueType);

            return (TResult)failureMethod.Invoke(null, [errorResult, null])!;
        }

        throw new InvalidOperationException($"Unexpected result type: {resultType}");
    }
}
