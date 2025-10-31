using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Presentation.Employees.Commands;

internal record EmployeeDeleteCommand(Guid Id) : ICommand<Result>;
