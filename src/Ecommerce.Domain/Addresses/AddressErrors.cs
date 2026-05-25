using FluentResults;

namespace Ecommerce.Domain.Addresses;

public static class AddressErrors
{
    public const string NotFoundCode = "Address.NotFound";
    public const string ValidationCode = "Address.Validation";

    public static Error NotFound(Guid id) =>
        new Error($"Dirección {id} no encontrada").WithMetadata("Code", NotFoundCode);

    public static Error LabelInvalid(int max) =>
        ValidationError($"Label es requerido y máximo {max} caracteres.", "label");

    public static Error StreetInvalid(int max) =>
        ValidationError($"Street es requerido y máximo {max} caracteres.", "street");

    public static Error CityInvalid(int max) =>
        ValidationError($"City es requerido y máximo {max} caracteres.", "city");

    public static Error StateInvalid(int max) =>
        ValidationError($"State es requerido y máximo {max} caracteres.", "state");

    public static Error PostalCodeInvalid(int max) =>
        ValidationError($"PostalCode es requerido y máximo {max} caracteres.", "postalCode");

    public static Error CountryInvalid(int max) =>
        ValidationError($"Country es requerido y máximo {max} caracteres (ISO).", "country");

    public static Error PhoneInvalid(int max) =>
        ValidationError($"Phone es requerido y máximo {max} caracteres.", "phone");

    private static Error ValidationError(string message, string propertyName) =>
        new Error(message)
            .WithMetadata("Code", ValidationCode)
            .WithMetadata("PropertyName", propertyName);
}
