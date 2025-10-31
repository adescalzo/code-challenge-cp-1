using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Presentation.Employees;
using EmployeeChallenge.Api.Presentation.Employees.Queries;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Application.Handlers.Queries;

internal sealed class EmployeeGetAllHandler(IEmployeeRepository repository)
    : ICommandHandler<EmployeeGetAllQuery, Result<IEnumerable<EmployeeResponse>>>
{
    public async Task<Result<IEnumerable<EmployeeResponse>>> Handle(
        EmployeeGetAllQuery query,
        CancellationToken cancellationToken)
    {
        var payload = query.Payload;

        // Calculate skip based on pagination
        // Get paginated employees with supervisor included in a single query
        var employees = await repository
            .GetPaginatedWithSupervisorAsync(payload.Page, payload.PageSize, cancellationToken)
            .ConfigureAwait(false);

        // Map to response
        var responses = employees.Select(employee => new EmployeeResponse(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.Supervisor?.Id,
            employee.Supervisor != null
                ? $"{employee.Supervisor.FirstName} {employee.Supervisor.LastName}"
                : null,
            0,
            employee.CreatedAt,
            employee.UpdatedAt
        )).ToList();

        return Result.Success<IEnumerable<EmployeeResponse>>(responses);
    }
}
