using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IDriverRepository
{
    Task<Driver?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Driver?> GetByIdAsync(Guid driverId, CancellationToken ct = default);
    Task<Driver> CreateAsync(Driver driver, CancellationToken ct = default);
}
