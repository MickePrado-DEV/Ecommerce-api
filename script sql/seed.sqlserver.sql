-- =============================================================================
-- ECOMMERCE — Datos iniciales (SQL Server)
-- Ejecutar DESPUÉS de schema.sqlserver.sql
-- =============================================================================

USE ecommerce;
GO

-- Permisos
MERGE permissions AS target
USING (VALUES
    ('admin.dashboard.view',    N'Ver dashboard administrativo'),
    ('admin.covers.view',       N'Ver portadas'),
    ('admin.covers.manage',     N'Gestionar portadas'),
    ('admin.families.view',     N'Ver familias'),
    ('admin.families.manage',   N'Gestionar familias'),
    ('admin.categories.view',   N'Ver categorías'),
    ('admin.categories.manage', N'Gestionar categorías'),
    ('admin.subcategories.view',    N'Ver subcategorías'),
    ('admin.subcategories.manage',  N'Gestionar subcategorías'),
    ('admin.products.view',     N'Ver productos'),
    ('admin.products.manage',   N'Gestionar productos'),
    ('admin.options.view',      N'Ver opciones de producto'),
    ('admin.options.manage',    N'Gestionar opciones de producto'),
    ('admin.stock.view',        N'Ver inventario'),
    ('admin.stock.manage',      N'Gestionar inventario'),
    ('admin.drivers.view',      N'Ver conductores'),
    ('admin.drivers.manage',    N'Gestionar conductores'),
    ('admin.orders.view',       N'Ver pedidos'),
    ('admin.orders.manage',     N'Gestionar pedidos'),
    ('admin.shipments.view',    N'Ver envíos'),
    ('admin.shipments.manage',  N'Gestionar envíos')
) AS source (name, description)
ON target.name = source.name
WHEN NOT MATCHED THEN
    INSERT (id, name, description) VALUES (NEWID(), source.name, source.description);
GO

-- Roles
MERGE roles AS target
USING (VALUES
    ('super-admin',       N'Acceso total al sistema'),
    ('administrator',     N'Administrador general'),
    ('catalog-manager',   N'Gestión de catálogo y portadas'),
    ('inventory-manager', N'Gestión de inventario y productos'),
    ('logistics-manager', N'Gestión de pedidos y envíos'),
    ('customer',          N'Cliente de tienda')
) AS source (name, description)
ON target.name = source.name
WHEN NOT MATCHED THEN
    INSERT (id, name, description) VALUES (NEWID(), source.name, source.description);
GO

-- super-admin y administrator → todos los permisos
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name IN ('super-admin', 'administrator')
  AND NOT EXISTS (
      SELECT 1 FROM role_permissions rp
      WHERE rp.role_id = r.id AND rp.permission_id = p.id
  );
GO

-- catalog-manager
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r, permissions p
WHERE r.name = 'catalog-manager'
  AND p.name IN (
    'admin.dashboard.view','admin.covers.view','admin.covers.manage',
    'admin.families.view','admin.families.manage','admin.categories.view','admin.categories.manage',
    'admin.subcategories.view','admin.subcategories.manage','admin.products.view','admin.products.manage'
  )
  AND NOT EXISTS (SELECT 1 FROM role_permissions rp WHERE rp.role_id = r.id AND rp.permission_id = p.id);
GO

-- inventory-manager
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r, permissions p
WHERE r.name = 'inventory-manager'
  AND p.name IN (
    'admin.dashboard.view','admin.options.view','admin.options.manage',
    'admin.products.view','admin.products.manage','admin.stock.view','admin.stock.manage'
  )
  AND NOT EXISTS (SELECT 1 FROM role_permissions rp WHERE rp.role_id = r.id AND rp.permission_id = p.id);
GO

-- logistics-manager
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r, permissions p
WHERE r.name = 'logistics-manager'
  AND p.name IN (
    'admin.dashboard.view','admin.drivers.view','admin.drivers.manage',
    'admin.orders.view','admin.orders.manage','admin.shipments.view','admin.shipments.manage'
  )
  AND NOT EXISTS (SELECT 1 FROM role_permissions rp WHERE rp.role_id = r.id AND rp.permission_id = p.id);
GO

-- Usuario admin (password: pragmant64.)
IF NOT EXISTS (SELECT 1 FROM users WHERE email = 'mickeprd@gmail.com')
BEGIN
    INSERT INTO users (
        id, email, email_confirmed, password_hash,
        first_name, last_name, phone, document_type, document_number, is_active
    ) VALUES (
        NEWID(), 'mickeprd@gmail.com', 1,
        '$2y$12$m8Kz/CLeZqKfVcqilValOegvgcjOzI6bRWn8cXUtQZtAhOCZWhXUK',
        N'Miguel Angel', N'Prado Garcia', '3755423112', 'INE', '234234234', 1
    );
END
GO

INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u, roles r
WHERE u.email = 'mickeprd@gmail.com' AND r.name = 'super-admin'
  AND NOT EXISTS (
      SELECT 1 FROM user_roles ur WHERE ur.user_id = u.id AND ur.role_id = r.id
  );
GO
