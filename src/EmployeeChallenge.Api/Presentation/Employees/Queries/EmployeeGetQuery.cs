using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Presentation.Employees.Queries;

internal record EmployeeGetQuery(Guid Id) : ICommand<Result<EmployeeResponse>>;
