using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class AddressWriteRepository(EcommerceDbContext db) : IAddressWriteRepository
{
    public Task<Address?> GetTrackedAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<Address> AddAsync(Address address, CancellationToken ct = default)
    {
        if (address.IsDefault)
            await ClearOtherDefaultsAsync(address.UserId, Guid.Empty, ct);

        db.Addresses.Add(address);
        await db.SaveChangesAsync(ct);
        return address;
    }

    public async Task UpdateAsync(Address address, CancellationToken ct = default)
    {
        if (address.IsDefault)
            await ClearOtherDefaultsAsync(address.UserId, address.Id, ct);

        db.Addresses.Update(address);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);
        if (address is not null)
        {
            db.Addresses.Remove(address);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task SetDefaultAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var addresses = await db.Addresses.Where(a => a.UserId == userId).ToListAsync(ct);
        foreach (var a in addresses)
            a.IsDefault = a.Id == id;
        await db.SaveChangesAsync(ct);
    }

    private async Task ClearOtherDefaultsAsync(Guid userId, Guid exceptId, CancellationToken ct)
    {
        var others = await db.Addresses
            .Where(a => a.UserId == userId && a.Id != exceptId && a.IsDefault)
            .ToListAsync(ct);
        foreach (var o in others)
            o.IsDefault = false;
    }
}
