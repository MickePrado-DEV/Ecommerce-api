using Ecommerce.Application.Features.Checkout;
using FluentValidation;

namespace Ecommerce.Application.Features.Checkout.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ShippingCost).GreaterThanOrEqualTo(0);
        When(x => x.AddressId is null, () =>
        {
            RuleFor(x => x.FullName).NotEmpty();
            RuleFor(x => x.Street).NotEmpty();
            RuleFor(x => x.City).NotEmpty();
            RuleFor(x => x.State).NotEmpty();
            RuleFor(x => x.PostalCode).NotEmpty();
            RuleFor(x => x.Country).NotEmpty();
            RuleFor(x => x.Phone).NotEmpty();
        });
    }
}
