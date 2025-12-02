using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Infrastructure.Mediator;

namespace EmployeeChallenge.Api.Presentation.Auth;

internal record LoginCommand(LoginPayload Payload) : ICommand<Result<AuthResponse>>;
