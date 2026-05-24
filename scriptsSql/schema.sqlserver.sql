-- =============================================================================
-- ECOMMERCE API — Esquema SQL Server alineado con Entity Framework Core
-- Tablas: snake_case | Columnas: PascalCase (convención EF)
-- Ejecutar antes que seed.sqlserver.sql
-- =============================================================================
-- sqlcmd -S "(localdb)\mssqllocaldb" -E -i schema.sqlserver.sql
-- sqlcmd -S localhost -E -i schema.sqlserver.sql
-- =============================================================================

USE master;
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'ecommerce')
BEGIN
    ALTER DATABASE ecommerce SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ecommerce;
END
GO

CREATE DATABASE ecommerce;
GO

USE ecommerce;
GO

-- ---------------------------------------------------------------------------
-- Auth
-- ---------------------------------------------------------------------------

CREATE TABLE users (
    Id           UNIQUEIDENTIFIER NOT NULL,
    Email        NVARCHAR(450)    NOT NULL,
    PasswordHash NVARCHAR(MAX)    NOT NULL,
    FirstName    NVARCHAR(MAX)    NOT NULL,
    LastName     NVARCHAR(MAX)    NOT NULL,
    Phone        NVARCHAR(MAX)    NULL,
    IsActive     BIT              NOT NULL,
    CreatedAt    DATETIME2        NOT NULL,
    UpdatedAt    DATETIME2        NOT NULL,
    CONSTRAINT PK_users PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_users_Email ON users(Email);

CREATE TABLE roles (
    Id        UNIQUEIDENTIFIER NOT NULL,
    Name      NVARCHAR(MAX)    NOT NULL,
    Code      NVARCHAR(MAX)    NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_roles PRIMARY KEY (Id)
);

CREATE TABLE permissions (
    Id        UNIQUEIDENTIFIER NOT NULL,
    Code      NVARCHAR(MAX)    NOT NULL,
    Name      NVARCHAR(MAX)    NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_permissions PRIMARY KEY (Id)
);

CREATE TABLE user_roles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_user_roles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_user_roles_users FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_user_roles_roles FOREIGN KEY (RoleId) REFERENCES roles(Id) ON DELETE CASCADE
);

CREATE TABLE role_permissions (
    RoleId       UNIQUEIDENTIFIER NOT NULL,
    PermissionId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_role_permissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_role_permissions_roles FOREIGN KEY (RoleId) REFERENCES roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_role_permissions_permissions FOREIGN KEY (PermissionId) REFERENCES permissions(Id) ON DELETE CASCADE
);

CREATE TABLE refresh_tokens (
    Id        UNIQUEIDENTIFIER NOT NULL,
    UserId    UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(MAX)    NOT NULL,
    ExpiresAt DATETIME2        NOT NULL,
    RevokedAt DATETIME2        NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_refresh_tokens PRIMARY KEY (Id),
    CONSTRAINT FK_refresh_tokens_users FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
);

-- ---------------------------------------------------------------------------
-- Catálogo
-- ---------------------------------------------------------------------------

