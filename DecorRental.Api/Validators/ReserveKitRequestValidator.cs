using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class ReserveKitRequestValidator : AbstractValidator<ReserveKitRequest>
{
    public ReserveKitRequestValidator()
    {
        RuleFor(request => request.KitCategoryId)
            .NotEmpty().WithMessage("KitCategoryId is required.");

        RuleFor(request => request.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(request => request.EndDate)
            .NotEmpty().WithMessage("EndDate is required.")
            .GreaterThanOrEqualTo(request => request.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");

        RuleFor(request => request.StockOverrideReason)
            .NotEmpty()
            .WithMessage("StockOverrideReason is required when AllowStockOverride is true.")
            .When(request => request.AllowStockOverride);

        RuleFor(request => request.StockOverrideReason)
            .MaximumLength(500)
            .WithMessage("StockOverrideReason must have at most 500 characters.")
            .When(request => !string.IsNullOrWhiteSpace(request.StockOverrideReason));
    }
}
