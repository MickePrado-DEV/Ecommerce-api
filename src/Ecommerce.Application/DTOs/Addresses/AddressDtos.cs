namespace Ecommerce.Application.DTOs.Addresses;

public record AddressDto(
    Guid Id,
    string Label,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Phone,
    bool IsDefault);

public record SaveAddressRequest(
    Guid? Id,
    string Label,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Phone,
    bool IsDefault);
