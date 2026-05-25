using Ecommerce.Application.Authorization;
using Ecommerce.Domain.Authorization;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql;

public static class DbSeeder
{
    public static async Task SeedAsync(EcommerceDbContext db, CancellationToken ct = default)
    {
        await EnsureRolesAsync(db, ct);

        if (await db.Users.AnyAsync(ct))
        {
            await SeedPhase4Async(db, ct);
            return;
        }

        var permissions = AdminPermissions.All.Select(code => new Permission
        {
            Code = code,
            Name = code
        }).ToList();
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync(ct);

        var adminRole = await db.Roles.FirstAsync(r => r.Code == RoleCodes.Admin, ct);
        var customerRole = await db.Roles.FirstAsync(r => r.Code == RoleCodes.Customer, ct);

        foreach (var perm in permissions)
            db.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
        await db.SaveChangesAsync(ct);

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

        var driverRole = await db.Roles.FirstAsync(r => r.Code == RoleCodes.Driver, ct);
        db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
        db.UserRoles.Add(new UserRole { UserId = customer.Id, RoleId = customerRole.Id });
        db.UserRoles.Add(new UserRole { UserId = driverUser.Id, RoleId = driverRole.Id });
        await db.SaveChangesAsync(ct);

        var family = new Family { Name = "Electrónica", Slug = "electronica", SortOrder = 1 };
        var category = new Category { Name = "Audio", Slug = "audio", SortOrder = 1, Family = family };
        var sub = new Subcategory { Name = "Audífonos", Slug = "audifonos", SortOrder = 1, Category = category };
        db.Families.Add(family);

        var product = new Product
        {
            Subcategory = sub,
            Name = "Audífonos Pro X",
            Slug = "audifonos-pro-x",
            Description = "Audífonos inalámbricos con cancelación de ruido",
            BasePrice = 199.99m,
            IsActive = true
        };
        db.Products.Add(product);

        var variant = new Variant { Product = product, Sku = "APX-001", Price = 199.99m, IsActive = true };
        db.Variants.Add(variant);
        db.ProductImages.Add(new ProductImage { Product = product, Url = "https://placehold.co/600x400", SortOrder = 1, IsPrimary = true });
        await db.SaveChangesAsync(ct);
        db.Inventories.Add(new Inventory { VariantId = variant.Id, QuantityOnHand = 50 });
        db.Covers.Add(new Cover
        {
            Title = "Bienvenido",
            ImageUrl = "https://placehold.co/1200x400",
            LinkUrl = "/catalog/products",
            SortOrder = 1,
            IsActive = true
        });
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

        await SeedPhase4Async(db, ct);
    }

    /// <summary>Opciones de variante, cupón demo y datos Fase 4 (también en BD ya sembrada).</summary>
    public static async Task SeedPhase4Async(EcommerceDbContext db, CancellationToken ct = default)
    {
        if (!await db.Coupons.AnyAsync(c => c.Code == "WELCOME10", ct))
        {
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

        var product = await db.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Slug == "audifonos-pro-x", ct);
        if (product is null) return;

        if (await db.ProductOptions.AnyAsync(o => o.ProductId == product.Id, ct)) return;

        var colorOption = new ProductOption { ProductId = product.Id, Name = "Color", SortOrder = 1 };
        db.ProductOptions.Add(colorOption);
        await db.SaveChangesAsync(ct);

        var negro = new OptionValue { ProductOptionId = colorOption.Id, Value = "Negro", SortOrder = 1 };
        var blanco = new OptionValue { ProductOptionId = colorOption.Id, Value = "Blanco", SortOrder = 2 };
        db.OptionValues.AddRange(negro, blanco);
        await db.SaveChangesAsync(ct);

        var variantNegro = product.Variants.First();
        db.VariantOptionValues.Add(new VariantOptionValue
        {
            VariantId = variantNegro.Id,
            OptionValueId = negro.Id
        });

        var variantBlanco = new Variant
        {
            ProductId = product.Id,
            Sku = "APX-002",
            Price = 209.99m,
            IsActive = true
        };
        db.Variants.Add(variantBlanco);
        await db.SaveChangesAsync(ct);
        db.Inventories.Add(new Inventory { VariantId = variantBlanco.Id, QuantityOnHand = 30 });
        db.VariantOptionValues.Add(new VariantOptionValue
        {
            VariantId = variantBlanco.Id,
            OptionValueId = blanco.Id
        });
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
