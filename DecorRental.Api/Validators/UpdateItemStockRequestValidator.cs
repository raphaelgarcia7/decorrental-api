using DecorRental.Api.Contracts;
using FluentValidation;

namespace DecorRental.Api.Validators;

public sealed class UpdateItemStockRequestValidator : AbstractValidator<UpdateItemStockRequest>
{
    public UpdateItemStockRequestValidator()
    {
        RuleFor(request => request.TotalStock)
            .GreaterThanOrEqualTo(0).WithMessage("TotalStock deve ser zero ou maior.");
    }
}
