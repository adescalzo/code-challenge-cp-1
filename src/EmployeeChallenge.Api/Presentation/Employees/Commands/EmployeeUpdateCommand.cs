using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Presentation.Employees.Commands;

internal record EmployeeUpdateCommand(Guid Id, EmployeeCommandPayload Payload) : ICommand<Result<Guid>>;