CREATE TABLE families (
    Id        UNIQUEIDENTIFIER NOT NULL,
    Name      NVARCHAR(MAX)    NOT NULL,
    Slug      NVARCHAR(450)    NOT NULL,
    SortOrder INT              NOT NULL,
    IsActive  BIT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_families PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_families_Slug ON families(Slug);

CREATE TABLE categories (
    Id        UNIQUEIDENTIFIER NOT NULL,
    FamilyId  UNIQUEIDENTIFIER NOT NULL,
    Name      NVARCHAR(MAX)    NOT NULL,
    Slug      NVARCHAR(MAX)    NOT NULL,
    SortOrder INT              NOT NULL,
    IsActive  BIT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_categories PRIMARY KEY (Id),
    CONSTRAINT FK_categories_families FOREIGN KEY (FamilyId) REFERENCES families(Id) ON DELETE CASCADE
);

CREATE TABLE subcategories (
    Id         UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    Name       NVARCHAR(MAX)    NOT NULL,
    Slug       NVARCHAR(MAX)    NOT NULL,
    SortOrder  INT              NOT NULL,
    IsActive   BIT              NOT NULL,
    CreatedAt  DATETIME2        NOT NULL,
    UpdatedAt  DATETIME2        NOT NULL,
    CONSTRAINT PK_subcategories PRIMARY KEY (Id),
    CONSTRAINT FK_subcategories_categories FOREIGN KEY (CategoryId) REFERENCES categories(Id) ON DELETE CASCADE
);

CREATE TABLE products (
    Id            UNIQUEIDENTIFIER NOT NULL,
    SubcategoryId UNIQUEIDENTIFIER NOT NULL,
    Name          NVARCHAR(MAX)    NOT NULL,
    Slug          NVARCHAR(450)    NOT NULL,
    Description   NVARCHAR(MAX)    NULL,
    BasePrice     DECIMAL(18, 2)   NOT NULL,
    IsActive      BIT              NOT NULL,
    CreatedAt     DATETIME2        NOT NULL,
    UpdatedAt     DATETIME2        NOT NULL,
    CONSTRAINT PK_products PRIMARY KEY (Id),
    CONSTRAINT FK_products_subcategories FOREIGN KEY (SubcategoryId) REFERENCES subcategories(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_products_Slug ON products(Slug);

CREATE TABLE product_images (
    Id        UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Url       NVARCHAR(MAX)    NOT NULL,
    SortOrder INT              NOT NULL,
    IsPrimary BIT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_product_images PRIMARY KEY (Id),
    CONSTRAINT FK_product_images_products FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE
);

CREATE TABLE product_options (
    Id        UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Name      NVARCHAR(MAX)    NOT NULL,
    SortOrder INT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_product_options PRIMARY KEY (Id),
    CONSTRAINT FK_product_options_products FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE
);

CREATE TABLE option_values (
    Id              UNIQUEIDENTIFIER NOT NULL,
    ProductOptionId UNIQUEIDENTIFIER NOT NULL,
    Value           NVARCHAR(MAX)    NOT NULL,
    SortOrder       INT              NOT NULL,
    CreatedAt       DATETIME2        NOT NULL,
    UpdatedAt       DATETIME2        NOT NULL,
    CONSTRAINT PK_option_values PRIMARY KEY (Id),
    CONSTRAINT FK_option_values_product_options FOREIGN KEY (ProductOptionId) REFERENCES product_options(Id) ON DELETE CASCADE
);

CREATE TABLE variants (
    Id        UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Sku       NVARCHAR(MAX)    NOT NULL,
    Price     DECIMAL(18, 2)   NULL,
    IsActive  BIT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_variants PRIMARY KEY (Id),
    CONSTRAINT FK_variants_products FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE
);

CREATE TABLE inventory (
    VariantId         UNIQUEIDENTIFIER NOT NULL,
    QuantityOnHand    INT              NOT NULL,
    QuantityReserved  INT              NOT NULL,
    CONSTRAINT PK_inventory PRIMARY KEY (VariantId),
    CONSTRAINT FK_inventory_variants FOREIGN KEY (VariantId) REFERENCES variants(Id) ON DELETE CASCADE
);

CREATE TABLE covers (
    Id        UNIQUEIDENTIFIER NOT NULL,
    Title     NVARCHAR(MAX)    NOT NULL,
    ImageUrl  NVARCHAR(MAX)    NOT NULL,
    LinkUrl   NVARCHAR(MAX)    NULL,
    SortOrder INT              NOT NULL,
    IsActive  BIT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_covers PRIMARY KEY (Id)
);

-- ---------------------------------------------------------------------------
-- Carrito y direcciones
-- ---------------------------------------------------------------------------

CREATE TABLE addresses (
    Id         UNIQUEIDENTIFIER NOT NULL,
    UserId     UNIQUEIDENTIFIER NOT NULL,
    Label      NVARCHAR(MAX)    NOT NULL,
    Street     NVARCHAR(MAX)    NOT NULL,
    City       NVARCHAR(MAX)    NOT NULL,
    State      NVARCHAR(MAX)    NOT NULL,
    PostalCode NVARCHAR(MAX)    NOT NULL,
    Country    NVARCHAR(MAX)    NOT NULL,
    Phone      NVARCHAR(MAX)    NOT NULL,
    CreatedAt  DATETIME2        NOT NULL,
    UpdatedAt  DATETIME2        NOT NULL,
    CONSTRAINT PK_addresses PRIMARY KEY (Id),
    CONSTRAINT FK_addresses_users FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
);

CREATE TABLE carts (
    Id         UNIQUEIDENTIFIER NOT NULL,
    UserId     UNIQUEIDENTIFIER NULL,
    GuestToken UNIQUEIDENTIFIER NULL,
    CreatedAt  DATETIME2        NOT NULL,
    UpdatedAt  DATETIME2        NOT NULL,
    CONSTRAINT PK_carts PRIMARY KEY (Id),
    CONSTRAINT FK_carts_users FOREIGN KEY (UserId) REFERENCES users(Id)
);

CREATE TABLE cart_items (
    Id        UNIQUEIDENTIFIER NOT NULL,
    CartId    UNIQUEIDENTIFIER NOT NULL,
    VariantId UNIQUEIDENTIFIER NOT NULL,
    Quantity  INT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_cart_items PRIMARY KEY (Id),
    CONSTRAINT FK_cart_items_carts FOREIGN KEY (CartId) REFERENCES carts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_cart_items_variants FOREIGN KEY (VariantId) REFERENCES variants(Id)
);

-- ---------------------------------------------------------------------------
-- Pedidos e inventario
-- ---------------------------------------------------------------------------

CREATE TABLE orders (
    Id            UNIQUEIDENTIFIER NOT NULL,
    OrderNumber   NVARCHAR(MAX)    NOT NULL,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    Status        NVARCHAR(30)     NOT NULL,
    Subtotal      DECIMAL(18, 2)   NOT NULL,
    ShippingCost  DECIMAL(18, 2)   NOT NULL,
    Total         DECIMAL(18, 2)   NOT NULL,
    CreatedAt     DATETIME2        NOT NULL,
    UpdatedAt     DATETIME2        NOT NULL,
    CONSTRAINT PK_orders PRIMARY KEY (Id)
);

CREATE TABLE order_items (
    Id          UNIQUEIDENTIFIER NOT NULL,
    OrderId     UNIQUEIDENTIFIER NOT NULL,
    VariantId   UNIQUEIDENTIFIER NOT NULL,
    ProductName NVARCHAR(MAX)    NOT NULL,
    Sku         NVARCHAR(MAX)    NOT NULL,
    Quantity    INT              NOT NULL,
    UnitPrice   DECIMAL(18, 2)   NOT NULL,
    LineTotal   DECIMAL(18, 2)   NOT NULL,
    CreatedAt   DATETIME2        NOT NULL,
    UpdatedAt   DATETIME2        NOT NULL,
    CONSTRAINT PK_order_items PRIMARY KEY (Id),
    CONSTRAINT FK_order_items_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_order_items_variants FOREIGN KEY (VariantId) REFERENCES variants(Id)
);

CREATE TABLE order_addresses (
    Id         UNIQUEIDENTIFIER NOT NULL,
    OrderId    UNIQUEIDENTIFIER NOT NULL,
    FullName   NVARCHAR(MAX)    NOT NULL,
    Street     NVARCHAR(MAX)    NOT NULL,
    City       NVARCHAR(MAX)    NOT NULL,
    State      NVARCHAR(MAX)    NOT NULL,
    PostalCode NVARCHAR(MAX)    NOT NULL,
    Country    NVARCHAR(MAX)    NOT NULL,
    Phone      NVARCHAR(MAX)    NOT NULL,
    CreatedAt  DATETIME2        NOT NULL,
    UpdatedAt  DATETIME2        NOT NULL,
    CONSTRAINT PK_order_addresses PRIMARY KEY (Id),
    CONSTRAINT FK_order_addresses_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE
);

CREATE TABLE payments (
    Id                UNIQUEIDENTIFIER NOT NULL,
    OrderId           UNIQUEIDENTIFIER NOT NULL,
    Amount            DECIMAL(18, 2)   NOT NULL,
    Status            NVARCHAR(30)     NOT NULL,
    ProviderReference NVARCHAR(MAX)    NULL,
    PaidAt            DATETIME2        NULL,
    CreatedAt         DATETIME2        NOT NULL,
    UpdatedAt         DATETIME2        NOT NULL,
    CONSTRAINT PK_payments PRIMARY KEY (Id),
    CONSTRAINT FK_payments_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE
);

CREATE TABLE stock_reservations (
    Id        UNIQUEIDENTIFIER NOT NULL,
    OrderId   UNIQUEIDENTIFIER NOT NULL,
    VariantId UNIQUEIDENTIFIER NOT NULL,
    Quantity  INT              NOT NULL,
    ExpiresAt DATETIME2        NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_stock_reservations PRIMARY KEY (Id),
    CONSTRAINT FK_stock_reservations_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_stock_reservations_variants FOREIGN KEY (VariantId) REFERENCES variants(Id)
);

CREATE TABLE stock_movements (
    Id        UNIQUEIDENTIFIER NOT NULL,
    VariantId UNIQUEIDENTIFIER NOT NULL,
    Type      NVARCHAR(30)     NOT NULL,
    Quantity  INT              NOT NULL,
    Reference NVARCHAR(MAX)    NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_stock_movements PRIMARY KEY (Id),
    CONSTRAINT FK_stock_movements_variants FOREIGN KEY (VariantId) REFERENCES variants(Id)
);

-- ---------------------------------------------------------------------------
-- Logística
-- ---------------------------------------------------------------------------

CREATE TABLE drivers (
    Id        UNIQUEIDENTIFIER NOT NULL,
    Name      NVARCHAR(MAX)    NOT NULL,
    Phone     NVARCHAR(MAX)    NOT NULL,
    IsActive  BIT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_drivers PRIMARY KEY (Id)
);

CREATE TABLE shipments (
    Id             UNIQUEIDENTIFIER NOT NULL,
    OrderId        UNIQUEIDENTIFIER NOT NULL,
    DriverId       UNIQUEIDENTIFIER NULL,
    Status         NVARCHAR(30)     NOT NULL,
    TrackingNumber NVARCHAR(MAX)    NULL,
    ShippedAt      DATETIME2        NULL,
    CreatedAt      DATETIME2        NOT NULL,
    UpdatedAt      DATETIME2        NOT NULL,
    CONSTRAINT PK_shipments PRIMARY KEY (Id),
    CONSTRAINT FK_shipments_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_shipments_drivers FOREIGN KEY (DriverId) REFERENCES drivers(Id)
);

CREATE TABLE dispatch_tickets (
    Id           UNIQUEIDENTIFIER NOT NULL,
    ShipmentId   UNIQUEIDENTIFIER NOT NULL,
    TicketNumber NVARCHAR(MAX)    NOT NULL,
    CreatedAt    DATETIME2        NOT NULL,
    UpdatedAt    DATETIME2        NOT NULL,
    CONSTRAINT PK_dispatch_tickets PRIMARY KEY (Id),
    CONSTRAINT FK_dispatch_tickets_shipments FOREIGN KEY (ShipmentId) REFERENCES shipments(Id) ON DELETE CASCADE
);

GO

PRINT 'Esquema ecommerce creado correctamente.';
GO
