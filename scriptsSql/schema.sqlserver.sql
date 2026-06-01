-- =============================================================================
-- ECOMMERCE API — Esquema completo (SQL Server)
-- Recrea la base `ecommerce` y todas las tablas.
-- =============================================================================
-- sqlcmd -S "(localdb)\mssqllocaldb" -E -i schema.sqlserver.sql
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
-- Auth: users, roles, permissions, tokens
CREATE TABLE users (
    Id                     UNIQUEIDENTIFIER NOT NULL,
    Email                  NVARCHAR(450)    NOT NULL,
    PasswordHash           NVARCHAR(MAX)    NOT NULL,
    TemporaryPasswordPlain NVARCHAR(MAX)    NULL,
    MustChangePassword     BIT              NOT NULL CONSTRAINT DF_users_MustChangePassword DEFAULT 0,
    FirstName              NVARCHAR(MAX)    NOT NULL,
    LastName               NVARCHAR(MAX)    NOT NULL,
    Phone                  NVARCHAR(MAX)    NULL,
    IsActive               BIT              NOT NULL,
    CreatedAt              DATETIME2        NOT NULL,
    UpdatedAt              DATETIME2        NOT NULL,
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
    Name      NVARCHAR(450)    NOT NULL,
    Code      NVARCHAR(450)    NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_permissions PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_permissions_Code ON permissions(Code);

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

GO
 -- CatÃ¡logo: familias â†’ variantes, opciones globales, reviews, cupones, portadas
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
    Id         UNIQUEIDENTIFIER NOT NULL,
    Name       NVARCHAR(MAX)    NOT NULL,
    OptionType INT              NOT NULL CONSTRAINT DF_product_options_OptionType DEFAULT 1,
    SortOrder  INT              NOT NULL,
    CreatedAt  DATETIME2        NOT NULL,
    UpdatedAt  DATETIME2        NOT NULL,
    CONSTRAINT PK_product_options PRIMARY KEY (Id)
);

CREATE TABLE option_values (
    Id              UNIQUEIDENTIFIER NOT NULL,
    ProductOptionId UNIQUEIDENTIFIER NOT NULL,
    Value           NVARCHAR(MAX)    NOT NULL,
    Description     NVARCHAR(200)    NULL,
    SortOrder       INT              NOT NULL,
    CreatedAt       DATETIME2        NOT NULL,
    UpdatedAt       DATETIME2        NOT NULL,
    CONSTRAINT PK_option_values PRIMARY KEY (Id),
    CONSTRAINT FK_option_values_product_options FOREIGN KEY (ProductOptionId) REFERENCES product_options(Id) ON DELETE CASCADE
);

