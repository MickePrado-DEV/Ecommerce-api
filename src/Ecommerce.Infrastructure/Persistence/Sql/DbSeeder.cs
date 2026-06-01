using Ecommerce.Application.Authorization;
using Ecommerce.Domain.Authorization;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Ecommerce.Infrastructure.Persistence.Sql.Seed;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql;

public static class DbSeeder
{
    public static async Task SeedAsync(EcommerceDbContext db, CancellationToken ct = default)
    {
        await EnsureRolesAsync(db, ct);
        await EnsureAdminPermissionsAsync(db, ct);
        await CatalogSeeder.EnsureTaxonomyAsync(db, ct);
        await CatalogSeeder.EnsureGlobalOptionsAsync(db, ct);
        await CatalogSeeder.EnsureProductsAsync(db, ct);
        await EnsureCouponAsync(db, ct);

        if (await db.Users.AnyAsync(ct))
            return;

        var adminRole = await db.Roles.FirstAsync(r => r.Code == RoleCodes.Admin, ct);
        var customerRole = await db.Roles.FirstAsync(r => r.Code == RoleCodes.Customer, ct);
        var driverRole = await db.Roles.FirstAsync(r => r.Code == RoleCodes.Driver, ct);

        var admin = new User
        {
            Email = "admin@ecommerce.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FirstName = "Admin",
            LastName = "Sistema"
        };
        var customer = new User
        {
            Email = "cliente@ecommerce.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cliente123!"),
            FirstName = "Cliente",
            LastName = "Demo",
            Phone = "+5215550100"
        };
        var driverUser = new User
        {
            Email = "repartidor@ecommerce.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Repartidor123!"),
            FirstName = "Juan",
            LastName = "Repartidor",
            Phone = "+5215550001"
        };
        db.Users.AddRange(admin, customer, driverUser);
        await db.SaveChangesAsync(ct);

        db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
        db.UserRoles.Add(new UserRole { UserId = customer.Id, RoleId = customerRole.Id });
        db.UserRoles.Add(new UserRole { UserId = driverUser.Id, RoleId = driverRole.Id });
        await db.SaveChangesAsync(ct);

        db.Drivers.Add(new Driver
        {
            UserId = driverUser.Id,
            Name = "Juan Repartidor",
            Phone = "+5215550001",
            LicenseNumber = "LIC-DEMO-001",
            VehiclePlate = "ABC-123",
            IsActive = true
        });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Cupón demo (también en BD ya sembrada).</summary>
    public static async Task EnsureCouponAsync(EcommerceDbContext db, CancellationToken ct = default)
    {
        if (await db.Coupons.AnyAsync(c => c.Code == "WELCOME10", ct))
            return;

        db.Coupons.Add(new Coupon
        {
            Code = "WELCOME10",
            DiscountType = CouponDiscountType.Percent,
            Value = 10,
            MinSubtotal = 50,
            MaxUses = 1000,
            IsActive = true
        });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Sincroniza permisos nuevos con el rol admin (BD ya existente).</summary>
    public static async Task EnsureAdminPermissionsAsync(EcommerceDbContext db, CancellationToken ct = default)
    {
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.Admin, ct);
        if (adminRole is null) return;

        foreach (var code in AdminPermissions.All)
        {
            var perm = await db.Permissions.FirstOrDefaultAsync(p => p.Code == code, ct);
            if (perm is null)
            {
                perm = new Permission { Code = code, Name = code };
                db.Permissions.Add(perm);
                await db.SaveChangesAsync(ct);
            }

            var linked = await db.RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == perm.Id, ct);
            if (!linked)
                db.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>Asegura roles admin, customer y driver (BD existentes sin recrear).</summary>
    public static async Task EnsureRolesAsync(EcommerceDbContext db, CancellationToken ct = default)
    {
        if (!await db.Roles.AnyAsync(r => r.Code == RoleCodes.Admin, ct))
            db.Roles.Add(new Role { Name = "Administrador", Code = RoleCodes.Admin });
        if (!await db.Roles.AnyAsync(r => r.Code == RoleCodes.Customer, ct))
            db.Roles.Add(new Role { Name = "Cliente", Code = RoleCodes.Customer });
        if (!await db.Roles.AnyAsync(r => r.Code == RoleCodes.Driver, ct))
            db.Roles.Add(new Role { Name = "Repartidor", Code = RoleCodes.Driver });
        await db.SaveChangesAsync(ct);
    }
}
