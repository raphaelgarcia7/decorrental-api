using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class GetKitsRequestValidator : AbstractValidator<GetKitsRequest>
{
    public GetKitsRequestValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
