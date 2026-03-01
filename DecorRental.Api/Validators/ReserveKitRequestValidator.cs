using DecorRental.Api.Contracts;
using FluentValidation;
using System.Text.RegularExpressions;

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
            .MaximumLength(250).WithMessage("CustomerAddress deve ter no maximo 250 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerAddress));

        RuleFor(request => request.CustomerZipCode)
            .Must(zipCode => string.IsNullOrWhiteSpace(zipCode) || Regex.IsMatch(new string(zipCode.Where(char.IsDigit).ToArray()), "^\\d{8}$"))
            .WithMessage("CustomerZipCode deve conter 8 digitos quando informado.");

        RuleFor(request => request.CustomerStreet)
            .MaximumLength(180).WithMessage("CustomerStreet deve ter no maximo 180 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerStreet));

        RuleFor(request => request.CustomerNumber)
            .MaximumLength(20).WithMessage("CustomerNumber deve ter no maximo 20 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerNumber));

        RuleFor(request => request.CustomerComplement)
            .MaximumLength(120).WithMessage("CustomerComplement deve ter no maximo 120 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerComplement));

        RuleFor(request => request.CustomerNeighborhood)
            .MaximumLength(120).WithMessage("CustomerNeighborhood deve ter no maximo 120 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerNeighborhood));

        RuleFor(request => request.CustomerCity)
            .MaximumLength(120).WithMessage("CustomerCity deve ter no maximo 120 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerCity));

        RuleFor(request => request.CustomerState)
            .Length(2).WithMessage("CustomerState deve ter exatamente 2 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerState));

        RuleFor(request => request.CustomerReference)
            .MaximumLength(250).WithMessage("CustomerReference deve ter no maximo 250 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerReference));

        RuleFor(request => request.Notes)
            .MaximumLength(500)
            .WithMessage("Notes deve ter no maximo 500 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.Notes));

        RuleFor(request => request)
            .Must(HasAddressInformation)
            .WithMessage("Informe CustomerAddress ou CustomerStreet e CustomerNumber.");
    }

    private static bool HasAddressInformation(ReserveKitRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.CustomerAddress))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(request.CustomerStreet) &&
               !string.IsNullOrWhiteSpace(request.CustomerNumber);
    }
}
