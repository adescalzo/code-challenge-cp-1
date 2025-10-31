using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Presentation.Employees.Commands;

internal record EmployeeCreateCommand(EmployeeCommandPayload Payload) : ICommand<Result<Guid>>;
