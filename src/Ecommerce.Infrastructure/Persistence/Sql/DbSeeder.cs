using Ecommerce.Application.Authorization;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql;

public static class DbSeeder
{
    public static async Task SeedAsync(EcommerceDbContext db, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync()) return;

        var permissions = AdminPermissions.All.Select(code => new Permission
        {
            Code = code,
            Name = code
        }).ToList();
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();

        var adminRole = new Role { Name = "Administrador", Code = "admin" };
        var customerRole = new Role { Name = "Cliente", Code = "customer" };
        db.Roles.AddRange(adminRole, customerRole);
        await db.SaveChangesAsync();

        foreach (var perm in permissions)
            db.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
        await db.SaveChangesAsync();

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
            LastName = "Demo"
        };
        db.Users.AddRange(admin, customer);
        await db.SaveChangesAsync();
        db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
        db.UserRoles.Add(new UserRole { UserId = customer.Id, RoleId = customerRole.Id });
        await db.SaveChangesAsync();

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
        await db.SaveChangesAsync();
        db.Inventories.Add(new Inventory { VariantId = variant.Id, QuantityOnHand = 50 });
        db.Drivers.Add(new Driver { Name = "Juan Repartidor", Phone = "+5215550001", IsActive = true });
        await db.SaveChangesAsync();
    }
}
