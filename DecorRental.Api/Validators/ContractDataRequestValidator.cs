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

        RuleFor(request => request.CustomerNeighborhood)
            .MaximumLength(120)
            .WithMessage("CustomerNeighborhood deve ter no maximo 120 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerNeighborhood));

        RuleFor(request => request.CustomerCity)
            .MaximumLength(120)
            .WithMessage("CustomerCity deve ter no maximo 120 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerCity));

        RuleFor(request => request.Notes)
            .MaximumLength(500)
            .WithMessage("Notes deve ter no maximo 500 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.Notes));

        RuleFor(request => request.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TotalAmount deve ser maior ou igual a zero.")
            .When(request => request.TotalAmount.HasValue);

        RuleFor(request => request.EntryAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("EntryAmount deve ser maior ou igual a zero.")
            .When(request => request.EntryAmount.HasValue);
    }
}
