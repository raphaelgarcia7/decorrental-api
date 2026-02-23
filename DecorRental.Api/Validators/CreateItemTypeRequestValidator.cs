using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class CreateItemTypeRequestValidator : AbstractValidator<CreateItemTypeRequest>
{
    public CreateItemTypeRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name must have at most 120 characters.");

        RuleFor(request => request.TotalStock)
            .GreaterThanOrEqualTo(0).WithMessage("TotalStock must be zero or greater.");
    }
}
