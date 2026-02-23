using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class AddCategoryItemRequestValidator : AbstractValidator<AddCategoryItemRequest>
{
    public AddCategoryItemRequestValidator()
    {
        RuleFor(request => request.ItemTypeId)
            .NotEmpty().WithMessage("ItemTypeId is required.");

        RuleFor(request => request.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
