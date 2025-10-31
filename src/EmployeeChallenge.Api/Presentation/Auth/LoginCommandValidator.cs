using FluentValidation;

namespace EmployeeChallenge.Api.Presentation.Auth;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Payload.Username)
            .NotEmpty()
            .WithMessage("Username is required");

        RuleFor(x => x.Payload.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}
