using EmployeeChallenge.Api.Application.Services;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Extensions;
using EmployeeChallenge.Infrastructure.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeChallenge.Api.Presentation.Employees.Queries;

internal class EmployeeQueryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup(EmployeesConstants.BaseUrl)
            .WithTags(EmployeesConstants.TagName)
            .RequireAuthorization();

        group.MapGet("/", async (
                [AsParameters] PaginationPayload payload,
                [FromServices] IDispatcher dispatcher,
                CancellationToken ct
            ) =>
            {
                var result = await dispatcher
                    .SendAsync<EmployeeGetAllQuery, Result<IEnumerable<EmployeeResponse>>>(
                        new EmployeeGetAllQuery(payload), ct
                    ).ConfigureAwait(false);

                return result.ToHttpResult();
            })
            .WithName("GetAllEmployees")
            .WithOpenApi()
            .Produces<IEnumerable<EmployeeResponse>>(200)
            .Produces(401);

        group.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] IDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var result = await dispatcher
                    .SendAsync<EmployeeGetQuery, Result<EmployeeResponse>>(
                        new EmployeeGetQuery(id), ct
                    ).ConfigureAwait(false);

                return result.ToHttpResult();
            })
            .WithName(EmployeesConstants.EndpointGetEmployeeByIdName)
            .WithOpenApi()
            .Produces<EmployeeResponse>(200)
            .Produces(401)
            .Produces(404);
    }
}
