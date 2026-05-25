using Ecommerce.Application.Features.Cart;
using FluentValidation;

namespace Ecommerce.Application.Features.Cart.Validators;

public class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class MergeCartCommandValidator : AbstractValidator<MergeCartCommand>
{
    public MergeCartCommandValidator()
    {
        RuleFor(x => x.GuestToken).NotEmpty();
    }
}
