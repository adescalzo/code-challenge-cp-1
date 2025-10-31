using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Extensions;
using EmployeeChallenge.Infrastructure.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeChallenge.Api.Presentation.Employees.Commands;

internal class EmployeeCommandEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup(EmployeesConstants.BaseUrl)
            .WithTags(EmployeesConstants.TagName)
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] EmployeeCommandPayload payload,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var result = await dispatcher
                .SendAsync<EmployeeCreateCommand, Result<Guid>>(new EmployeeCreateCommand(payload), ct)
                .ConfigureAwait(false);

            return result.ToCreatedAtRouteResult(EmployeesConstants.EndpointGetEmployeeByIdName, result.Value);
        })
        .WithName("CreateEmployee")
        .WithOpenApi()
        .Produces(201)
        .Produces(400)
        .Produces(401)
        .Produces(404)
        .Produces(409);

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] EmployeeCommandPayload payload,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var result = await dispatcher
                .SendAsync<EmployeeUpdateCommand, Result<Guid>>(new EmployeeUpdateCommand(id, payload), ct)
                .ConfigureAwait(false);

            return result.ToCreatedAtRouteResult(EmployeesConstants.EndpointGetEmployeeByIdName, result.Value);
        })
        .WithName("UpdateEmployee")
        .WithOpenApi()
        .Produces(200)
        .Produces(400)
        .Produces(401)
        .Produces(404)
        .Produces(409);

        group.MapDelete("/{id:guid}", async (
            Guid id,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var result = await dispatcher
                .SendAsync<EmployeeDeleteCommand, Result>(new EmployeeDeleteCommand(id), ct)
                .ConfigureAwait(false);

            return result.ToHttpResultEmpty();
        })
        .WithName("DeleteEmployee")
        .WithOpenApi()
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(404);
    }
}
