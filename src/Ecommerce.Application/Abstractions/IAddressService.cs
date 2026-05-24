using Ecommerce.Application.DTOs.Addresses;

namespace Ecommerce.Application.Abstractions;

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> ListAsync(Guid userId, CancellationToken ct = default);
    Task<AddressDto?> GetAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<AddressDto> SaveAsync(Guid userId, SaveAddressRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task SetDefaultAsync(Guid userId, Guid id, CancellationToken ct = default);
}
