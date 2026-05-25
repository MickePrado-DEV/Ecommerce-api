using Ecommerce.Application.Features.Addresses.Commands;
using Ecommerce.Domain.Addresses;
using FluentValidation;

namespace Ecommerce.Application.Features.Addresses.Validators;

public class SaveAddressCommandValidator : AbstractValidator<SaveAddressCommand>
{
    public SaveAddressCommandValidator()
    {
        RuleFor(x => x.Type).Must(t => AddressRules.AllowedTypes.Contains(t));
        RuleFor(x => x.Label).NotEmpty().MaximumLength(AddressRules.LabelMaxLength);
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(AddressRules.ContactNameMaxLength);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(AddressRules.StreetMaxLength);
        RuleFor(x => x.ExternalNumber).NotEmpty().MaximumLength(AddressRules.ExternalNumberMaxLength);
        RuleFor(x => x.Neighborhood).NotEmpty().MaximumLength(AddressRules.NeighborhoodMaxLength);
        RuleFor(x => x.Municipality).NotEmpty().MaximumLength(AddressRules.MunicipalityMaxLength);
        RuleFor(x => x.State).NotEmpty().MaximumLength(AddressRules.StateMaxLength);
        RuleFor(x => x.PostalCode).NotEmpty().Length(5).Matches(@"^\d{5}$");
        RuleFor(x => x.Country).NotEmpty().MaximumLength(AddressRules.CountryMaxLength);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(AddressRules.PhoneMaxLength);
    }
}
