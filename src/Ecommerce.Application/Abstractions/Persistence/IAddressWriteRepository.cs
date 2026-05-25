using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IAddressWriteRepository
{
    Task<Address?> GetTrackedAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<Address> AddAsync(Address address, CancellationToken ct = default);
    Task UpdateAsync(Address address, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task SetDefaultAsync(Guid id, Guid userId, CancellationToken ct = default);
}
