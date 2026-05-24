-- =============================================================================
-- ECOMMERCE API — Datos iniciales (SQL Server)
-- Requiere schema.sqlserver.sql ejecutado antes
-- Alineado con DbSeeder.cs y Postman (admin@ecommerce.local / cliente@ecommerce.local)
-- =============================================================================
-- sqlcmd -S "(localdb)\mssqllocaldb" -E -d ecommerce -i seed.sqlserver.sql
-- =============================================================================

USE ecommerce;
GO

SET NOCOUNT ON;
DECLARE @now DATETIME2 = SYSUTCDATETIME();

-- IDs fijos para referencias y pruebas
DECLARE @roleAdmin    UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111101';
DECLARE @roleCustomer UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111102';
DECLARE @userAdmin    UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222201';
DECLARE @userCustomer UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222202';
DECLARE @familyId     UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333301';
DECLARE @categoryId   UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333302';
DECLARE @subcategoryId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333303';
DECLARE @productId    UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444401';
DECLARE @variantId    UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444402';
DECLARE @driverId     UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555501';

-- ---------------------------------------------------------------------------
-- Permisos (admin.* — mismos códigos que AdminPermissions.cs)
-- ---------------------------------------------------------------------------

INSERT INTO permissions (Id, Code, Name, CreatedAt, UpdatedAt)
SELECT NEWID(), v.Code, v.Code, @now, @now
FROM (VALUES
    ('admin.dashboard.view'),
    ('admin.covers.view'),
    ('admin.covers.manage'),
    ('admin.families.view'),
    ('admin.families.manage'),
    ('admin.categories.view'),
    ('admin.categories.manage'),
    ('admin.subcategories.view'),
    ('admin.subcategories.manage'),
    ('admin.products.view'),
    ('admin.products.manage'),
    ('admin.options.view'),
    ('admin.options.manage'),
    ('admin.stock.view'),
    ('admin.stock.manage'),
    ('admin.drivers.view'),
    ('admin.drivers.manage'),
    ('admin.orders.view'),
    ('admin.orders.manage'),
    ('admin.shipments.view'),
    ('admin.shipments.manage')
) AS v(Code)
WHERE NOT EXISTS (SELECT 1 FROM permissions p WHERE p.Code = v.Code);

-- ---------------------------------------------------------------------------
-- Roles
-- ---------------------------------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM roles WHERE Id = @roleAdmin)
    INSERT INTO roles (Id, Name, Code, CreatedAt, UpdatedAt)
    VALUES (@roleAdmin, N'Administrador', N'admin', @now, @now);

IF NOT EXISTS (SELECT 1 FROM roles WHERE Id = @roleCustomer)
    INSERT INTO roles (Id, Name, Code, CreatedAt, UpdatedAt)
    VALUES (@roleCustomer, N'Cliente', N'customer', @now, @now);

-- Admin → todos los permisos
INSERT INTO role_permissions (RoleId, PermissionId)
SELECT @roleAdmin, p.Id
FROM permissions p
WHERE NOT EXISTS (
    SELECT 1 FROM role_permissions rp
    WHERE rp.RoleId = @roleAdmin AND rp.PermissionId = p.Id
);

-- ---------------------------------------------------------------------------
-- Usuarios (BCrypt — generados con scriptsSql/tools/HashGen)
-- Admin123!  / Cliente123!
-- ---------------------------------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM users WHERE Email = N'admin@ecommerce.local')
    INSERT INTO users (Id, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedAt, UpdatedAt)
    VALUES (
        @userAdmin,
        N'admin@ecommerce.local',
        N'$2a$11$ATsEPSiT1ctqwo0bAcTehOVNJAc6kwRfGi1hFUro7IBFNz945Y7Iq',
        N'Admin', N'Sistema', NULL, 1, @now, @now
    );

IF NOT EXISTS (SELECT 1 FROM users WHERE Email = N'cliente@ecommerce.local')
    INSERT INTO users (Id, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedAt, UpdatedAt)
    VALUES (
        @userCustomer,
        N'cliente@ecommerce.local',
        N'$2a$11$6bgD9QCHuTRmzifRKM77d.nKHDwSjB5OeLnn18mqRKaz6.hI/L5bm',
        N'Cliente', N'Demo', NULL, 1, @now, @now
    );

IF NOT EXISTS (SELECT 1 FROM user_roles WHERE UserId = @userAdmin AND RoleId = @roleAdmin)
    INSERT INTO user_roles (UserId, RoleId) VALUES (@userAdmin, @roleAdmin);

IF NOT EXISTS (SELECT 1 FROM user_roles WHERE UserId = @userCustomer AND RoleId = @roleCustomer)
    INSERT INTO user_roles (UserId, RoleId) VALUES (@userCustomer, @roleCustomer);

-- ---------------------------------------------------------------------------
-- Catálogo demo (Postman: productSlug = audifonos-pro-x)
-- ---------------------------------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM families WHERE Id = @familyId)
    INSERT INTO families (Id, Name, Slug, SortOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (@familyId, N'Electrónica', N'electronica', 1, 1, @now, @now);

IF NOT EXISTS (SELECT 1 FROM categories WHERE Id = @categoryId)
    INSERT INTO categories (Id, FamilyId, Name, Slug, SortOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (@categoryId, @familyId, N'Audio', N'audio', 1, 1, @now, @now);

IF NOT EXISTS (SELECT 1 FROM subcategories WHERE Id = @subcategoryId)
    INSERT INTO subcategories (Id, CategoryId, Name, Slug, SortOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (@subcategoryId, @categoryId, N'Audífonos', N'audifonos', 1, 1, @now, @now);

IF NOT EXISTS (SELECT 1 FROM products WHERE Id = @productId)
    INSERT INTO products (Id, SubcategoryId, Name, Slug, Description, BasePrice, IsActive, CreatedAt, UpdatedAt)
    VALUES (
        @productId, @subcategoryId,
        N'Audífonos Pro X', N'audifonos-pro-x',
        N'Audífonos inalámbricos con cancelación de ruido',
        199.99, 1, @now, @now
    );

IF NOT EXISTS (SELECT 1 FROM variants WHERE Id = @variantId)
    INSERT INTO variants (Id, ProductId, Sku, Price, IsActive, CreatedAt, UpdatedAt)
    VALUES (@variantId, @productId, N'APX-001', 199.99, 1, @now, @now);

IF NOT EXISTS (SELECT 1 FROM product_images WHERE ProductId = @productId)
    INSERT INTO product_images (Id, ProductId, Url, SortOrder, IsPrimary, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @productId, N'https://placehold.co/600x400', 1, 1, @now, @now);

IF NOT EXISTS (SELECT 1 FROM inventory WHERE VariantId = @variantId)
    INSERT INTO inventory (VariantId, QuantityOnHand, QuantityReserved)
    VALUES (@variantId, 50, 0);

IF NOT EXISTS (SELECT 1 FROM drivers WHERE Id = @driverId)
    INSERT INTO drivers (Id, Name, Phone, IsActive, CreatedAt, UpdatedAt)
    VALUES (@driverId, N'Juan Repartidor', N'+5215550001', 1, @now, @now);

GO

PRINT 'Seed completado.';
PRINT 'Admin:    admin@ecommerce.local / Admin123!';
PRINT 'Cliente:  cliente@ecommerce.local / Cliente123!';
PRINT 'Producto: slug audifonos-pro-x, SKU APX-001';
GO
