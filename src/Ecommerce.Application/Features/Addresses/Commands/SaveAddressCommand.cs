using Ecommerce.Application.DTOs.Addresses;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Commands;

public record SaveAddressCommand(
    Guid UserId,
    Guid? Id,
    int Type,
    string Label,
    string ContactName,
    string Street,
    string ExternalNumber,
    string? InternalNumber,
    string Neighborhood,
    string Municipality,
    string? City,
    string State,
    string PostalCode,
    string Country,
    string Phone,
    string? References,
    string? DeliveryInstructions,
    decimal? Latitude,
    decimal? Longitude,
    bool IsDefault) : IRequest<Result<AddressDto>>;

public static class SaveAddressCommandMapping
{
    public static SaveAddressCommand FromRequest(Guid userId, SaveAddressRequest request) => new(
        userId,
        request.Id,
        request.Type,
        request.Label,
        request.ContactName,
        request.Street,
        request.ExternalNumber,
        request.InternalNumber,
        request.Neighborhood,
        request.Municipality,
        request.City,
        request.State,
        request.PostalCode,
        request.Country,
        request.Phone,
        request.References,
        request.DeliveryInstructions,
        request.Latitude,
        request.Longitude,
        request.IsDefault);
}
