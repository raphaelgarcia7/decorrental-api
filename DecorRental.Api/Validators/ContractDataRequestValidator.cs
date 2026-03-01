using System.Text.RegularExpressions;
using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class ContractDataRequestValidator : AbstractValidator<ContractDataRequest>
{
    public ContractDataRequestValidator()
    {
        RuleFor(request => request.KitThemeName)
            .NotEmpty().WithMessage("KitThemeName e obrigatorio.")
            .MaximumLength(120).WithMessage("KitThemeName deve ter no maximo 120 caracteres.");

        RuleFor(request => request.KitCategoryName)
            .NotEmpty().WithMessage("KitCategoryName e obrigatorio.")
            .MaximumLength(120).WithMessage("KitCategoryName deve ter no maximo 120 caracteres.");

        RuleFor(request => request.ReservationEndDate)
            .GreaterThanOrEqualTo(request => request.ReservationStartDate)
            .WithMessage("ReservationEndDate deve ser maior ou igual a ReservationStartDate.");

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

        RuleFor(request => request.CustomerZipCode)
            .Must(zipCode => string.IsNullOrWhiteSpace(zipCode) || Regex.IsMatch(new string(zipCode.Where(char.IsDigit).ToArray()), "^\\d{8}$"))
            .WithMessage("CustomerZipCode deve conter 8 digitos quando informado.");

        RuleFor(request => request.CustomerStreet)
            .NotEmpty().WithMessage("CustomerStreet e obrigatorio.")
            .MaximumLength(180).WithMessage("CustomerStreet deve ter no maximo 180 caracteres.");

        RuleFor(request => request.CustomerNumber)
            .NotEmpty().WithMessage("CustomerNumber e obrigatorio.")
            .MaximumLength(20).WithMessage("CustomerNumber deve ter no maximo 20 caracteres.");

        RuleFor(request => request.CustomerComplement)
            .MaximumLength(120).WithMessage("CustomerComplement deve ter no maximo 120 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerComplement));

        RuleFor(request => request.CustomerNeighborhood)
            .NotEmpty().WithMessage("CustomerNeighborhood e obrigatorio.")
            .MaximumLength(120).WithMessage("CustomerNeighborhood deve ter no maximo 120 caracteres.");

        RuleFor(request => request.CustomerCity)
            .NotEmpty().WithMessage("CustomerCity e obrigatorio.")
            .MaximumLength(120).WithMessage("CustomerCity deve ter no maximo 120 caracteres.");

        RuleFor(request => request.CustomerState)
            .NotEmpty().WithMessage("CustomerState e obrigatorio.")
            .Length(2).WithMessage("CustomerState deve ter exatamente 2 caracteres.");

        RuleFor(request => request.CustomerReference)
            .MaximumLength(250).WithMessage("CustomerReference deve ter no maximo 250 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerReference));

        RuleFor(request => request.Notes)
            .MaximumLength(500).WithMessage("Notes deve ter no maximo 500 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.Notes));

        RuleFor(request => request.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("TotalAmount deve ser maior ou igual a zero.")
            .When(request => request.TotalAmount.HasValue);

        RuleFor(request => request.EntryAmount)
            .GreaterThanOrEqualTo(0).WithMessage("EntryAmount deve ser maior ou igual a zero.")
            .When(request => request.EntryAmount.HasValue);
    }
}
