using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class CreateKitRequestValidator : AbstractValidator<CreateKitRequest>
{
    public CreateKitRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must have at most 200 characters.");
    }
}
