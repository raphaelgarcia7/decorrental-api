using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class AuthTokenRequestValidator : AbstractValidator<AuthTokenRequest>
{
    public AuthTokenRequestValidator()
    {
        RuleFor(request => request.Username)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MaximumLength(100);
    }
}
