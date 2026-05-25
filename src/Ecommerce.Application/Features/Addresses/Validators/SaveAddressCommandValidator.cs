using Ecommerce.Application.Features.Addresses.Commands;
using Ecommerce.Domain.Addresses;
using FluentValidation;

namespace Ecommerce.Application.Features.Addresses.Validators;

public class SaveAddressCommandValidator : AbstractValidator<SaveAddressCommand>
{
    public SaveAddressCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(AddressRules.LabelMaxLength);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(AddressRules.StreetMaxLength);
        RuleFor(x => x.City).NotEmpty().MaximumLength(AddressRules.CityMaxLength);
        RuleFor(x => x.State).NotEmpty().MaximumLength(AddressRules.StateMaxLength);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(AddressRules.PostalCodeMaxLength);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(AddressRules.CountryMaxLength);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(AddressRules.PhoneMaxLength);
    }
}
