using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class UpdateItemStockRequestValidator : AbstractValidator<UpdateItemStockRequest>
{
    public UpdateItemStockRequestValidator()
    {
        RuleFor(request => request.TotalStock)
            .GreaterThanOrEqualTo(0).WithMessage("TotalStock must be zero or greater.");
    }
}
