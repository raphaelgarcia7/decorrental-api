using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class AddCategoryItemRequestValidator : AbstractValidator<AddCategoryItemRequest>
{
    public AddCategoryItemRequestValidator()
    {
        RuleFor(request => request.ItemTypeId)
            .NotEmpty().WithMessage("ItemTypeId e obrigatorio.");

        RuleFor(request => request.Quantity)
            .GreaterThan(0).WithMessage("Quantity deve ser maior que zero.");
    }
}
