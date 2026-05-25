using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Addresses;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class AddressReadRepository(EcommerceDbContext db) : IAddressReadRepository
{
    public async Task<IReadOnlyList<AddressDto>> ListByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await db.Addresses.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .Select(a => new AddressDto(
                a.Id, a.Label, a.Street, a.City, a.State,
                a.PostalCode, a.Country, a.Phone, a.IsDefault))
            .ToListAsync(ct);
        return list;
    }

    public Task<AddressDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Addresses.AsNoTracking()
            .Where(a => a.Id == id && a.UserId == userId)
            .Select(a => new AddressDto(
                a.Id, a.Label, a.Street, a.City, a.State,
                a.PostalCode, a.Country, a.Phone, a.IsDefault))
            .FirstOrDefaultAsync(ct);
}
