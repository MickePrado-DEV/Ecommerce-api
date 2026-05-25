// Command: crear o actualizar dirección del usuario (mapeo desde SaveAddressRequest del endpoint).
using Ecommerce.Application.DTOs.Addresses;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Commands;

public record SaveAddressCommand(
    Guid UserId,
    Guid? Id,
    string Label,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Phone,
    bool IsDefault) : IRequest<Result<AddressDto>>;

public static class SaveAddressCommandMapping
{
    public static SaveAddressCommand FromRequest(Guid userId, SaveAddressRequest request) => new(
        userId,
        request.Id,
        request.Label,
        request.Street,
        request.City,
        request.State,
        request.PostalCode,
        request.Country,
        request.Phone,
        request.IsDefault);
}
