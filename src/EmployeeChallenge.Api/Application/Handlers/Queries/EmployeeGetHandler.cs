using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Presentation.Employees;
using EmployeeChallenge.Api.Presentation.Employees.Queries;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Application.Handlers.Queries;

internal sealed class EmployeeGetHandler(IEmployeeRepository repository)
    : ICommandHandler<EmployeeGetQuery, Result<EmployeeResponse>>
{
    public async Task<Result<EmployeeResponse>> Handle(
        EmployeeGetQuery query,
        CancellationToken cancellationToken)
    {
        var employee = await repository
            .GetByIdWithSupervisorAsync(query.Id, cancellationToken)
            .ConfigureAwait(false);

        if (employee is null)
        {
            return Result.Failure<EmployeeResponse>(
                ErrorResult.NotFound(nameof(Employee), query.Id.ToString())
            );
        }

        var totalReportsCount = employee.IsSupervisor
            ? await repository.GetTotalReportsCountAsync(employee.Id, cancellationToken).ConfigureAwait(false)
            : 0;

        var response = new EmployeeResponse(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.Supervisor?.Id,
            employee.Supervisor != null
                ? $"{employee.Supervisor.FirstName} {employee.Supervisor.LastName}"
                : null,
            totalReportsCount,
            employee.CreatedAt,
            employee.UpdatedAt
        );

        return Result.Success(response);
    }
}
