using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Presentation.Employees.Commands;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Application.Handlers.Commands;

internal sealed class EmployeeCreateCommandHandler(IEmployeeRepository repository)
    : ICommandHandler<EmployeeCreateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        EmployeeCreateCommand command,
        CancellationToken cancellationToken)
    {
        var payload = command.Payload;
        var emailExists = await repository
            .Any(e => e.Email == payload.Email)
            .ConfigureAwait(false);

        if (emailExists)
        {
            return Result.Failure<Guid>(ErrorResult.Conflict("Email already exists"));
        }

        if (payload.SupervisorId.HasValue)
        {
            var supervisorExists = await repository
                .Any(e => e.Id == payload.SupervisorId.Value)
                .ConfigureAwait(false);

            if (!supervisorExists)
            {
                return Result.Failure<Guid>(
                    ErrorResult.NotFound(nameof(Employee), payload.SupervisorId.Value.ToString())
                );
            }
        }

        var employee = new Employee(
            Guid.CreateVersion7(),
            payload.FirstName,
            payload.LastName,
            payload.Email,
            payload.IsSupervisor,
            payload.SupervisorId
        );

        await repository.Add(employee).ConfigureAwait(false);

        return Result.Success(employee.Id);
    }
}
