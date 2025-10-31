using EmployeeChallenge.Api.Application.Services;
using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Presentation.Auth;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Application.Handlers.Commands;

internal sealed class LoginCommandHandler(
    IUserRepository repository,
    IAuthService authService)
    : ICommandHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var payload = command.Payload;

        // Find user
        var user = await repository.GetByUsernameAsync(payload.Username, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Result.Failure<AuthResponse>(ErrorResult.Unauthorized("Invalid username or password"));
        }

        // Verify password
        if (!authService.VerifyPassword(payload.Password, user.Password))
        {
            return Result.Failure<AuthResponse>(
                ErrorResult.Unauthorized("Invalid username or password")
            );
        }

        // Generate JWT token
        var token = authService.GenerateJwtToken(user);

        return Result.Success(new AuthResponse(token, user.Username, user.Email));
    }
}
