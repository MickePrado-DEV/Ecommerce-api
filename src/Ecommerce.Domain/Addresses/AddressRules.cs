using Ecommerce.Domain.Entities;
using FluentResults;

namespace Ecommerce.Domain.Addresses;

public static class AddressRules
{
    public const int LabelMaxLength = 100;
    public const int StreetMaxLength = 250;
    public const int CityMaxLength = 100;
    public const int StateMaxLength = 100;
    public const int PostalCodeMaxLength = 20;
    public const int CountryMaxLength = 3;
    public const int PhoneMaxLength = 30;

    public static Result<Address> Create(
        Guid userId,
        string label,
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        string phone,
        bool isDefault)
    {
        var validation = ValidateFields(label, street, city, state, postalCode, country, phone);
        if (validation.IsFailed)
            return Result.Fail<Address>(validation.Errors);

        return Result.Ok(new Address
        {
            UserId = userId,
            Label = label.Trim(),
            Street = street.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            PostalCode = postalCode.Trim(),
            Country = country.Trim().ToUpperInvariant(),
            Phone = phone.Trim(),
            IsDefault = isDefault
        });
    }

    public static Result ApplyUpdate(
        Address address,
        string label,
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        string phone,
        bool isDefault)
    {
        var validation = ValidateFields(label, street, city, state, postalCode, country, phone);
        if (validation.IsFailed) return validation;

        address.Label = label.Trim();
        address.Street = street.Trim();
        address.City = city.Trim();
        address.State = state.Trim();
        address.PostalCode = postalCode.Trim();
        address.Country = country.Trim().ToUpperInvariant();
        address.Phone = phone.Trim();
        address.IsDefault = isDefault;
        return Result.Ok();
    }

    private static Result ValidateFields(
        string label, string street, string city, string state,
        string postalCode, string country, string phone)
    {
        if (string.IsNullOrWhiteSpace(label) || label.Length > LabelMaxLength)
            return Result.Fail(AddressErrors.LabelInvalid(LabelMaxLength));

        if (string.IsNullOrWhiteSpace(street) || street.Length > StreetMaxLength)
            return Result.Fail(AddressErrors.StreetInvalid(StreetMaxLength));

        if (string.IsNullOrWhiteSpace(city) || city.Length > CityMaxLength)
            return Result.Fail(AddressErrors.CityInvalid(CityMaxLength));

        if (string.IsNullOrWhiteSpace(state) || state.Length > StateMaxLength)
            return Result.Fail(AddressErrors.StateInvalid(StateMaxLength));

        if (string.IsNullOrWhiteSpace(postalCode) || postalCode.Length > PostalCodeMaxLength)
            return Result.Fail(AddressErrors.PostalCodeInvalid(PostalCodeMaxLength));

        if (string.IsNullOrWhiteSpace(country) || country.Length > CountryMaxLength)
            return Result.Fail(AddressErrors.CountryInvalid(CountryMaxLength));

        if (string.IsNullOrWhiteSpace(phone) || phone.Length > PhoneMaxLength)
            return Result.Fail(AddressErrors.PhoneInvalid(PhoneMaxLength));

        return Result.Ok();
    }
}
