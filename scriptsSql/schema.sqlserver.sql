-- =============================================================================
-- ECOMMERCE — Esquema relacional normalizado
-- Motor: Microsoft SQL Server 2019+ / Azure SQL
-- Convención: snake_case, UNIQUEIDENTIFIER PKs, DATETIME2 UTC, BIT is_active
-- Uso: sqlcmd -S localhost -E -d ecommerce -i schema.sqlserver.sql
-- =============================================================================

USE master;
GO
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'ecommerce')
    CREATE DATABASE ecommerce;
GO
USE ecommerce;
GO

-- ===========================================================================
-- AUTENTICACIÓN Y AUTORIZACIÓN
-- ===========================================================================

CREATE TABLE users (
    id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_users_id DEFAULT NEWID(),
    email               NVARCHAR(255)    NOT NULL,
    email_confirmed     BIT              NOT NULL CONSTRAINT DF_users_email_confirmed DEFAULT 0,
    password_hash       NVARCHAR(512)    NOT NULL,
    first_name          NVARCHAR(100)    NOT NULL,
    last_name           NVARCHAR(100)    NOT NULL,
    phone               NVARCHAR(20)     NULL,
    document_type       NVARCHAR(20)     NULL,
    document_number     NVARCHAR(50)     NULL,
    profile_photo_url   NVARCHAR(500)    NULL,
    is_active           BIT              NOT NULL CONSTRAINT DF_users_is_active DEFAULT 1,
    lockout_end         DATETIME2        NULL,
    access_failed_count INT              NOT NULL CONSTRAINT DF_users_failed DEFAULT 0,
    two_factor_enabled  BIT              NOT NULL CONSTRAINT DF_users_2fa DEFAULT 0,
    created_at          DATETIME2        NOT NULL CONSTRAINT DF_users_created DEFAULT SYSUTCDATETIME(),
    updated_at          DATETIME2        NOT NULL CONSTRAINT DF_users_updated DEFAULT SYSUTCDATETIME(),
    deleted_at          DATETIME2        NULL,
    CONSTRAINT PK_users PRIMARY KEY (id),
    CONSTRAINT UQ_users_email UNIQUE (email)
);

CREATE TABLE roles (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_roles_id DEFAULT NEWID(),
    name        NVARCHAR(50)     NOT NULL,
    description NVARCHAR(255)    NULL,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_roles_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_roles PRIMARY KEY (id),
    CONSTRAINT UQ_roles_name UNIQUE (name)
);

CREATE TABLE permissions (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_permissions_id DEFAULT NEWID(),
    name        NVARCHAR(100)    NOT NULL,
    description NVARCHAR(255)    NULL,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_permissions_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_permissions PRIMARY KEY (id),
    CONSTRAINT UQ_permissions_name UNIQUE (name)
);

CREATE TABLE user_roles (
    user_id UNIQUEIDENTIFIER NOT NULL,
    role_id UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_user_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT FK_user_roles_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT FK_user_roles_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
);

CREATE TABLE role_permissions (
    role_id       UNIQUEIDENTIFIER NOT NULL,
    permission_id UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_role_permissions PRIMARY KEY (role_id, permission_id),
    CONSTRAINT FK_role_permissions_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT FK_role_permissions_perm FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE
);

