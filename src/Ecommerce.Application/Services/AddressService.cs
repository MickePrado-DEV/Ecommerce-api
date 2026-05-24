using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Addresses;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class AddressService(IAddressRepository repo) : IAddressService
{
    public async Task<IReadOnlyList<AddressDto>> ListAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await repo.ListByUserAsync(userId, ct);
        return items.Select(Map).ToList();
    }

    public async Task<AddressDto?> GetAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var address = await repo.GetAsync(id, userId, ct);
        return address is null ? null : Map(address);
    }

    public async Task<AddressDto> SaveAsync(Guid userId, SaveAddressRequest request, CancellationToken ct = default)
    {
        var entity = new Address
        {
            Id = request.Id ?? Guid.Empty,
            UserId = userId,
            Label = request.Label,
            Street = request.Street,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Phone = request.Phone,
            IsDefault = request.IsDefault
        };
        var saved = await repo.SaveAsync(entity, ct);
        return Map(saved);
    }

    public Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default) =>
        repo.DeleteAsync(id, userId, ct);

    public async Task SetDefaultAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        _ = await repo.GetAsync(id, userId, ct) ?? throw new NotFoundException("Address", id);
        await repo.SetDefaultAsync(id, userId, ct);
    }

    private static AddressDto Map(Address a) => new(
        a.Id, a.Label, a.Street, a.City, a.State, a.PostalCode, a.Country, a.Phone, a.IsDefault);
}
