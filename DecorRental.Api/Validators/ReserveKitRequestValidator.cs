using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class ReserveKitRequestValidator : AbstractValidator<ReserveKitRequest>
{
    public ReserveKitRequestValidator()
    {
        RuleFor(request => request.KitCategoryId)
            .NotEmpty().WithMessage("KitCategoryId e obrigatorio.");

        RuleFor(request => request.StartDate)
            .NotEmpty().WithMessage("StartDate e obrigatoria.");

        RuleFor(request => request.EndDate)
            .NotEmpty().WithMessage("EndDate e obrigatoria.")
            .GreaterThanOrEqualTo(request => request.StartDate)
            .WithMessage("EndDate deve ser maior ou igual a StartDate.");

        RuleFor(request => request.StockOverrideReason)
            .NotEmpty()
            .WithMessage("StockOverrideReason e obrigatoria quando AllowStockOverride for true.")
            .When(request => request.AllowStockOverride);

        RuleFor(request => request.StockOverrideReason)
            .MaximumLength(500)
            .WithMessage("StockOverrideReason deve ter no maximo 500 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.StockOverrideReason));
    }
}
