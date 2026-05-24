using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class AddressRepository(EcommerceDbContext db) : IAddressRepository
{
    public Task<List<Address>> ListByUserAsync(Guid userId, CancellationToken ct = default) =>
        db.Addresses.AsNoTracking().Where(a => a.UserId == userId).OrderByDescending(a => a.IsDefault).ToListAsync(ct);

    public Task<Address?> GetAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<Address> SaveAsync(Address address, CancellationToken ct = default)
    {
        if (address.IsDefault)
        {
            var others = await db.Addresses.Where(a => a.UserId == address.UserId && a.Id != address.Id).ToListAsync(ct);
            foreach (var o in others) o.IsDefault = false;
        }

        if (address.Id == Guid.Empty) db.Addresses.Add(address);
        else db.Addresses.Update(address);
        await db.SaveChangesAsync(ct);
        return address;
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);
        if (address is not null) db.Addresses.Remove(address);
        await db.SaveChangesAsync(ct);
    }

    public async Task SetDefaultAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var addresses = await db.Addresses.Where(a => a.UserId == userId).ToListAsync(ct);
        foreach (var a in addresses) a.IsDefault = a.Id == id;
        await db.SaveChangesAsync(ct);
    }
}
