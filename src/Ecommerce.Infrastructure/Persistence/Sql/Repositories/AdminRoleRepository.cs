using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class AdminRoleRepository(EcommerceDbContext db) : IAdminRoleRepository
{
    public Task<List<Role>> ListAsync(CancellationToken ct = default) =>
        db.Roles.AsNoTracking()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Roles.AsNoTracking()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<Permission>> ListPermissionsAsync(CancellationToken ct = default) =>
        db.Permissions.AsNoTracking().OrderBy(p => p.Code).ToListAsync(ct);

    public async Task UpdatePermissionsAsync(Guid roleId, IReadOnlyList<string> permissionCodes, CancellationToken ct = default)
    {
        var role = await db.Roles.Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, ct)
            ?? throw new InvalidOperationException("Rol no encontrado");

        var distinct = permissionCodes.Distinct(StringComparer.Ordinal).ToList();
        var permissions = await db.Permissions.Where(p => distinct.Contains(p.Code)).ToListAsync(ct);
        if (permissions.Count != distinct.Count)
            throw new InvalidOperationException("Uno o más permisos no existen");

        db.RolePermissions.RemoveRange(role.RolePermissions);
        foreach (var permission in permissions)
            db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permission.Id });

        await db.SaveChangesAsync(ct);
    }
}
