using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Presentation.Employees.Commands;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Application.Handlers.Commands;

internal sealed class EmployeeUpdateCommandHandler(IEmployeeRepository repository)
    : ICommandHandler<EmployeeUpdateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        EmployeeUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var payload = command.Payload;
        var employee = await repository
            .GetById(command.Id, tracking: true)
            .ConfigureAwait(false);

        if (employee is null)
        {
            return Result.Failure<Guid>(
                ErrorResult.NotFound(nameof(Employee), command.Id.ToString())
            );
        }

        var emailExists = await repository
            .Any(e => e.Email == payload.Email && e.Id != command.Id)
            .ConfigureAwait(false);

        if (emailExists)
        {
            return Result.Failure<Guid>(ErrorResult.Conflict("Email already exists"));
        }

        var supervisorValidations = await ValidateSupervisorReferences(command, payload).ConfigureAwait(false);
        if (supervisorValidations.IsFailure)
        {
            return Result.Failure<Guid>(supervisorValidations.Error);
        }

        employee.Update(
            payload.FirstName,
            payload.LastName,
            payload.Email,
            payload.IsSupervisor,
            payload.SupervisorId
        );

        repository.Update([employee]);

        return Result.Success(command.Id);
    }

    private async Task<Result> ValidateSupervisorReferences(EmployeeUpdateCommand command,
        EmployeeCommandPayload payload)
    {
        if (!payload.SupervisorId.HasValue)
        {
            return Result.Success();
        }

        if (payload.SupervisorId.Value == command.Id)
        {
            var validationErrors1 = new Dictionary<string, string>
            {
                { nameof(payload.SupervisorId), "Employee cannot be their own supervisor" }
            };

            return Result.Failure(ErrorResult.Validation(nameof(EmployeeUpdateCommand), validationErrors1));
        }

        var supervisor = await repository
            .GetById(payload.SupervisorId.Value, tracking: false)
            .ConfigureAwait(false);

        if (supervisor is null)
        {
            return Result.Failure(ErrorResult.NotFound("Supervisor", payload.SupervisorId.Value.ToString()));
        }

        if (supervisor.SupervisorId != command.Id)
        {
            return Result.Success();
        }

        var validationErrors2 = new Dictionary<string, string>
        {
            {
                nameof(payload.SupervisorId),
                "Circular reference detected: The selected supervisor has this employee as their supervisor"
            }
        };

        return Result.Failure(ErrorResult.Validation(nameof(EmployeeUpdateCommand), validationErrors2));
    }
}
