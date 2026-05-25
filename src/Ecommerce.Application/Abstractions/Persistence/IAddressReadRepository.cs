using Ecommerce.Application.DTOs.Addresses;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IAddressReadRepository
{
    Task<IReadOnlyList<AddressDto>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<AddressDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
}