CREATE TABLE product_option_assignments (
    ProductId       UNIQUEIDENTIFIER NOT NULL,
    ProductOptionId UNIQUEIDENTIFIER NOT NULL,
    FeaturesJson      NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_product_option_assignments_FeaturesJson DEFAULT '[]',
    CONSTRAINT PK_product_option_assignments PRIMARY KEY (ProductId, ProductOptionId),
    CONSTRAINT FK_product_option_assignments_products FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_product_option_assignments_product_options FOREIGN KEY (ProductOptionId) REFERENCES product_options(Id)
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

CREATE TABLE variant_option_values (
    VariantId     UNIQUEIDENTIFIER NOT NULL,
    OptionValueId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_variant_option_values PRIMARY KEY (VariantId, OptionValueId),
    CONSTRAINT FK_variant_option_values_variants FOREIGN KEY (VariantId) REFERENCES variants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_variant_option_values_option_values FOREIGN KEY (OptionValueId) REFERENCES option_values(Id)
);

CREATE TABLE inventory (
    VariantId        UNIQUEIDENTIFIER NOT NULL,
    QuantityOnHand   INT              NOT NULL,
    QuantityReserved INT              NOT NULL,
    CONSTRAINT PK_inventory PRIMARY KEY (VariantId),
    CONSTRAINT FK_inventory_variants FOREIGN KEY (VariantId) REFERENCES variants(Id) ON DELETE CASCADE
);

CREATE TABLE wishlist_items (
    Id        UNIQUEIDENTIFIER NOT NULL,
    UserId    UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_wishlist_items PRIMARY KEY (Id),
    CONSTRAINT FK_wishlist_items_users FOREIGN KEY (UserId) REFERENCES users(Id),
    CONSTRAINT FK_wishlist_items_products FOREIGN KEY (ProductId) REFERENCES products(Id)
);
CREATE UNIQUE INDEX IX_wishlist_items_UserId_ProductId ON wishlist_items(UserId, ProductId);

CREATE TABLE product_reviews (
    Id          UNIQUEIDENTIFIER NOT NULL,
    ProductId   UNIQUEIDENTIFIER NOT NULL,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    Rating      INT              NOT NULL,
    Title       NVARCHAR(MAX)    NULL,
    Comment     NVARCHAR(MAX)    NOT NULL,
    IsApproved  BIT              NOT NULL,
    CreatedAt   DATETIME2        NOT NULL,
    UpdatedAt   DATETIME2        NOT NULL,
    CONSTRAINT PK_product_reviews PRIMARY KEY (Id),
    CONSTRAINT FK_product_reviews_products FOREIGN KEY (ProductId) REFERENCES products(Id),
    CONSTRAINT FK_product_reviews_users FOREIGN KEY (UserId) REFERENCES users(Id)
);

CREATE TABLE coupons (
    Id           UNIQUEIDENTIFIER NOT NULL,
    Code         NVARCHAR(450)    NOT NULL,
    DiscountType NVARCHAR(20)     NOT NULL,
    Value        DECIMAL(18, 2)   NOT NULL,
    MinSubtotal  DECIMAL(18, 2) NULL,
    MaxUses      INT              NULL,
    UsedCount    INT              NOT NULL CONSTRAINT DF_coupons_UsedCount DEFAULT 0,
    ValidFrom    DATETIME2        NULL,
    ValidUntil   DATETIME2        NULL,
    IsActive     BIT              NOT NULL,
    CreatedAt    DATETIME2        NOT NULL,
    UpdatedAt    DATETIME2        NOT NULL,
    CONSTRAINT PK_coupons PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_coupons_Code ON coupons(Code);

CREATE TABLE covers (
    Id        UNIQUEIDENTIFIER NOT NULL,
    Title     NVARCHAR(MAX)    NOT NULL,
    ImageUrl  NVARCHAR(MAX)    NOT NULL,
    LinkUrl   NVARCHAR(MAX)    NULL,
    SortOrder INT              NOT NULL,
    IsActive  BIT              NOT NULL,
    StartsAt  DATETIME2        NULL,
    EndsAt    DATETIME2        NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_covers PRIMARY KEY (Id)
);

GO
 -- Comercio: direcciones, carrito, pedidos, pagos, stock
CREATE TABLE addresses (
    Id                   UNIQUEIDENTIFIER NOT NULL,
    UserId               UNIQUEIDENTIFIER NOT NULL,
    Type                 INT              NOT NULL CONSTRAINT DF_addresses_Type DEFAULT 1,
    Label                NVARCHAR(100)    NOT NULL,
    ContactName          NVARCHAR(120)    NULL,
    Street               NVARCHAR(250)    NOT NULL,
    ExternalNumber       NVARCHAR(20)     NULL,
    InternalNumber       NVARCHAR(20)     NULL,
    Neighborhood         NVARCHAR(120)    NULL,
    Municipality         NVARCHAR(120)    NULL,
    City                 NVARCHAR(100)    NOT NULL,
    State                NVARCHAR(100)    NOT NULL,
    PostalCode           NVARCHAR(20)     NOT NULL,
    Country              NVARCHAR(3)      NOT NULL,
    Phone                NVARCHAR(30)     NOT NULL,
    [References]         NVARCHAR(500)    NULL,
    DeliveryInstructions NVARCHAR(500)    NULL,
    Latitude             DECIMAL(10,7)    NULL,
    Longitude            DECIMAL(10,7)    NULL,
    IsDefault            BIT              NOT NULL CONSTRAINT DF_addresses_IsDefault DEFAULT 0,
    CreatedAt            DATETIME2        NOT NULL,
    UpdatedAt            DATETIME2        NOT NULL,
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

CREATE TABLE orders (
    Id                  UNIQUEIDENTIFIER NOT NULL,
    OrderNumber         NVARCHAR(MAX)    NOT NULL,
    UserId              UNIQUEIDENTIFIER NOT NULL,
    Status              NVARCHAR(30)     NOT NULL,
    DispatchStatus      NVARCHAR(30)     NOT NULL CONSTRAINT DF_orders_DispatchStatus DEFAULT 'Pending',
    ReadyAt             DATETIME2        NULL,
    BatchedAt           DATETIME2        NULL,
    RoutedAt            DATETIME2        NULL,
    AssignedAt          DATETIME2        NULL,
    DispatchDeliveredAt DATETIME2        NULL,
    Subtotal            DECIMAL(18, 2)   NOT NULL,
    DiscountAmount      DECIMAL(18, 2)   NOT NULL CONSTRAINT DF_orders_DiscountAmount DEFAULT 0,
    CouponCode          NVARCHAR(50)     NULL,
    ShippingCost        DECIMAL(18, 2)   NOT NULL,
    Total               DECIMAL(18, 2)   NOT NULL,
    CreatedAt           DATETIME2        NOT NULL,
    UpdatedAt           DATETIME2        NOT NULL,
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
    Id          UNIQUEIDENTIFIER NOT NULL,
    OrderId     UNIQUEIDENTIFIER NOT NULL,
    FullName    NVARCHAR(MAX)    NOT NULL,
    Street      NVARCHAR(MAX)    NOT NULL,
    City        NVARCHAR(MAX)    NOT NULL,
    State       NVARCHAR(MAX)    NOT NULL,
    PostalCode  NVARCHAR(MAX)    NOT NULL,
    Country     NVARCHAR(MAX)    NOT NULL,
    Phone       NVARCHAR(MAX)    NOT NULL,
    Latitude    DECIMAL(10,7)    NULL,
    Longitude   DECIMAL(10,7)    NULL,
    AddressText NVARCHAR(MAX)    NULL,
    CreatedAt   DATETIME2        NOT NULL,
    UpdatedAt   DATETIME2        NOT NULL,
    CONSTRAINT PK_order_addresses PRIMARY KEY (Id),
    CONSTRAINT FK_order_addresses_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE
);

CREATE TABLE payments (
    Id                UNIQUEIDENTIFIER NOT NULL,
    OrderId           UNIQUEIDENTIFIER NOT NULL,
    Amount            DECIMAL(18, 2)   NOT NULL,
    Status            NVARCHAR(30)     NOT NULL,
    CardHolderName    NVARCHAR(120)    NULL,
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

GO
 -- LogÃ­stica: conductores, envÃ­os, tickets
CREATE TABLE drivers (
    Id             UNIQUEIDENTIFIER NOT NULL,
    UserId         UNIQUEIDENTIFIER NULL,
    Name           NVARCHAR(MAX)    NOT NULL,
    Phone          NVARCHAR(MAX)    NOT NULL,
    Email          NVARCHAR(MAX)    NULL,
    LicenseNumber  NVARCHAR(MAX)    NULL,
    VehicleType    NVARCHAR(MAX)    NULL,
    VehiclePlate   NVARCHAR(MAX)    NULL,
    Notes          NVARCHAR(MAX)    NULL,
    IsActive       BIT              NOT NULL,
    StartLatitude  DECIMAL(10,7)    NULL,
    StartLongitude DECIMAL(10,7)    NULL,
    Capacity       INT              NULL,
    CreatedAt      DATETIME2        NOT NULL,
    UpdatedAt      DATETIME2        NOT NULL,
    CONSTRAINT PK_drivers PRIMARY KEY (Id),
    CONSTRAINT FK_drivers_users FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE SET NULL
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
 -- Despacho por lotes y rutas
CREATE TABLE dispatch_settings (
    Id                       UNIQUEIDENTIFIER NOT NULL,
    DefaultClusterRadiusKm   DECIMAL(8,3)     NOT NULL CONSTRAINT DF_dispatch_settings_Radius DEFAULT 2.5,
    DefaultMaxStopsPerRoute  INT              NOT NULL CONSTRAINT DF_dispatch_settings_MaxRoute DEFAULT 20,
    DefaultMaxStopsPerBatch  INT              NOT NULL CONSTRAINT DF_dispatch_settings_MaxBatch DEFAULT 20,
    DefaultRouteOriginType   NVARCHAR(20)     NOT NULL CONSTRAINT DF_dispatch_settings_Origin DEFAULT 'Centroid',
    AllowOriginSelection     BIT              NOT NULL CONSTRAINT DF_dispatch_settings_AllowOrigin DEFAULT 1,
    CreatedAt                DATETIME2        NOT NULL,
    UpdatedAt                DATETIME2        NOT NULL,
    CONSTRAINT PK_dispatch_settings PRIMARY KEY (Id)
);

CREATE TABLE dispatch_batches (
    Id        UNIQUEIDENTIFIER NOT NULL,
    Code      NVARCHAR(450)    NOT NULL,
    Status    NVARCHAR(20)     NOT NULL,
    CenterLat DECIMAL(10,7)    NOT NULL,
    CenterLng DECIMAL(10,7)    NOT NULL,
    RadiusKm  DECIMAL(8,3)     NOT NULL,
    MaxStops  INT              NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_dispatch_batches PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_dispatch_batches_Code ON dispatch_batches(Code);

CREATE TABLE dispatch_batch_orders (
    Id        UNIQUEIDENTIFIER NOT NULL,
    BatchId   UNIQUEIDENTIFIER NOT NULL,
    OrderId   UNIQUEIDENTIFIER NOT NULL,
    DistanceKm DECIMAL(8,3)    NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,
    CONSTRAINT PK_dispatch_batch_orders PRIMARY KEY (Id),
    CONSTRAINT FK_dispatch_batch_orders_batches FOREIGN KEY (BatchId) REFERENCES dispatch_batches(Id),
    CONSTRAINT FK_dispatch_batch_orders_orders FOREIGN KEY (OrderId) REFERENCES orders(Id)
);
CREATE UNIQUE INDEX IX_dispatch_batch_orders_OrderId ON dispatch_batch_orders(OrderId);

CREATE TABLE delivery_routes (
    Id              UNIQUEIDENTIFIER NOT NULL,
    Code            NVARCHAR(450)    NOT NULL,
    Status          NVARCHAR(20)     NOT NULL,
    DriverId        UNIQUEIDENTIFIER NULL,
    BatchId         UNIQUEIDENTIFIER NULL,
    OriginType      NVARCHAR(20)     NOT NULL,
    OriginLat       DECIMAL(10,7)    NULL,
    OriginLng       DECIMAL(10,7)    NULL,
    TotalStops      INT              NOT NULL,
    TotalDistanceKm DECIMAL(10,3)    NULL,
    StartedAt       DATETIME2        NULL,
    FinishedAt      DATETIME2        NULL,
    CreatedAt       DATETIME2        NOT NULL,
    UpdatedAt       DATETIME2        NOT NULL,
    CONSTRAINT PK_delivery_routes PRIMARY KEY (Id),
    CONSTRAINT FK_delivery_routes_drivers FOREIGN KEY (DriverId) REFERENCES drivers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_delivery_routes_batches FOREIGN KEY (BatchId) REFERENCES dispatch_batches(Id) ON DELETE SET NULL
);
CREATE UNIQUE INDEX IX_delivery_routes_Code ON delivery_routes(Code);

CREATE TABLE delivery_route_stops (
    Id            UNIQUEIDENTIFIER NOT NULL,
    RouteId       UNIQUEIDENTIFIER NOT NULL,
    OrderId       UNIQUEIDENTIFIER NOT NULL,
    StopIndex     INT              NOT NULL,
    Lat           DECIMAL(10,7)    NOT NULL,
    Lng           DECIMAL(10,7)    NOT NULL,
    AddressText   NVARCHAR(MAX)    NOT NULL,
    Status        NVARCHAR(20)     NOT NULL,
    DeliveredAt   DATETIME2        NULL,
    FailedAt      DATETIME2        NULL,
    FailureReason NVARCHAR(MAX)    NULL,
    CreatedAt     DATETIME2        NOT NULL,
    UpdatedAt     DATETIME2        NOT NULL,
    CONSTRAINT PK_delivery_route_stops PRIMARY KEY (Id),
    CONSTRAINT FK_delivery_route_stops_routes FOREIGN KEY (RouteId) REFERENCES delivery_routes(Id),
    CONSTRAINT FK_delivery_route_stops_orders FOREIGN KEY (OrderId) REFERENCES orders(Id)
);
CREATE UNIQUE INDEX IX_delivery_route_stops_RouteId_OrderId ON delivery_route_stops(RouteId, OrderId);
CREATE UNIQUE INDEX IX_delivery_route_stops_RouteId_StopIndex ON delivery_route_stops(RouteId, StopIndex);

GO

PRINT 'Esquema ecommerce creado correctamente.';
GO

