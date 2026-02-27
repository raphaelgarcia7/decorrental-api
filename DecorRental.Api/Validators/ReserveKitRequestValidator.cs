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

        RuleFor(request => request.CustomerName)
            .NotEmpty().WithMessage("CustomerName e obrigatorio.")
            .MaximumLength(120).WithMessage("CustomerName deve ter no maximo 120 caracteres.");

        RuleFor(request => request.CustomerDocumentNumber)
            .NotEmpty().WithMessage("CustomerDocumentNumber e obrigatorio.")
            .MaximumLength(40).WithMessage("CustomerDocumentNumber deve ter no maximo 40 caracteres.");

        RuleFor(request => request.CustomerPhoneNumber)
            .NotEmpty().WithMessage("CustomerPhoneNumber e obrigatorio.")
            .MaximumLength(30).WithMessage("CustomerPhoneNumber deve ter no maximo 30 caracteres.");

        RuleFor(request => request.CustomerAddress)
            .NotEmpty().WithMessage("CustomerAddress e obrigatorio.")
            .MaximumLength(250).WithMessage("CustomerAddress deve ter no maximo 250 caracteres.");

        RuleFor(request => request.Notes)
            .MaximumLength(500)
            .WithMessage("Notes deve ter no maximo 500 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.Notes));
    }
}
