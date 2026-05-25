using Ecommerce.Domain.Entities;
using FluentResults;

namespace Ecommerce.Domain.Addresses;

public static class AddressRules
{
    public const int MaxPerUser = 5;

    public const int LabelMaxLength = 100;
    public const int ContactNameMaxLength = 120;
    public const int StreetMaxLength = 250;
    public const int ExternalNumberMaxLength = 20;
    public const int InternalNumberMaxLength = 20;
    public const int NeighborhoodMaxLength = 120;
    public const int MunicipalityMaxLength = 120;
    public const int CityMaxLength = 100;
    public const int StateMaxLength = 100;
    public const int PostalCodeMaxLength = 20;
    public const int CountryMaxLength = 3;
    public const int PhoneMaxLength = 30;
    public const int ReferencesMaxLength = 500;
    public const int DeliveryInstructionsMaxLength = 500;

    public static readonly int[] AllowedTypes = [1, 2, 3, 4];

    public static Result<Address> Create(
        Guid userId,
        int type,
        string label,
        string contactName,
        string street,
        string externalNumber,
        string? internalNumber,
        string neighborhood,
        string municipality,
        string? city,
        string state,
        string postalCode,
        string country,
        string phone,
        string? references,
        string? deliveryInstructions,
        decimal? latitude,
        decimal? longitude,
        bool isDefault)
    {
        var validation = ValidateFields(
            type, label, contactName, street, externalNumber, neighborhood, municipality,
            city, state, postalCode, country, phone, references, deliveryInstructions, latitude, longitude);
        if (validation.IsFailed)
            return Result.Fail<Address>(validation.Errors);

        return Result.Ok(new Address
        {
            UserId = userId,
            Type = type,
            Label = label.Trim(),
            ContactName = contactName.Trim(),
            Street = street.Trim(),
            ExternalNumber = externalNumber.Trim(),
            InternalNumber = internalNumber?.Trim(),
            Neighborhood = neighborhood.Trim(),
            Municipality = municipality.Trim(),
            City = (city ?? municipality).Trim(),
            State = state.Trim(),
            PostalCode = postalCode.Trim(),
            Country = country.Trim().ToUpperInvariant(),
            Phone = phone.Trim(),
            References = references?.Trim(),
            DeliveryInstructions = deliveryInstructions?.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            IsDefault = isDefault
        });
    }

    public static Result ApplyUpdate(
        Address address,
        int type,
        string label,
        string contactName,
        string street,
        string externalNumber,
        string? internalNumber,
        string neighborhood,
        string municipality,
        string? city,
        string state,
        string postalCode,
        string country,
        string phone,
        string? references,
        string? deliveryInstructions,
        decimal? latitude,
        decimal? longitude,
        bool isDefault)
    {
        var validation = ValidateFields(
            type, label, contactName, street, externalNumber, neighborhood, municipality,
            city, state, postalCode, country, phone, references, deliveryInstructions, latitude, longitude);
        if (validation.IsFailed) return validation;

        address.Type = type;
        address.Label = label.Trim();
        address.ContactName = contactName.Trim();
        address.Street = street.Trim();
        address.ExternalNumber = externalNumber.Trim();
        address.InternalNumber = internalNumber?.Trim();
        address.Neighborhood = neighborhood.Trim();
        address.Municipality = municipality.Trim();
        address.City = (city ?? municipality).Trim();
        address.State = state.Trim();
        address.PostalCode = postalCode.Trim();
        address.Country = country.Trim().ToUpperInvariant();
        address.Phone = phone.Trim();
        address.References = references?.Trim();
        address.DeliveryInstructions = deliveryInstructions?.Trim();
        address.Latitude = latitude;
        address.Longitude = longitude;
        address.IsDefault = isDefault;
        return Result.Ok();
    }

    private static Result ValidateFields(
        int type, string label, string contactName, string street, string externalNumber,
        string neighborhood, string municipality, string? city, string state,
        string postalCode, string country, string phone,
        string? references, string? deliveryInstructions,
        decimal? latitude, decimal? longitude)
    {
        if (!AllowedTypes.Contains(type))
            return Result.Fail(AddressErrors.Validation("Tipo de dirección inválido.", "type"));

        if (string.IsNullOrWhiteSpace(label) || label.Length > LabelMaxLength)
            return Result.Fail(AddressErrors.LabelInvalid(LabelMaxLength));

        if (string.IsNullOrWhiteSpace(contactName) || contactName.Length > ContactNameMaxLength)
            return Result.Fail(AddressErrors.Validation("Nombre de contacto requerido.", "contactName"));

        if (string.IsNullOrWhiteSpace(street) || street.Length > StreetMaxLength)
            return Result.Fail(AddressErrors.StreetInvalid(StreetMaxLength));

        if (string.IsNullOrWhiteSpace(externalNumber) || externalNumber.Length > ExternalNumberMaxLength)
            return Result.Fail(AddressErrors.Validation("Número exterior requerido.", "externalNumber"));

        if (string.IsNullOrWhiteSpace(neighborhood) || neighborhood.Length > NeighborhoodMaxLength)
            return Result.Fail(AddressErrors.Validation("Colonia requerida.", "neighborhood"));

        if (string.IsNullOrWhiteSpace(municipality) || municipality.Length > MunicipalityMaxLength)
            return Result.Fail(AddressErrors.Validation("Municipio requerido.", "municipality"));

        var cityVal = city ?? municipality;
        if (string.IsNullOrWhiteSpace(cityVal) || cityVal.Length > CityMaxLength)
            return Result.Fail(AddressErrors.CityInvalid(CityMaxLength));

        if (string.IsNullOrWhiteSpace(state) || state.Length > StateMaxLength)
            return Result.Fail(AddressErrors.StateInvalid(StateMaxLength));

        if (string.IsNullOrWhiteSpace(postalCode) || postalCode.Length > PostalCodeMaxLength || postalCode.Length != 5 || !postalCode.All(char.IsDigit))
            return Result.Fail(AddressErrors.Validation("Código postal de 5 dígitos requerido.", "postalCode"));

        if (string.IsNullOrWhiteSpace(country) || country.Length > CountryMaxLength)
            return Result.Fail(AddressErrors.CountryInvalid(CountryMaxLength));

        if (string.IsNullOrWhiteSpace(phone) || phone.Length > PhoneMaxLength)
            return Result.Fail(AddressErrors.PhoneInvalid(PhoneMaxLength));

        if (references?.Length > ReferencesMaxLength)
            return Result.Fail(AddressErrors.Validation("Referencias demasiado largas.", "references"));

        if (deliveryInstructions?.Length > DeliveryInstructionsMaxLength)
            return Result.Fail(AddressErrors.Validation("Instrucciones demasiado largas.", "deliveryInstructions"));

        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            return Result.Fail(AddressErrors.Validation("Coordenadas inválidas.", "latitude"));

        return Result.Ok();
    }
}
