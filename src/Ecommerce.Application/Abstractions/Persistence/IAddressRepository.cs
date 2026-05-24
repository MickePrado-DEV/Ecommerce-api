using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IAddressRepository
{
    Task<List<Address>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Address?> GetAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<Address> SaveAsync(Address address, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task SetDefaultAsync(Guid id, Guid userId, CancellationToken ct = default);
}
