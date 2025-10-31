using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Presentation.Employees.Commands;
using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Application.Handlers.Commands;

internal sealed class EmployeeDeleteCommandHandler(IEmployeeRepository repository)
    : ICommandHandler<EmployeeDeleteCommand, Result>
{
    public async Task<Result> Handle(
        EmployeeDeleteCommand command,
        CancellationToken cancellationToken)
    {
        // Get employee
        var employee = await repository
            .GetById(command.Id, tracking: true)
            .ConfigureAwait(false);

        if (employee is null)
        {
            return Result.Failure(ErrorResult.NotFound(nameof(Employee), command.Id.ToString()));
        }

        var directReports = (await repository
            .GetDirectReportsAsync(command.Id, cancellationToken)
            .ConfigureAwait(false)).ToArray();

        if (directReports.Length != 0)
        {
            var validationErrors = new Dictionary<string, string>
            {
                { "DirectReports", $"Cannot delete employee with direct reports. {directReports.Length} cases." }
            };

            return Result.Failure(ErrorResult.Validation(nameof(EmployeeDeleteCommand), validationErrors));
        }

        repository.Remove(employee);

        return Result.Success();
    }
}
