using Ecommerce.Application.DTOs.Addresses;
using FluentValidation;

namespace Ecommerce.Application.Validators;

public class SaveAddressRequestValidator : AbstractValidator<SaveAddressRequest>
{
    public SaveAddressRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty();
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.State).NotEmpty();
        RuleFor(x => x.PostalCode).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
    }
}
