using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class AdminUserRepository(EcommerceDbContext db) : IAdminUserRepository
{
    public async Task<(IReadOnlyList<User> Items, int Total)> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var q = db.Users.AsNoTracking().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User> CreateAsync(User user, IReadOnlyList<string> roleCodes, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var roles = await db.Roles.Where(r => roleCodes.Contains(r.Code)).ToListAsync(ct);
        foreach (var role in roles)
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(user.Id, ct))!;
    }

    public async Task UpdateAsync(Guid id, bool? isActive, IReadOnlyList<string>? roleCodes, CancellationToken ct = default)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        if (isActive.HasValue)
            user.IsActive = isActive.Value;

        if (roleCodes is not null)
        {
            var roles = await db.Roles.Where(r => roleCodes.Contains(r.Code)).ToListAsync(ct);
            db.UserRoles.RemoveRange(user.UserRoles);
            foreach (var role in roles)
                db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
