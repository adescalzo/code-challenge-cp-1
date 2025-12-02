using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Presentation.Employees.Queries;

internal record EmployeeGetAllQuery(PaginationPayload Payload) : ICommand<Result<IEnumerable<EmployeeResponse>>>;
