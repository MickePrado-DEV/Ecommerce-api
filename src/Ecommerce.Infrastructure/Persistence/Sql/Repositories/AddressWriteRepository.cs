using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class AddressWriteRepository(EcommerceDbContext db) : IAddressWriteRepository
{
    public Task<Address?> GetTrackedAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<Address> AddAsync(Address address, CancellationToken ct = default)
    {
        await EnsureUserExistsAsync(address.UserId, ct);

        if (address.IsDefault)
            await ClearOtherDefaultsAsync(address.UserId, Guid.Empty, ct);

        address.User = null;
        db.Addresses.Add(address);
        await SaveAsync(ct);
        return address;
    }

    public async Task UpdateAsync(Address address, CancellationToken ct = default)
    {
        await EnsureUserExistsAsync(address.UserId, ct);

        if (address.IsDefault)
            await ClearOtherDefaultsAsync(address.UserId, address.Id, ct);

        address.User = null;
        db.Addresses.Update(address);
        await SaveAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);
        if (address is not null)
        {
            db.Addresses.Remove(address);
            await SaveAsync(ct);
        }
    }

    public async Task SetDefaultAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var addresses = await db.Addresses.Where(a => a.UserId == userId).ToListAsync(ct);
        foreach (var a in addresses)
            a.IsDefault = a.Id == id;
        await SaveAsync(ct);
    }

    private async Task EnsureUserExistsAsync(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            throw new InvalidOperationException("ADDRESS_USER_MISSING: UserId vacío.");

        var exists = await db.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.IsActive, ct);
        if (!exists)
            throw new InvalidOperationException(
                "ADDRESS_USER_MISSING: Tu sesión no coincide con la base de datos. Cierra sesión e inicia de nuevo.");
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(BuildDbErrorMessage(ex), ex);
        }
    }

    private static string BuildDbErrorMessage(DbUpdateException ex)
    {
        var sql = FindSqlException(ex);
        if (sql?.Number == 547)
            return $"ADDRESS_FK_VIOLATION: {sql.Message}";

        return $"ADDRESS_DB_ERROR: {ex.InnerException?.Message ?? ex.Message}";
    }

    private static SqlException? FindSqlException(Exception ex)
    {
        for (var cur = ex; cur is not null; cur = cur.InnerException)
            if (cur is SqlException sql) return sql;
        return null;
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