CREATE TABLE refresh_tokens (
    id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_refresh_tokens_id DEFAULT NEWID(),
    user_id       UNIQUEIDENTIFIER NOT NULL,
    token_hash    NVARCHAR(128)    NOT NULL,
    expires_at    DATETIME2        NOT NULL,
    revoked_at    DATETIME2        NULL,
    created_at    DATETIME2        NOT NULL CONSTRAINT DF_refresh_tokens_created DEFAULT SYSUTCDATETIME(),
    created_by_ip NVARCHAR(45)     NULL,
    CONSTRAINT PK_refresh_tokens PRIMARY KEY (id),
    CONSTRAINT UQ_refresh_tokens_hash UNIQUE (token_hash),
    CONSTRAINT FK_refresh_tokens_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
CREATE INDEX IX_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX IX_refresh_tokens_expires ON refresh_tokens(expires_at) WHERE revoked_at IS NULL;

-- ===========================================================================
-- CATÁLOGO
-- ===========================================================================

CREATE TABLE families (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_families_id DEFAULT NEWID(),
    name        NVARCHAR(150)    NOT NULL,
    slug        NVARCHAR(180)    NOT NULL,
    sort_order  INT              NOT NULL CONSTRAINT DF_families_sort DEFAULT 0,
    is_active   BIT              NOT NULL CONSTRAINT DF_families_active DEFAULT 1,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_families_created DEFAULT SYSUTCDATETIME(),
    updated_at  DATETIME2        NOT NULL CONSTRAINT DF_families_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_families PRIMARY KEY (id),
    CONSTRAINT UQ_families_slug UNIQUE (slug)
);

CREATE TABLE categories (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_categories_id DEFAULT NEWID(),
    family_id   UNIQUEIDENTIFIER NOT NULL,
    name        NVARCHAR(150)    NOT NULL,
    slug        NVARCHAR(180)    NOT NULL,
    sort_order  INT              NOT NULL CONSTRAINT DF_categories_sort DEFAULT 0,
    is_active   BIT              NOT NULL CONSTRAINT DF_categories_active DEFAULT 1,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_categories_created DEFAULT SYSUTCDATETIME(),
    updated_at  DATETIME2        NOT NULL CONSTRAINT DF_categories_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_categories PRIMARY KEY (id),
    CONSTRAINT UQ_categories_family_slug UNIQUE (family_id, slug),
    CONSTRAINT FK_categories_family FOREIGN KEY (family_id) REFERENCES families(id)
);
CREATE INDEX IX_categories_family ON categories(family_id, is_active, sort_order);

CREATE TABLE subcategories (
    id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_subcategories_id DEFAULT NEWID(),
    category_id  UNIQUEIDENTIFIER NOT NULL,
    name         NVARCHAR(150)    NOT NULL,
    slug         NVARCHAR(180)    NOT NULL,
    sort_order   INT              NOT NULL CONSTRAINT DF_subcategories_sort DEFAULT 0,
    is_active    BIT              NOT NULL CONSTRAINT DF_subcategories_active DEFAULT 1,
    created_at   DATETIME2        NOT NULL CONSTRAINT DF_subcategories_created DEFAULT SYSUTCDATETIME(),
    updated_at   DATETIME2        NOT NULL CONSTRAINT DF_subcategories_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_subcategories PRIMARY KEY (id),
    CONSTRAINT UQ_subcategories_category_slug UNIQUE (category_id, slug),
    CONSTRAINT FK_subcategories_category FOREIGN KEY (category_id) REFERENCES categories(id)
);
CREATE INDEX IX_subcategories_category ON subcategories(category_id, is_active, sort_order);

CREATE TABLE products (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_products_id DEFAULT NEWID(),
    subcategory_id  UNIQUEIDENTIFIER NOT NULL,
    sku             NVARCHAR(80)     NOT NULL,
    name            NVARCHAR(255)    NOT NULL,
    slug            NVARCHAR(280)    NOT NULL,
    description     NVARCHAR(MAX)    NULL,
    base_price      DECIMAL(12, 2)   NOT NULL,
    is_active       BIT              NOT NULL CONSTRAINT DF_products_active DEFAULT 1,
    created_at      DATETIME2        NOT NULL CONSTRAINT DF_products_created DEFAULT SYSUTCDATETIME(),
    updated_at      DATETIME2        NOT NULL CONSTRAINT DF_products_updated DEFAULT SYSUTCDATETIME(),
    deleted_at      DATETIME2        NULL,
    CONSTRAINT PK_products PRIMARY KEY (id),
    CONSTRAINT UQ_products_sku UNIQUE (sku),
    CONSTRAINT UQ_products_slug UNIQUE (slug),
    CONSTRAINT FK_products_subcategory FOREIGN KEY (subcategory_id) REFERENCES subcategories(id),
    CONSTRAINT CK_products_base_price CHECK (base_price >= 0)
);
CREATE INDEX IX_products_subcategory_active ON products(subcategory_id, is_active);

CREATE TABLE product_images (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_product_images_id DEFAULT NEWID(),
    product_id  UNIQUEIDENTIFIER NOT NULL,
    url         NVARCHAR(500)    NOT NULL,
    alt_text    NVARCHAR(255)    NULL,
    sort_order  INT              NOT NULL CONSTRAINT DF_product_images_sort DEFAULT 0,
    is_primary  BIT              NOT NULL CONSTRAINT DF_product_images_primary DEFAULT 0,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_product_images_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_product_images PRIMARY KEY (id),
    CONSTRAINT FK_product_images_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE product_options (
    id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_product_options_id DEFAULT NEWID(),
    name          NVARCHAR(100)    NOT NULL,
    display_type  NVARCHAR(20)     NOT NULL CONSTRAINT DF_product_options_display DEFAULT 'text',
    sort_order    INT              NOT NULL CONSTRAINT DF_product_options_sort DEFAULT 0,
    created_at    DATETIME2        NOT NULL CONSTRAINT DF_product_options_created DEFAULT SYSUTCDATETIME(),
    updated_at    DATETIME2        NOT NULL CONSTRAINT DF_product_options_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_product_options PRIMARY KEY (id),
    CONSTRAINT UQ_product_options_name UNIQUE (name),
    CONSTRAINT CK_product_options_display CHECK (display_type IN ('text','color_swatch','size','numeric'))
);

CREATE TABLE option_values (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_option_values_id DEFAULT NEWID(),
    option_id   UNIQUEIDENTIFIER NOT NULL,
    value       NVARCHAR(100)    NOT NULL,
    label       NVARCHAR(150)    NOT NULL,
    metadata    NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_option_values_metadata DEFAULT '{}',
    sort_order  INT              NOT NULL CONSTRAINT DF_option_values_sort DEFAULT 0,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_option_values_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_option_values PRIMARY KEY (id),
    CONSTRAINT UQ_option_values_option_value UNIQUE (option_id, value),
    CONSTRAINT FK_option_values_option FOREIGN KEY (option_id) REFERENCES product_options(id) ON DELETE CASCADE,
    CONSTRAINT CK_option_values_metadata CHECK (ISJSON(metadata) = 1)
);

CREATE TABLE product_option_assignments (
    product_id UNIQUEIDENTIFIER NOT NULL,
    option_id  UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_product_option_assignments PRIMARY KEY (product_id, option_id),
    CONSTRAINT FK_poa_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT FK_poa_option FOREIGN KEY (option_id) REFERENCES product_options(id)
);

CREATE TABLE product_option_value_assignments (
    product_id      UNIQUEIDENTIFIER NOT NULL,
    option_value_id UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_product_option_value_assignments PRIMARY KEY (product_id, option_value_id),
    CONSTRAINT FK_pova_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT FK_pova_value FOREIGN KEY (option_value_id) REFERENCES option_values(id)
);

CREATE TABLE variants (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_variants_id DEFAULT NEWID(),
    product_id  UNIQUEIDENTIFIER NOT NULL,
    sku         NVARCHAR(80)     NOT NULL,
    price       DECIMAL(12, 2)   NULL,
    is_active   BIT              NOT NULL CONSTRAINT DF_variants_active DEFAULT 1,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_variants_created DEFAULT SYSUTCDATETIME(),
    updated_at  DATETIME2        NOT NULL CONSTRAINT DF_variants_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_variants PRIMARY KEY (id),
    CONSTRAINT UQ_variants_sku UNIQUE (sku),
    CONSTRAINT FK_variants_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT CK_variants_price CHECK (price IS NULL OR price >= 0)
);
CREATE INDEX IX_variants_product ON variants(product_id, is_active);

CREATE TABLE variant_option_values (
    variant_id      UNIQUEIDENTIFIER NOT NULL,
    option_value_id UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_variant_option_values PRIMARY KEY (variant_id, option_value_id),
    CONSTRAINT FK_vov_variant FOREIGN KEY (variant_id) REFERENCES variants(id) ON DELETE CASCADE,
    CONSTRAINT FK_vov_value FOREIGN KEY (option_value_id) REFERENCES option_values(id)
);

-- ===========================================================================
-- INVENTARIO
-- ===========================================================================

CREATE TABLE inventory (
    variant_id        UNIQUEIDENTIFIER NOT NULL,
    quantity_on_hand  INT              NOT NULL CONSTRAINT DF_inventory_on_hand DEFAULT 0,
    quantity_reserved INT              NOT NULL CONSTRAINT DF_inventory_reserved DEFAULT 0,
    updated_at        DATETIME2        NOT NULL CONSTRAINT DF_inventory_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_inventory PRIMARY KEY (variant_id),
    CONSTRAINT FK_inventory_variant FOREIGN KEY (variant_id) REFERENCES variants(id) ON DELETE CASCADE,
    CONSTRAINT CK_inventory_on_hand CHECK (quantity_on_hand >= 0),
    CONSTRAINT CK_inventory_reserved CHECK (quantity_reserved >= 0),
    CONSTRAINT CK_inventory_reserved_lte CHECK (quantity_reserved <= quantity_on_hand)
);

CREATE TABLE stock_movements (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_stock_movements_id DEFAULT NEWID(),
    variant_id      UNIQUEIDENTIFIER NOT NULL,
    user_id         UNIQUEIDENTIFIER NULL,
    movement_type   NVARCHAR(20)     NOT NULL,
    quantity        INT              NOT NULL,
    quantity_before INT              NOT NULL,
    quantity_after  INT              NOT NULL,
    reason          NVARCHAR(255)    NULL,
    reference_type  NVARCHAR(100)    NULL,
    reference_id    UNIQUEIDENTIFIER NULL,
    created_at      DATETIME2        NOT NULL CONSTRAINT DF_stock_movements_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_stock_movements PRIMARY KEY (id),
    CONSTRAINT FK_stock_movements_variant FOREIGN KEY (variant_id) REFERENCES variants(id),
    CONSTRAINT FK_stock_movements_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT CK_stock_movements_type CHECK (movement_type IN ('in','out','sale','adjustment','return')),
    CONSTRAINT CK_stock_movements_qty CHECK (quantity > 0)
);
CREATE INDEX IX_stock_movements_variant ON stock_movements(variant_id, created_at DESC);

CREATE TABLE covers (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_covers_id DEFAULT NEWID(),
    image_url   NVARCHAR(500)    NOT NULL,
    title       NVARCHAR(200)    NULL,
    subtitle    NVARCHAR(300)    NULL,
    link_url    NVARCHAR(500)    NULL,
    sort_order  INT              NOT NULL CONSTRAINT DF_covers_sort DEFAULT 0,
    starts_at   DATETIME2        NULL,
    ends_at     DATETIME2        NULL,
    is_active   BIT              NOT NULL CONSTRAINT DF_covers_active DEFAULT 1,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_covers_created DEFAULT SYSUTCDATETIME(),
    updated_at  DATETIME2        NOT NULL CONSTRAINT DF_covers_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_covers PRIMARY KEY (id)
);

CREATE TABLE addresses (
    id                    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_addresses_id DEFAULT NEWID(),
    user_id               UNIQUEIDENTIFIER NOT NULL,
    type                  NVARCHAR(20)     NOT NULL CONSTRAINT DF_addresses_type DEFAULT 'home',
    label                 NVARCHAR(80)     NULL,
    is_default            BIT              NOT NULL CONSTRAINT DF_addresses_default DEFAULT 0,
    contact_name          NVARCHAR(150)    NOT NULL,
    phone                 NVARCHAR(20)     NOT NULL,
    street                NVARCHAR(200)    NOT NULL,
    external_number       NVARCHAR(20)     NOT NULL,
    internal_number       NVARCHAR(20)     NULL,
    neighborhood          NVARCHAR(120)    NOT NULL,
    municipality          NVARCHAR(120)    NULL,
    city                  NVARCHAR(120)    NULL,
    state                 NVARCHAR(120)    NOT NULL,
    postal_code           NVARCHAR(10)     NOT NULL,
    country               CHAR(2)          NOT NULL CONSTRAINT DF_addresses_country DEFAULT 'MX',
    reference_notes       NVARCHAR(MAX)    NULL,
    delivery_instructions NVARCHAR(MAX)    NULL,
    latitude              DECIMAL(10, 7)   NULL,
    longitude             DECIMAL(10, 7)   NULL,
    created_at            DATETIME2        NOT NULL CONSTRAINT DF_addresses_created DEFAULT SYSUTCDATETIME(),
    updated_at            DATETIME2        NOT NULL CONSTRAINT DF_addresses_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_addresses PRIMARY KEY (id),
    CONSTRAINT FK_addresses_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT CK_addresses_type CHECK (type IN ('home','work','other','billing'))
);
CREATE INDEX IX_addresses_user ON addresses(user_id);

CREATE TABLE carts (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_carts_id DEFAULT NEWID(),
    user_id     UNIQUEIDENTIFIER NULL,
    guest_token UNIQUEIDENTIFIER NULL,
    expires_at  DATETIME2        NULL,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_carts_created DEFAULT SYSUTCDATETIME(),
    updated_at  DATETIME2        NOT NULL CONSTRAINT DF_carts_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_carts PRIMARY KEY (id),
    CONSTRAINT FK_carts_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT CK_cart_owner CHECK (user_id IS NOT NULL OR guest_token IS NOT NULL)
);
CREATE UNIQUE INDEX UQ_carts_user ON carts(user_id) WHERE user_id IS NOT NULL;
CREATE UNIQUE INDEX UQ_carts_guest ON carts(guest_token) WHERE guest_token IS NOT NULL;

CREATE TABLE cart_items (
    id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_cart_items_id DEFAULT NEWID(),
    cart_id             UNIQUEIDENTIFIER NOT NULL,
    variant_id          UNIQUEIDENTIFIER NOT NULL,
    quantity            INT              NOT NULL,
    unit_price_snapshot DECIMAL(12, 2)   NOT NULL,
    created_at          DATETIME2        NOT NULL CONSTRAINT DF_cart_items_created DEFAULT SYSUTCDATETIME(),
    updated_at          DATETIME2        NOT NULL CONSTRAINT DF_cart_items_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_cart_items PRIMARY KEY (id),
    CONSTRAINT UQ_cart_items_cart_variant UNIQUE (cart_id, variant_id),
    CONSTRAINT FK_cart_items_cart FOREIGN KEY (cart_id) REFERENCES carts(id) ON DELETE CASCADE,
    CONSTRAINT FK_cart_items_variant FOREIGN KEY (variant_id) REFERENCES variants(id),
    CONSTRAINT CK_cart_items_qty CHECK (quantity > 0)
);

CREATE TABLE order_number_seq (
    id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_order_number_seq PRIMARY KEY
);

CREATE TABLE orders (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_orders_id DEFAULT NEWID(),
    order_number    NVARCHAR(30)     NOT NULL,
    user_id         UNIQUEIDENTIFIER NOT NULL,
    status          NVARCHAR(30)     NOT NULL CONSTRAINT DF_orders_status DEFAULT 'pending_payment',
    currency        CHAR(3)          NOT NULL CONSTRAINT DF_orders_currency DEFAULT 'MXN',
    subtotal        DECIMAL(12, 2)   NOT NULL CONSTRAINT DF_orders_subtotal DEFAULT 0,
    tax_amount      DECIMAL(12, 2)   NOT NULL CONSTRAINT DF_orders_tax DEFAULT 0,
    shipping_amount DECIMAL(12, 2)   NOT NULL CONSTRAINT DF_orders_shipping DEFAULT 0,
    total           DECIMAL(12, 2)   NOT NULL CONSTRAINT DF_orders_total DEFAULT 0,
    items_count     INT              NOT NULL CONSTRAINT DF_orders_items DEFAULT 0,
    paid_at         DATETIME2        NULL,
    cancelled_at    DATETIME2        NULL,
    created_at      DATETIME2        NOT NULL CONSTRAINT DF_orders_created DEFAULT SYSUTCDATETIME(),
    updated_at      DATETIME2        NOT NULL CONSTRAINT DF_orders_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_orders PRIMARY KEY (id),
    CONSTRAINT UQ_orders_order_number UNIQUE (order_number),
    CONSTRAINT FK_orders_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT CK_orders_status CHECK (status IN (
        'pending_payment','payment_failed','paid','ready_to_dispatch','dispatched','delivered','cancelled'
    ))
);
CREATE INDEX IX_orders_user_status ON orders(user_id, status);

CREATE TABLE order_addresses (
    id                    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_order_addresses_id DEFAULT NEWID(),
    order_id              UNIQUEIDENTIFIER NOT NULL,
    type                  NVARCHAR(20)     NOT NULL CONSTRAINT DF_order_addresses_type DEFAULT 'shipping',
    contact_name          NVARCHAR(150)    NOT NULL,
    phone                 NVARCHAR(20)     NOT NULL,
    street                NVARCHAR(200)    NOT NULL,
    external_number       NVARCHAR(20)     NOT NULL,
    internal_number       NVARCHAR(20)     NULL,
    neighborhood          NVARCHAR(120)    NOT NULL,
    municipality          NVARCHAR(120)    NULL,
    city                  NVARCHAR(120)    NULL,
    state                 NVARCHAR(120)    NOT NULL,
    postal_code           NVARCHAR(10)     NOT NULL,
    country               CHAR(2)          NOT NULL CONSTRAINT DF_order_addresses_country DEFAULT 'MX',
    reference_notes       NVARCHAR(MAX)    NULL,
    delivery_instructions NVARCHAR(MAX)    NULL,
    latitude              DECIMAL(10, 7)   NULL,
    longitude             DECIMAL(10, 7)   NULL,
    created_at            DATETIME2        NOT NULL CONSTRAINT DF_order_addresses_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_order_addresses PRIMARY KEY (id),
    CONSTRAINT FK_order_addresses_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT CK_order_addresses_type CHECK (type IN ('shipping','billing'))
);

CREATE TABLE order_items (
    id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_order_items_id DEFAULT NEWID(),
    order_id         UNIQUEIDENTIFIER NOT NULL,
    variant_id       UNIQUEIDENTIFIER NULL,
    product_name     NVARCHAR(255)    NOT NULL,
    variant_sku      NVARCHAR(80)     NOT NULL,
    variant_label    NVARCHAR(500)    NULL,
    quantity         INT              NOT NULL,
    unit_price       DECIMAL(12, 2)   NOT NULL,
    line_total       DECIMAL(12, 2)   NOT NULL,
    variant_snapshot NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_order_items_snapshot DEFAULT '[]',
    created_at       DATETIME2        NOT NULL CONSTRAINT DF_order_items_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_order_items PRIMARY KEY (id),
    CONSTRAINT FK_order_items_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT FK_order_items_variant FOREIGN KEY (variant_id) REFERENCES variants(id) ON DELETE SET NULL,
    CONSTRAINT CK_order_items_snapshot CHECK (ISJSON(variant_snapshot) = 1)
);

CREATE TABLE stock_reservations (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_stock_reservations_id DEFAULT NEWID(),
    order_id    UNIQUEIDENTIFIER NOT NULL,
    variant_id  UNIQUEIDENTIFIER NOT NULL,
    quantity    INT              NOT NULL,
    expires_at  DATETIME2        NOT NULL,
    created_at  DATETIME2        NOT NULL CONSTRAINT DF_stock_reservations_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_stock_reservations PRIMARY KEY (id),
    CONSTRAINT UQ_stock_reservations_order_variant UNIQUE (order_id, variant_id),
    CONSTRAINT FK_stock_reservations_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT FK_stock_reservations_variant FOREIGN KEY (variant_id) REFERENCES variants(id)
);

CREATE TABLE payments (
    id                UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_payments_id DEFAULT NEWID(),
    order_id          UNIQUEIDENTIFIER NOT NULL,
    user_id           UNIQUEIDENTIFIER NOT NULL,
    gateway           NVARCHAR(50)     NOT NULL,
    external_id       NVARCHAR(150)    NULL,
    status            NVARCHAR(20)     NOT NULL CONSTRAINT DF_payments_status DEFAULT 'pending',
    amount            DECIMAL(12, 2)   NOT NULL,
    currency          CHAR(3)          NOT NULL CONSTRAINT DF_payments_currency DEFAULT 'MXN',
    card_last_four    CHAR(4)          NULL,
    card_brand        NVARCHAR(30)     NULL,
    idempotency_key   NVARCHAR(100)    NULL,
    request_payload   NVARCHAR(MAX)    NULL,
    response_payload  NVARCHAR(MAX)    NULL,
    created_at        DATETIME2        NOT NULL CONSTRAINT DF_payments_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_payments PRIMARY KEY (id),
    CONSTRAINT UQ_payments_idempotency UNIQUE (idempotency_key),
    CONSTRAINT FK_payments_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT FK_payments_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT CK_payments_status CHECK (status IN ('pending','approved','declined','error','refunded'))
);

CREATE TABLE drivers (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_drivers_id DEFAULT NEWID(),
    name            NVARCHAR(150)    NOT NULL,
    phone           NVARCHAR(20)     NOT NULL,
    email           NVARCHAR(255)    NULL,
    license_number  NVARCHAR(50)     NULL,
    vehicle_type    NVARCHAR(50)     NULL,
    vehicle_plate   NVARCHAR(20)     NULL,
    is_active       BIT              NOT NULL CONSTRAINT DF_drivers_active DEFAULT 1,
    notes           NVARCHAR(MAX)    NULL,
    created_at      DATETIME2        NOT NULL CONSTRAINT DF_drivers_created DEFAULT SYSUTCDATETIME(),
    updated_at      DATETIME2        NOT NULL CONSTRAINT DF_drivers_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_drivers PRIMARY KEY (id)
);

CREATE TABLE shipments (
    id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_shipments_id DEFAULT NEWID(),
    order_id         UNIQUEIDENTIFIER NOT NULL,
    driver_id        UNIQUEIDENTIFIER NOT NULL,
    shipment_number  NVARCHAR(30)     NOT NULL,
    tracking_code    NVARCHAR(80)     NULL,
    status           NVARCHAR(20)     NOT NULL CONSTRAINT DF_shipments_status DEFAULT 'pending',
    dispatched_at    DATETIME2        NULL,
    in_transit_at    DATETIME2        NULL,
    delivered_at     DATETIME2        NULL,
    notes            NVARCHAR(MAX)    NULL,
    created_at       DATETIME2        NOT NULL CONSTRAINT DF_shipments_created DEFAULT SYSUTCDATETIME(),
    updated_at       DATETIME2        NOT NULL CONSTRAINT DF_shipments_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_shipments PRIMARY KEY (id),
    CONSTRAINT UQ_shipments_order UNIQUE (order_id),
    CONSTRAINT UQ_shipments_number UNIQUE (shipment_number),
    CONSTRAINT FK_shipments_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT FK_shipments_driver FOREIGN KEY (driver_id) REFERENCES drivers(id),
    CONSTRAINT CK_shipments_status CHECK (status IN ('pending','in_transit','delivered','cancelled'))
);

CREATE TABLE dispatch_tickets (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_dispatch_tickets_id DEFAULT NEWID(),
    order_id       UNIQUEIDENTIFIER NOT NULL,
    ticket_number  NVARCHAR(30)     NOT NULL,
    file_url       NVARCHAR(500)    NOT NULL,
    generated_at   DATETIME2        NOT NULL CONSTRAINT DF_dispatch_tickets_generated DEFAULT SYSUTCDATETIME(),
    metadata       NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_dispatch_tickets_metadata DEFAULT '{}',
    CONSTRAINT PK_dispatch_tickets PRIMARY KEY (id),
    CONSTRAINT UQ_dispatch_tickets_order UNIQUE (order_id),
    CONSTRAINT UQ_dispatch_tickets_number UNIQUE (ticket_number),
    CONSTRAINT FK_dispatch_tickets_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT CK_dispatch_tickets_metadata CHECK (ISJSON(metadata) = 1)
);

GO

CREATE OR ALTER VIEW v_variant_availability AS
SELECT
    v.id AS variant_id,
    v.product_id,
    v.sku,
    ISNULL(i.quantity_on_hand, 0) AS quantity_on_hand,
    ISNULL(i.quantity_reserved, 0) AS quantity_reserved,
    ISNULL(i.quantity_on_hand, 0) - ISNULL(i.quantity_reserved, 0) AS quantity_available,
    ISNULL(v.price, p.base_price) AS effective_price
FROM variants v
INNER JOIN products p ON p.id = v.product_id
LEFT JOIN inventory i ON i.variant_id = v.id
WHERE v.is_active = 1 AND p.is_active = 1 AND p.deleted_at IS NULL;
GO
