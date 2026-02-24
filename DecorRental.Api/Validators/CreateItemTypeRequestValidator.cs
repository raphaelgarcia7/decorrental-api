using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class CreateItemTypeRequestValidator : AbstractValidator<CreateItemTypeRequest>
{
    public CreateItemTypeRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Nome e obrigatorio.")
            .MaximumLength(120).WithMessage("Name must have at most 120 characters.");

        RuleFor(request => request.TotalStock)
            .GreaterThanOrEqualTo(0).WithMessage("TotalStock deve ser zero ou maior.");
    }
}
