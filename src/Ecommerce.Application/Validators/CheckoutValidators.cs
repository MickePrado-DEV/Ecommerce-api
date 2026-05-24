using Ecommerce.Application.DTOs.Checkout;
using FluentValidation;

namespace Ecommerce.Application.Validators;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.State).NotEmpty();
        RuleFor(x => x.PostalCode).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.ShippingCost).GreaterThanOrEqualTo(0);
    }
}
