using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class GetKitsRequestValidator : AbstractValidator<GetKitsRequest>
{
    public GetKitsRequestValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page deve ser maior ou igual a 1.");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize deve estar entre 1 e 100.");
    }
}
