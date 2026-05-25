using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class DriverRepository(EcommerceDbContext db) : IDriverRepository
{
    public Task<Driver?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        db.Drivers.FirstOrDefaultAsync(d => d.UserId == userId && d.IsActive, ct);

    public Task<Driver?> GetByIdAsync(Guid driverId, CancellationToken ct = default) =>
        db.Drivers.FirstOrDefaultAsync(d => d.Id == driverId, ct);

    public async Task<Driver> CreateAsync(Driver driver, CancellationToken ct = default)
    {
        db.Drivers.Add(driver);
        await db.SaveChangesAsync(ct);
        return driver;
    }
}
