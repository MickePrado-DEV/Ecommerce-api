-- ECOMMERCE API — Esquema completo (MySQL 8+)
-- mysql -u root -p < schema.mysql.sql

DROP DATABASE IF EXISTS ecommerce;
CREATE DATABASE ecommerce CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE ecommerce;
SET FOREIGN_KEY_CHECKS = 0;

CREATE TABLE users (
    Id                     CHAR(36) NOT NULL,
    Email                  VARCHAR(450)    NOT NULL,
    PasswordHash           TEXT    NOT NULL,
    TemporaryPasswordPlain TEXT    NULL,
    MustChangePassword     TINYINT(1)              NOT NULL DEFAULT 0,
    FirstName              TEXT    NOT NULL,
    LastName               TEXT    NOT NULL,
    Phone                  TEXT    NULL,
    IsActive               TINYINT(1)              NOT NULL,
    CreatedAt              DATETIME(6)        NOT NULL,
    UpdatedAt              DATETIME(6)        NOT NULL,
    CONSTRAINT PK_users PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_users_Email ON users(Email);

CREATE TABLE roles (
    Id        CHAR(36) NOT NULL,
    Name      TEXT    NOT NULL,
    Code      TEXT    NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_roles PRIMARY KEY (Id)
);

CREATE TABLE permissions (
    Id        CHAR(36) NOT NULL,
    Name      VARCHAR(450)    NOT NULL,
    Code      VARCHAR(450)    NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_permissions PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_permissions_Code ON permissions(Code);

CREATE TABLE user_roles (
    UserId CHAR(36) NOT NULL,
    RoleId CHAR(36) NOT NULL,
    CONSTRAINT PK_user_roles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_user_roles_users FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_user_roles_roles FOREIGN KEY (RoleId) REFERENCES roles(Id) ON DELETE CASCADE
);

CREATE TABLE role_permissions (
    RoleId       CHAR(36) NOT NULL,
    PermissionId CHAR(36) NOT NULL,
    CONSTRAINT PK_role_permissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_role_permissions_roles FOREIGN KEY (RoleId) REFERENCES roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_role_permissions_permissions FOREIGN KEY (PermissionId) REFERENCES permissions(Id) ON DELETE CASCADE
);

CREATE TABLE refresh_tokens (
    Id        CHAR(36) NOT NULL,
    UserId    CHAR(36) NOT NULL,
    TokenHash TEXT    NOT NULL,
    ExpiresAt DATETIME(6)        NOT NULL,
    RevokedAt DATETIME(6)        NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_refresh_tokens PRIMARY KEY (Id),
    CONSTRAINT FK_refresh_tokens_users FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
);
;

 -- CatÃ¡logo: familias â†’ variantes, opciones globales, reviews, cupones, portadas
CREATE TABLE families (
    Id        CHAR(36) NOT NULL,
    Name      TEXT    NOT NULL,
    Slug      VARCHAR(450)    NOT NULL,
    SortOrder INT              NOT NULL,
    IsActive  TINYINT(1)              NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_families PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_families_Slug ON families(Slug);

CREATE TABLE categories (
    Id        CHAR(36) NOT NULL,
    FamilyId  CHAR(36) NOT NULL,
    Name      TEXT    NOT NULL,
    Slug      TEXT    NOT NULL,
    SortOrder INT              NOT NULL,
    IsActive  TINYINT(1)              NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_categories PRIMARY KEY (Id),
    CONSTRAINT FK_categories_families FOREIGN KEY (FamilyId) REFERENCES families(Id) ON DELETE CASCADE
);

CREATE TABLE subcategories (
    Id         CHAR(36) NOT NULL,
    CategoryId CHAR(36) NOT NULL,
    Name       TEXT    NOT NULL,
    Slug       TEXT    NOT NULL,
    SortOrder  INT              NOT NULL,
    IsActive   TINYINT(1)              NOT NULL,
    CreatedAt  DATETIME(6)        NOT NULL,
    UpdatedAt  DATETIME(6)        NOT NULL,
    CONSTRAINT PK_subcategories PRIMARY KEY (Id),
    CONSTRAINT FK_subcategories_categories FOREIGN KEY (CategoryId) REFERENCES categories(Id) ON DELETE CASCADE
);

CREATE TABLE products (
    Id            CHAR(36) NOT NULL,
    SubcategoryId CHAR(36) NOT NULL,
    Name          TEXT    NOT NULL,
    Slug          VARCHAR(450)    NOT NULL,
    Description   TEXT    NULL,
    BasePrice     DECIMAL(18, 2)   NOT NULL,
    IsActive      TINYINT(1)              NOT NULL,
    CreatedAt     DATETIME(6)        NOT NULL,
    UpdatedAt     DATETIME(6)        NOT NULL,
    CONSTRAINT PK_products PRIMARY KEY (Id),
    CONSTRAINT FK_products_subcategories FOREIGN KEY (SubcategoryId) REFERENCES subcategories(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_products_Slug ON products(Slug);

CREATE TABLE product_images (
    Id        CHAR(36) NOT NULL,
    ProductId CHAR(36) NOT NULL,
    Url       TEXT    NOT NULL,
    SortOrder INT              NOT NULL,
    IsPrimary TINYINT(1)              NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_product_images PRIMARY KEY (Id),
    CONSTRAINT FK_product_images_products FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE
);

CREATE TABLE product_options (
    Id         CHAR(36) NOT NULL,
    Name       TEXT    NOT NULL,
    OptionType INT              NOT NULL DEFAULT 1,
    SortOrder  INT              NOT NULL,
    CreatedAt  DATETIME(6)        NOT NULL,
    UpdatedAt  DATETIME(6)        NOT NULL,
    CONSTRAINT PK_product_options PRIMARY KEY (Id)
);

CREATE TABLE option_values (
    Id              CHAR(36) NOT NULL,
    ProductOptionId CHAR(36) NOT NULL,
    Value           TEXT    NOT NULL,
    Description     VARCHAR(200)    NULL,
    SortOrder       INT              NOT NULL,
    CreatedAt       DATETIME(6)        NOT NULL,
    UpdatedAt       DATETIME(6)        NOT NULL,
    CONSTRAINT PK_option_values PRIMARY KEY (Id),
    CONSTRAINT FK_option_values_product_options FOREIGN KEY (ProductOptionId) REFERENCES product_options(Id) ON DELETE CASCADE
);

CREATE TABLE product_option_assignments (
    ProductId       CHAR(36) NOT NULL,
    ProductOptionId CHAR(36) NOT NULL,
    FeaturesJson      TEXT    NOT NULL DEFAULT '[]',
    CONSTRAINT PK_product_option_assignments PRIMARY KEY (ProductId, ProductOptionId),
    CONSTRAINT FK_product_option_assignments_products FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_product_option_assignments_product_options FOREIGN KEY (ProductOptionId) REFERENCES product_options(Id)
);

CREATE TABLE variants (
    Id        CHAR(36) NOT NULL,
    ProductId CHAR(36) NOT NULL,
    Sku       TEXT    NOT NULL,
    Price     DECIMAL(18, 2)   NULL,
    IsActive  TINYINT(1)              NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_variants PRIMARY KEY (Id),
    CONSTRAINT FK_variants_products FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE
);

CREATE TABLE variant_option_values (
    VariantId     CHAR(36) NOT NULL,
    OptionValueId CHAR(36) NOT NULL,
    CONSTRAINT PK_variant_option_values PRIMARY KEY (VariantId, OptionValueId),
    CONSTRAINT FK_variant_option_values_variants FOREIGN KEY (VariantId) REFERENCES variants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_variant_option_values_option_values FOREIGN KEY (OptionValueId) REFERENCES option_values(Id)
);

CREATE TABLE inventory (
    VariantId        CHAR(36) NOT NULL,
    QuantityOnHand   INT              NOT NULL,
    QuantityReserved INT              NOT NULL,
    CONSTRAINT PK_inventory PRIMARY KEY (VariantId),
    CONSTRAINT FK_inventory_variants FOREIGN KEY (VariantId) REFERENCES variants(Id) ON DELETE CASCADE
);

CREATE TABLE wishlist_items (
    Id        CHAR(36) NOT NULL,
    UserId    CHAR(36) NOT NULL,
    ProductId CHAR(36) NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_wishlist_items PRIMARY KEY (Id),
    CONSTRAINT FK_wishlist_items_users FOREIGN KEY (UserId) REFERENCES users(Id),
    CONSTRAINT FK_wishlist_items_products FOREIGN KEY (ProductId) REFERENCES products(Id)
);
CREATE UNIQUE INDEX IX_wishlist_items_UserId_ProductId ON wishlist_items(UserId, ProductId);

CREATE TABLE product_reviews (
    Id          CHAR(36) NOT NULL,
    ProductId   CHAR(36) NOT NULL,
    UserId      CHAR(36) NOT NULL,
    Rating      INT              NOT NULL,
    Title       TEXT    NULL,
    Comment     TEXT    NOT NULL,
    IsApproved  TINYINT(1)              NOT NULL,
    CreatedAt   DATETIME(6)        NOT NULL,
    UpdatedAt   DATETIME(6)        NOT NULL,
    CONSTRAINT PK_product_reviews PRIMARY KEY (Id),
    CONSTRAINT FK_product_reviews_products FOREIGN KEY (ProductId) REFERENCES products(Id),
    CONSTRAINT FK_product_reviews_users FOREIGN KEY (UserId) REFERENCES users(Id)
);

CREATE TABLE coupons (
    Id           CHAR(36) NOT NULL,
    Code         VARCHAR(450)    NOT NULL,
    DiscountType VARCHAR(20)     NOT NULL,
    Value        DECIMAL(18, 2)   NOT NULL,
    MinSubtotal  DECIMAL(18, 2) NULL,
    MaxUses      INT              NULL,
    UsedCount    INT              NOT NULL DEFAULT 0,
    ValidFrom    DATETIME(6)        NULL,
    ValidUntil   DATETIME(6)        NULL,
    IsActive     TINYINT(1)              NOT NULL,
    CreatedAt    DATETIME(6)        NOT NULL,
    UpdatedAt    DATETIME(6)        NOT NULL,
    CONSTRAINT PK_coupons PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_coupons_Code ON coupons(Code);

CREATE TABLE covers (
    Id        CHAR(36) NOT NULL,
    Title     TEXT    NOT NULL,
    ImageUrl  TEXT    NOT NULL,
    LinkUrl   TEXT    NULL,
    SortOrder INT              NOT NULL,
    IsActive  TINYINT(1)              NOT NULL,
    StartsAt  DATETIME(6)        NULL,
    EndsAt    DATETIME(6)        NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_covers PRIMARY KEY (Id)
);
;

 -- Comercio: direcciones, carrito, pedidos, pagos, stock
CREATE TABLE addresses (
    Id                   CHAR(36) NOT NULL,
    UserId               CHAR(36) NOT NULL,
    Type                 INT              NOT NULL DEFAULT 1,
    Label                VARCHAR(100)    NOT NULL,
    ContactName          VARCHAR(120)    NULL,
    Street               VARCHAR(250)    NOT NULL,
    ExternalNumber       VARCHAR(20)     NULL,
    InternalNumber       VARCHAR(20)     NULL,
    Neighborhood         VARCHAR(120)    NULL,
    Municipality         VARCHAR(120)    NULL,
    City                 VARCHAR(100)    NOT NULL,
    State                VARCHAR(100)    NOT NULL,
    PostalCode           VARCHAR(20)     NOT NULL,
    Country              VARCHAR(3)      NOT NULL,
    Phone                VARCHAR(30)     NOT NULL,
    `References`         VARCHAR(500)    NULL,
    DeliveryInstructions VARCHAR(500)    NULL,
    Latitude             DECIMAL(10,7)    NULL,
    Longitude            DECIMAL(10,7)    NULL,
    IsDefault            TINYINT(1)              NOT NULL DEFAULT 0,
    CreatedAt            DATETIME(6)        NOT NULL,
    UpdatedAt            DATETIME(6)        NOT NULL,
    CONSTRAINT PK_addresses PRIMARY KEY (Id),
    CONSTRAINT FK_addresses_users FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
);

CREATE TABLE carts (
    Id         CHAR(36) NOT NULL,
    UserId     CHAR(36) NULL,
    GuestToken CHAR(36) NULL,
    CreatedAt  DATETIME(6)        NOT NULL,
    UpdatedAt  DATETIME(6)        NOT NULL,
    CONSTRAINT PK_carts PRIMARY KEY (Id),
    CONSTRAINT FK_carts_users FOREIGN KEY (UserId) REFERENCES users(Id)
);

CREATE TABLE cart_items (
    Id        CHAR(36) NOT NULL,
    CartId    CHAR(36) NOT NULL,
    VariantId CHAR(36) NOT NULL,
    Quantity  INT              NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_cart_items PRIMARY KEY (Id),
    CONSTRAINT FK_cart_items_carts FOREIGN KEY (CartId) REFERENCES carts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_cart_items_variants FOREIGN KEY (VariantId) REFERENCES variants(Id)
);

CREATE TABLE orders (
    Id                  CHAR(36) NOT NULL,
    OrderNumber         TEXT    NOT NULL,
    UserId              CHAR(36) NOT NULL,
    Status              VARCHAR(30)     NOT NULL,
    DispatchStatus      VARCHAR(30)     NOT NULL DEFAULT 'Pending',
    ReadyAt             DATETIME(6)        NULL,
    BatchedAt           DATETIME(6)        NULL,
    RoutedAt            DATETIME(6)        NULL,
    AssignedAt          DATETIME(6)        NULL,
    DispatchDeliveredAt DATETIME(6)        NULL,
    Subtotal            DECIMAL(18, 2)   NOT NULL,
    DiscountAmount      DECIMAL(18, 2)   NOT NULL DEFAULT 0,
    CouponCode          VARCHAR(50)     NULL,
    ShippingCost        DECIMAL(18, 2)   NOT NULL,
    Total               DECIMAL(18, 2)   NOT NULL,
    CreatedAt           DATETIME(6)        NOT NULL,
    UpdatedAt           DATETIME(6)        NOT NULL,
    CONSTRAINT PK_orders PRIMARY KEY (Id)
);

CREATE TABLE order_items (
    Id          CHAR(36) NOT NULL,
    OrderId     CHAR(36) NOT NULL,
    VariantId   CHAR(36) NOT NULL,
    ProductName TEXT    NOT NULL,
    Sku         TEXT    NOT NULL,
    Quantity    INT              NOT NULL,
    UnitPrice   DECIMAL(18, 2)   NOT NULL,
    LineTotal   DECIMAL(18, 2)   NOT NULL,
    CreatedAt   DATETIME(6)        NOT NULL,
    UpdatedAt   DATETIME(6)        NOT NULL,
    CONSTRAINT PK_order_items PRIMARY KEY (Id),
    CONSTRAINT FK_order_items_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_order_items_variants FOREIGN KEY (VariantId) REFERENCES variants(Id)
);

CREATE TABLE order_addresses (
    Id          CHAR(36) NOT NULL,
    OrderId     CHAR(36) NOT NULL,
    FullName    TEXT    NOT NULL,
    Street      TEXT    NOT NULL,
    City        TEXT    NOT NULL,
    State       TEXT    NOT NULL,
    PostalCode  TEXT    NOT NULL,
    Country     TEXT    NOT NULL,
    Phone       TEXT    NOT NULL,
    Latitude    DECIMAL(10,7)    NULL,
    Longitude   DECIMAL(10,7)    NULL,
    AddressText TEXT    NULL,
    CreatedAt   DATETIME(6)        NOT NULL,
    UpdatedAt   DATETIME(6)        NOT NULL,
    CONSTRAINT PK_order_addresses PRIMARY KEY (Id),
    CONSTRAINT FK_order_addresses_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE
);

CREATE TABLE payments (
    Id                CHAR(36) NOT NULL,
    OrderId           CHAR(36) NOT NULL,
    Amount            DECIMAL(18, 2)   NOT NULL,
    Status            VARCHAR(30)     NOT NULL,
    CardHolderName    VARCHAR(120)    NULL,
    ProviderReference TEXT    NULL,
    PaidAt            DATETIME(6)        NULL,
    CreatedAt         DATETIME(6)        NOT NULL,
    UpdatedAt         DATETIME(6)        NOT NULL,
    CONSTRAINT PK_payments PRIMARY KEY (Id),
    CONSTRAINT FK_payments_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE
);

CREATE TABLE stock_reservations (
    Id        CHAR(36) NOT NULL,
    OrderId   CHAR(36) NOT NULL,
    VariantId CHAR(36) NOT NULL,
    Quantity  INT              NOT NULL,
    ExpiresAt DATETIME(6)        NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_stock_reservations PRIMARY KEY (Id),
    CONSTRAINT FK_stock_reservations_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_stock_reservations_variants FOREIGN KEY (VariantId) REFERENCES variants(Id)
);

CREATE TABLE stock_movements (
    Id        CHAR(36) NOT NULL,
    VariantId CHAR(36) NOT NULL,
    Type      VARCHAR(30)     NOT NULL,
    Quantity  INT              NOT NULL,
    Reference TEXT    NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_stock_movements PRIMARY KEY (Id),
    CONSTRAINT FK_stock_movements_variants FOREIGN KEY (VariantId) REFERENCES variants(Id)
);
;

 -- LogÃ­stica: conductores, envÃ­os, tickets
CREATE TABLE drivers (
    Id             CHAR(36) NOT NULL,
    UserId         CHAR(36) NULL,
    Name           TEXT    NOT NULL,
    Phone          TEXT    NOT NULL,
    Email          TEXT    NULL,
    LicenseNumber  TEXT    NULL,
    VehicleType    TEXT    NULL,
    VehiclePlate   TEXT    NULL,
    Notes          TEXT    NULL,
    IsActive       TINYINT(1)              NOT NULL,
    StartLatitude  DECIMAL(10,7)    NULL,
    StartLongitude DECIMAL(10,7)    NULL,
    Capacity       INT              NULL,
    CreatedAt      DATETIME(6)        NOT NULL,
    UpdatedAt      DATETIME(6)        NOT NULL,
    CONSTRAINT PK_drivers PRIMARY KEY (Id),
    CONSTRAINT FK_drivers_users FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE SET NULL
);

CREATE TABLE shipments (
    Id             CHAR(36) NOT NULL,
    OrderId        CHAR(36) NOT NULL,
    DriverId       CHAR(36) NULL,
    Status         VARCHAR(30)     NOT NULL,
    TrackingNumber TEXT    NULL,
    ShippedAt      DATETIME(6)        NULL,
    CreatedAt      DATETIME(6)        NOT NULL,
    UpdatedAt      DATETIME(6)        NOT NULL,
    CONSTRAINT PK_shipments PRIMARY KEY (Id),
    CONSTRAINT FK_shipments_orders FOREIGN KEY (OrderId) REFERENCES orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_shipments_drivers FOREIGN KEY (DriverId) REFERENCES drivers(Id)
);

CREATE TABLE dispatch_tickets (
    Id           CHAR(36) NOT NULL,
    ShipmentId   CHAR(36) NOT NULL,
    TicketNumber TEXT    NOT NULL,
    CreatedAt    DATETIME(6)        NOT NULL,
    UpdatedAt    DATETIME(6)        NOT NULL,
    CONSTRAINT PK_dispatch_tickets PRIMARY KEY (Id),
    CONSTRAINT FK_dispatch_tickets_shipments FOREIGN KEY (ShipmentId) REFERENCES shipments(Id) ON DELETE CASCADE
);
;

 -- Despacho por lotes y rutas
CREATE TABLE dispatch_settings (
    Id                       CHAR(36) NOT NULL,
    DefaultClusterRadiusKm   DECIMAL(8,3)     NOT NULL DEFAULT 2.5,
    DefaultMaxStopsPerRoute  INT              NOT NULL DEFAULT 20,
    DefaultMaxStopsPerBatch  INT              NOT NULL DEFAULT 20,
    DefaultRouteOriginType   VARCHAR(20)     NOT NULL DEFAULT 'Centroid',
    AllowOriginSelection     TINYINT(1)              NOT NULL DEFAULT 1,
    CreatedAt                DATETIME(6)        NOT NULL,
    UpdatedAt                DATETIME(6)        NOT NULL,
    CONSTRAINT PK_dispatch_settings PRIMARY KEY (Id)
);

CREATE TABLE dispatch_batches (
    Id        CHAR(36) NOT NULL,
    Code      VARCHAR(450)    NOT NULL,
    Status    VARCHAR(20)     NOT NULL,
    CenterLat DECIMAL(10,7)    NOT NULL,
    CenterLng DECIMAL(10,7)    NOT NULL,
    RadiusKm  DECIMAL(8,3)     NOT NULL,
    MaxStops  INT              NOT NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_dispatch_batches PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_dispatch_batches_Code ON dispatch_batches(Code);

CREATE TABLE dispatch_batch_orders (
    Id        CHAR(36) NOT NULL,
    BatchId   CHAR(36) NOT NULL,
    OrderId   CHAR(36) NOT NULL,
    DistanceKm DECIMAL(8,3)    NULL,
    CreatedAt DATETIME(6)        NOT NULL,
    UpdatedAt DATETIME(6)        NOT NULL,
    CONSTRAINT PK_dispatch_batch_orders PRIMARY KEY (Id),
    CONSTRAINT FK_dispatch_batch_orders_batches FOREIGN KEY (BatchId) REFERENCES dispatch_batches(Id),
    CONSTRAINT FK_dispatch_batch_orders_orders FOREIGN KEY (OrderId) REFERENCES orders(Id)
);
CREATE UNIQUE INDEX IX_dispatch_batch_orders_OrderId ON dispatch_batch_orders(OrderId);

CREATE TABLE delivery_routes (
    Id              CHAR(36) NOT NULL,
    Code            VARCHAR(450)    NOT NULL,
    Status          VARCHAR(20)     NOT NULL,
    DriverId        CHAR(36) NULL,
    BatchId         CHAR(36) NULL,
    OriginType      VARCHAR(20)     NOT NULL,
    OriginLat       DECIMAL(10,7)    NULL,
    OriginLng       DECIMAL(10,7)    NULL,
    TotalStops      INT              NOT NULL,
    TotalDistanceKm DECIMAL(10,3)    NULL,
    StartedAt       DATETIME(6)        NULL,
    FinishedAt      DATETIME(6)        NULL,
    CreatedAt       DATETIME(6)        NOT NULL,
    UpdatedAt       DATETIME(6)        NOT NULL,
    CONSTRAINT PK_delivery_routes PRIMARY KEY (Id),
    CONSTRAINT FK_delivery_routes_drivers FOREIGN KEY (DriverId) REFERENCES drivers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_delivery_routes_batches FOREIGN KEY (BatchId) REFERENCES dispatch_batches(Id) ON DELETE SET NULL
);
CREATE UNIQUE INDEX IX_delivery_routes_Code ON delivery_routes(Code);

CREATE TABLE delivery_route_stops (
    Id            CHAR(36) NOT NULL,
    RouteId       CHAR(36) NOT NULL,
    OrderId       CHAR(36) NOT NULL,
    StopIndex     INT              NOT NULL,
    Lat           DECIMAL(10,7)    NOT NULL,
    Lng           DECIMAL(10,7)    NOT NULL,
    AddressText   TEXT    NOT NULL,
    Status        VARCHAR(20)     NOT NULL,
    DeliveredAt   DATETIME(6)        NULL,
    FailedAt      DATETIME(6)        NULL,
    FailureReason TEXT    NULL,
    CreatedAt     DATETIME(6)        NOT NULL,
    UpdatedAt     DATETIME(6)        NOT NULL,
    CONSTRAINT PK_delivery_route_stops PRIMARY KEY (Id),
    CONSTRAINT FK_delivery_route_stops_routes FOREIGN KEY (RouteId) REFERENCES delivery_routes(Id),
    CONSTRAINT FK_delivery_route_stops_orders FOREIGN KEY (OrderId) REFERENCES orders(Id)
);
CREATE UNIQUE INDEX IX_delivery_route_stops_RouteId_OrderId ON delivery_route_stops(RouteId, OrderId);
CREATE UNIQUE INDEX IX_delivery_route_stops_RouteId_StopIndex ON delivery_route_stops(RouteId, StopIndex);
;

PRINT 'Esquema ecommerce creado correctamente.';;

SET FOREIGN_KEY_CHECKS = 1;
SELECT 'Esquema ecommerce creado correctamente.' AS message;
