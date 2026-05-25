using Ecommerce.Application.Features.Addresses.Commands;
using Ecommerce.Domain.Addresses;
using FluentValidation;

namespace Ecommerce.Application.Features.Addresses.Validators;

public class SaveAddressCommandValidator : AbstractValidator<SaveAddressCommand>
{
    public SaveAddressCommandValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => AddressRules.AllowedTypes.Contains(t))
            .WithMessage("Tipo de dirección inválido.");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("La etiqueta es requerida.")
            .MaximumLength(AddressRules.LabelMaxLength);

        RuleFor(x => x.ContactName)
            .NotEmpty().WithMessage("El nombre de contacto es requerido.")
            .MaximumLength(AddressRules.ContactNameMaxLength);

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("La calle es requerida.")
            .MaximumLength(AddressRules.StreetMaxLength);

        RuleFor(x => x.ExternalNumber)
            .NotEmpty().WithMessage("El número exterior es requerido.")
            .MaximumLength(AddressRules.ExternalNumberMaxLength);

        RuleFor(x => x.Neighborhood)
            .NotEmpty().WithMessage("La colonia es requerida.")
            .MaximumLength(AddressRules.NeighborhoodMaxLength);

        RuleFor(x => x.Municipality)
            .NotEmpty().WithMessage("El municipio es requerido.")
            .MaximumLength(AddressRules.MunicipalityMaxLength);

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("El estado es requerido.")
            .MaximumLength(AddressRules.StateMaxLength);

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("El código postal es requerido.")
            .Length(5).WithMessage("El código postal debe tener 5 dígitos.")
            .Matches(@"^\d{5}$").WithMessage("El código postal solo debe contener números.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("El país es requerido.")
            .MaximumLength(AddressRules.CountryMaxLength);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es requerido.")
            .MaximumLength(AddressRules.PhoneMaxLength);
    }
}
