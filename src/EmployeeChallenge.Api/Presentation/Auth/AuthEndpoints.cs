using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Extensions;
using EmployeeChallenge.Infrastructure.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeChallenge.Api.Presentation.Auth;

internal class AuthEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            [FromBody] LoginPayload payload,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct
        ) =>
        {
            var result = await dispatcher
                .SendAsync<LoginCommand, Result<AuthResponse>>(new LoginCommand(payload), ct)
                .ConfigureAwait(false);

            return result.ToHttpResult();
        })
        .WithTags("Authentication")
        .WithName("Login")
        .WithOpenApi()
        .Produces<AuthResponse>(200)
        .Produces(400)
        .Produces(401);
    }
}
