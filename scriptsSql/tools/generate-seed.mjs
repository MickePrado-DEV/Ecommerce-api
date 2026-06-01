/**
 * Genera seed.sqlserver.sql, seed.mysql.sql, seed.mariadb.sql, seed.postgresql.sql
 * con mínimo BULK_COUNT registros por entidad principal.
 *
 * Uso: node scriptsSql/tools/generate-seed.mjs
 */
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.join(__dirname, '..');

const BULK_COUNT = 1000;

const HASH_ADMIN = '$2a$11$AiFO5/uMXLbByntOOjudIOYDU0uOGA56W0AXP/GZFK6NAaWagiZpG';
const HASH_CUSTOMER = '$2a$11$gfwpufX8e5IgdVd8RGcPWOfU6AEUs4iFSyeTjJCPd5CZZP15ibJOS';
const HASH_DRIVER = '$2a$11$7Xhavc/I5a8uxYxaBuS8EO7HJjd5uwGeyay3.CU0b1RQMv4QZW.s.';

const PERMISSIONS = [
  'admin.dashboard.view', 'admin.covers.view', 'admin.covers.manage',
  'admin.families.view', 'admin.families.manage', 'admin.categories.view', 'admin.categories.manage',
  'admin.subcategories.view', 'admin.subcategories.manage', 'admin.products.view', 'admin.products.manage',
  'admin.options.view', 'admin.options.manage', 'admin.stock.view', 'admin.stock.manage',
  'admin.drivers.view', 'admin.drivers.manage', 'admin.orders.view', 'admin.orders.manage',
  'admin.shipments.view', 'admin.shipments.manage', 'admin.dispatch.view', 'admin.dispatch.manage',
  'admin.users.view', 'admin.users.manage', 'admin.roles.view', 'admin.roles.manage',
];

const pad = (n, len = 12) => String(n).padStart(len, '0');
const guid = (prefix8, n) => `${prefix8}-${pad(n, 4)}-${pad(n, 4)}-${pad(n, 4)}-${pad(n, 12)}`;

function orderStatus(i) {
  const m = i % 10;
  if (m === 0) return { status: 'PendingPayment', dispatch: 'Pending', ship: null };
  if (m === 1) return { status: 'Paid', dispatch: 'Paid', ship: null };
  if (m === 2 || m === 3) return { status: 'ReadyToDispatch', dispatch: 'Ready', ship: null };
  if (m === 4) return { status: 'ReadyToDispatch', dispatch: 'Batched', ship: null };
  if (m === 5) return { status: 'ReadyToDispatch', dispatch: 'Routed', ship: null };
  if (m === 6 || m === 7) return { status: 'Dispatched', dispatch: 'Assigned', ship: i % 2 === 0 ? 'InTransit' : 'Pending' };
  return { status: 'Delivered', dispatch: 'Delivered', ship: 'Delivered' };
}

function generateSqlServer() {
  const N = BULK_COUNT;
  const lines = [];
  const w = (s) => lines.push(s);

  w(`-- ECOMMERCE API — Seed completo (SQL Server)`);
  w(`-- Volumen: ${N}+ por entidad principal | Ejecutar tras schema.sqlserver.sql`);
  w(`-- sqlcmd -S "(localdb)\\mssqllocaldb" -E -d ecommerce -i seed.sqlserver.sql`);
  w('');
  w('USE ecommerce;');
  w('GO');
  w('SET NOCOUNT ON;');
  w('DECLARE @now DATETIME2 = SYSUTCDATETIME();');
  w('DECLARE @N INT = ' + N + ';');
  w(`DECLARE @hashAdmin NVARCHAR(MAX) = N'${HASH_ADMIN}';`);
  w(`DECLARE @hashCustomer NVARCHAR(MAX) = N'${HASH_CUSTOMER}';`);
  w(`DECLARE @hashDriver NVARCHAR(MAX) = N'${HASH_DRIVER}';`);
  w(`DECLARE @roleAdmin UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111101';`);
  w(`DECLARE @roleCustomer UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111102';`);
  w(`DECLARE @roleDriver UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111103';`);
  w('');

  w('-- Permisos y roles');
  w(`INSERT INTO permissions (Id, Code, Name, CreatedAt, UpdatedAt)`);
  w(`SELECT NEWID(), v.Code, v.Code, @now, @now FROM (VALUES`);
  w(PERMISSIONS.map((p) => `    ('${p}')`).join(',\n'));
  w(`) AS v(Code) WHERE NOT EXISTS (SELECT 1 FROM permissions p WHERE p.Code = v.Code);`);
  w(`IF NOT EXISTS (SELECT 1 FROM roles WHERE Id='11111111-1111-1111-1111-111111111101') INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111101', N'Administrador', N'admin', @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM roles WHERE Id='11111111-1111-1111-1111-111111111102') INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111102', N'Cliente', N'customer', @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM roles WHERE Id='11111111-1111-1111-1111-111111111103') INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111103', N'Repartidor', N'driver', @now, @now);`);
  w(`INSERT INTO role_permissions (RoleId, PermissionId) SELECT @roleAdmin, p.Id FROM permissions p WHERE NOT EXISTS (SELECT 1 FROM role_permissions rp WHERE rp.RoleId=@roleAdmin AND rp.PermissionId=p.Id);`);
  w('');

  w('-- Usuarios fijos Postman');
  w(`IF NOT EXISTS (SELECT 1 FROM users WHERE Email=N'admin@ecommerce.local') INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222201', N'admin@ecommerce.local', @hashAdmin, NULL, 0, N'Admin', N'Sistema', NULL, 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM users WHERE Email=N'cliente@ecommerce.local') INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222202', N'cliente@ecommerce.local', @hashCustomer, NULL, 0, N'Cliente', N'Demo', N'+5215550100', 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM users WHERE Email=N'repartidor@ecommerce.local') INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222203', N'repartidor@ecommerce.local', @hashDriver, NULL, 0, N'Juan', N'Repartidor', N'+5215550001', 1, @now, @now);`);
  w(`INSERT INTO user_roles SELECT '22222222-2222-2222-2222-222222222201', @roleAdmin WHERE NOT EXISTS (SELECT 1 FROM user_roles WHERE UserId='22222222-2222-2222-2222-222222222201');`);
  w(`INSERT INTO user_roles SELECT '22222222-2222-2222-2222-222222222202', @roleCustomer WHERE NOT EXISTS (SELECT 1 FROM user_roles WHERE UserId='22222222-2222-2222-2222-222222222202');`);
  w(`INSERT INTO user_roles SELECT '22222222-2222-2222-2222-222222222203', @roleDriver WHERE NOT EXISTS (SELECT 1 FROM user_roles WHERE UserId='22222222-2222-2222-2222-222222222203');`);
  w('');

  w('-- Catálogo base + opciones globales');
  w(`IF NOT EXISTS (SELECT 1 FROM families WHERE Id='33333333-3333-3333-3333-333333333301') INSERT INTO families VALUES ('33333333-3333-3333-3333-333333333301', N'Electrónica', N'electronica', 1, 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM categories WHERE Id='33333333-3333-3333-3333-333333333302') INSERT INTO categories VALUES ('33333333-3333-3333-3333-333333333302', '33333333-3333-3333-3333-333333333301', N'Audio', N'audio', 1, 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM subcategories WHERE Id='33333333-3333-3333-3333-333333333303') INSERT INTO subcategories VALUES ('33333333-3333-3333-3333-333333333303', '33333333-3333-3333-3333-333333333302', N'Audífonos', N'audifonos', 1, 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM subcategories WHERE Id='33333333-3333-3333-3333-333333333304') INSERT INTO subcategories VALUES ('33333333-3333-3333-3333-333333333304', '33333333-3333-3333-3333-333333333302', N'General', N'general', 2, 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM product_options WHERE Id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02') INSERT INTO product_options VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', N'Color', 2, 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM option_values WHERE Id='bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002') INSERT INTO option_values VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', N'#000000', N'black', 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM products WHERE Slug=N'audifonos-pro-x') INSERT INTO products VALUES ('44444444-4444-4444-4444-444444444401', '33333333-3333-3333-3333-333333333303', N'Audífonos Pro X', N'audifonos-pro-x', N'Demo Postman', 199.99, 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM variants WHERE Sku=N'APX-001-BLK') INSERT INTO variants VALUES ('44444444-4444-4444-4444-444444444402', '44444444-4444-4444-4444-444444444401', N'APX-001-BLK', 199.99, 1, @now, @now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM inventory WHERE VariantId='44444444-4444-4444-4444-444444444402') INSERT INTO inventory VALUES ('44444444-4444-4444-4444-444444444402', 5000, 0);`);
  w(`IF NOT EXISTS (SELECT 1 FROM addresses WHERE Id='66666666-6666-6666-6666-666666666601') INSERT INTO addresses (Id,UserId,Type,Label,ContactName,Street,ExternalNumber,City,State,PostalCode,Country,Phone,Latitude,Longitude,IsDefault,CreatedAt,UpdatedAt) VALUES ('66666666-6666-6666-6666-666666666601','22222222-2222-2222-2222-222222222202',1,N'Casa',N'Cliente Demo',N'Av. Reforma',N'123',N'Ciudad de México',N'CDMX',N'06600',N'MX',N'5551234567',19.4326,-99.1332,1,@now,@now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM drivers WHERE Id='55555555-5555-5555-5555-555555555501') INSERT INTO drivers (Id,UserId,Name,Phone,Email,LicenseNumber,VehicleType,VehiclePlate,Notes,IsActive,StartLatitude,StartLongitude,Capacity,CreatedAt,UpdatedAt) VALUES ('55555555-5555-5555-5555-555555555501','22222222-2222-2222-2222-222222222203',N'Juan Repartidor',N'+5215550001',N'repartidor@ecommerce.local',N'LIC-001',N'Moto',N'ABC-123',N'Principal',1,19.4326,-99.1332,25,@now,@now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM dispatch_settings WHERE Id='99999999-9999-9999-9999-999999999901') INSERT INTO dispatch_settings VALUES ('99999999-9999-9999-9999-999999999901',2.5,20,20,N'Centroid',1,@now,@now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM coupons WHERE Code=N'WELCOME10') INSERT INTO coupons VALUES ('88888888-8888-8888-8888-888888888801',N'WELCOME10',N'Percent',10,50,100000,0,NULL,NULL,1,@now,@now);`);
  w(`IF NOT EXISTS (SELECT 1 FROM covers WHERE Id='77777777-7777-7777-7777-777777777701') INSERT INTO covers VALUES ('77777777-7777-7777-7777-777777777701',N'Promo Audífonos',N'https://placehold.co/1200x400?text=Promo',N'/catalog/products/audifonos-pro-x',1,1,NULL,NULL,@now,@now);`);
  w('');

  w(`DECLARE @i INT = 1;`);
  w(`WHILE @i <= @N BEGIN`);

  w(`  DECLARE @custId UNIQUEIDENTIFIER = CAST('cccccccc-cccc-cccc-cccc-' + RIGHT('000000000000' + CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM users WHERE Id=@custId) INSERT INTO users VALUES (@custId, N'cliente' + RIGHT('0000'+CAST(@i AS VARCHAR(4)),4) + N'@ecommerce.local', @hashCustomer, NULL, 0, N'Cliente', N'Usuario '+CAST(@i AS NVARCHAR(10)), N'+52155'+RIGHT('000000'+CAST(@i AS VARCHAR(6)),6), 1, @now, @now);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM user_roles WHERE UserId=@custId) INSERT INTO user_roles VALUES (@custId, @roleCustomer);`);

  w(`  DECLARE @addrId UNIQUEIDENTIFIER = CAST('66666666-6666-6666-6666-' + RIGHT('000000000000' + CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM addresses WHERE Id=@addrId) INSERT INTO addresses (Id,UserId,Type,Label,ContactName,Street,ExternalNumber,City,State,PostalCode,Country,Phone,Latitude,Longitude,IsDefault,CreatedAt,UpdatedAt) VALUES (@addrId,@custId,1,N'Casa',N'Cliente '+CAST(@i AS NVARCHAR(10)),N'Calle Seed',CAST(@i AS NVARCHAR(10)),N'Ciudad de México',N'CDMX',N'06600',N'MX',N'5550000000',19.42+(@i*0.0001),-99.15+(@i*0.0001),CASE WHEN @i=1 THEN 1 ELSE 0 END,@now,@now);`);

  w(`  DECLARE @subId UNIQUEIDENTIFIER = CASE WHEN @i % 2 = 0 THEN '33333333-3333-3333-3333-333333333303' ELSE '33333333-3333-3333-3333-333333333304' END;`);
  w(`  DECLARE @prodId UNIQUEIDENTIFIER = CAST('44444444-4444-4444-4444-' + RIGHT('000000000000' + CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`  DECLARE @slug NVARCHAR(200) = N'producto-seed-' + RIGHT('0000'+CAST(@i AS VARCHAR(4)),4);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM products WHERE Id=@prodId) INSERT INTO products VALUES (@prodId,@subId,N'Producto Seed '+CAST(@i AS NVARCHAR(10)),@slug,N'Producto generado seed.',50.00+(@i%500),1,@now,@now);`);

  w(`  DECLARE @varId UNIQUEIDENTIFIER = CAST('55555555-aaaa-aaaa-aaaa-' + RIGHT('000000000000' + CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM variants WHERE Id=@varId) BEGIN INSERT INTO variants VALUES (@varId,@prodId,N'SKU-'+RIGHT('000000'+CAST(@i AS VARCHAR(6)),6),50.00+(@i%500),1,@now,@now); INSERT INTO inventory VALUES (@varId,500,0); END;`);

  w(`  DECLARE @drvUser UNIQUEIDENTIFIER = CAST('dddddddd-dddd-dddd-dddd-' + RIGHT('000000000000' + CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`  DECLARE @drvId UNIQUEIDENTIFIER = CAST('55555555-5555-5555-5555-' + RIGHT('000000000000' + CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM users WHERE Id=@drvUser) INSERT INTO users VALUES (@drvUser,N'repartidor'+RIGHT('0000'+CAST(@i AS VARCHAR(4)),4)+N'@ecommerce.local',@hashDriver,NULL,0,N'Repartidor',N'Conductor '+CAST(@i AS NVARCHAR(10)),N'+52155000000',1,@now,@now);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM user_roles WHERE UserId=@drvUser) INSERT INTO user_roles VALUES (@drvUser,@roleDriver);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM drivers WHERE Id=@drvId) INSERT INTO drivers (Id,UserId,Name,Phone,Email,LicenseNumber,VehicleType,VehiclePlate,Notes,IsActive,StartLatitude,StartLongitude,Capacity,CreatedAt,UpdatedAt) VALUES (@drvId,@drvUser,N'Conductor '+CAST(@i AS NVARCHAR(10)),N'+52155000000',N'repartidor'+RIGHT('0000'+CAST(@i AS VARCHAR(4)),4)+N'@ecommerce.local',N'LIC-'+CAST(@i AS NVARCHAR(10)),N'Moto',N'PLT-'+CAST(@i AS NVARCHAR(10)),N'Seed',1,19.43,-99.14,20,@now,@now);`);

  w(`  DECLARE @orderId UNIQUEIDENTIFIER = CAST('00000000-0000-4000-8000-' + RIGHT('000000000000' + CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`  DECLARE @st NVARCHAR(30), @dsp NVARCHAR(30), @shipSt NVARCHAR(30);`);
  w(`  SET @st = CASE @i%10 WHEN 0 THEN N'PendingPayment' WHEN 1 THEN N'Paid' WHEN 2 THEN N'ReadyToDispatch' WHEN 3 THEN N'ReadyToDispatch' WHEN 4 THEN N'ReadyToDispatch' WHEN 5 THEN N'ReadyToDispatch' WHEN 6 THEN N'Dispatched' WHEN 7 THEN N'Dispatched' ELSE N'Delivered' END;`);
  w(`  SET @dsp = CASE @i%10 WHEN 0 THEN N'Pending' WHEN 1 THEN N'Paid' WHEN 2 THEN N'Ready' WHEN 3 THEN N'Batched' WHEN 4 THEN N'Routed' WHEN 5 THEN N'Assigned' WHEN 6 THEN N'Assigned' WHEN 7 THEN N'InTransit' ELSE N'Delivered' END;`);
  w(`  SET @shipSt = CASE WHEN @i%10 >= 8 THEN N'Delivered' WHEN @i%2=0 THEN N'InTransit' ELSE N'Pending' END;`);
  w(`  DECLARE @total DECIMAL(18,2) = 149.99 + 99.00;`);
  w(`  IF NOT EXISTS (SELECT 1 FROM orders WHERE Id=@orderId) INSERT INTO orders (Id,OrderNumber,UserId,Status,DispatchStatus,Subtotal,DiscountAmount,ShippingCost,Total,CreatedAt,UpdatedAt) VALUES (@orderId,N'ORD-SEED-'+RIGHT('000000'+CAST(@i AS VARCHAR(6)),6),@custId,@st,@dsp,149.99,0,99.00,@total,DATEADD(DAY,-(@i%365),@now),DATEADD(DAY,-(@i%365),@now));`);
  w(`  IF NOT EXISTS (SELECT 1 FROM order_items WHERE OrderId=@orderId) INSERT INTO order_items (Id,OrderId,VariantId,ProductName,Sku,Quantity,UnitPrice,LineTotal,CreatedAt,UpdatedAt) VALUES (CAST('10000000-0000-4000-8000-'+RIGHT('000000000000'+CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER),@orderId,@varId,N'Producto Seed',N'SKU-'+RIGHT('000000'+CAST(@i AS VARCHAR(6)),6),1+(@i%3),149.99,149.99*(1+(@i%3)),@now,@now);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM order_addresses WHERE OrderId=@orderId) INSERT INTO order_addresses (Id,OrderId,FullName,Street,City,State,PostalCode,Country,Phone,Latitude,Longitude,AddressText,CreatedAt,UpdatedAt) VALUES (CAST('20000000-0000-4000-8000-'+RIGHT('000000000000'+CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER),@orderId,N'Cliente '+CAST(@i AS NVARCHAR(10)),N'Calle '+CAST(@i AS NVARCHAR(10)),N'CDMX',N'CDMX',N'06600',N'MX',N'5550000000',19.43,-99.14,N'Entrega',@now,@now);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM payments WHERE OrderId=@orderId) INSERT INTO payments (Id,OrderId,Amount,Status,CardHolderName,ProviderReference,PaidAt,CreatedAt,UpdatedAt) VALUES (CAST('30000000-0000-4000-8000-'+RIGHT('000000000000'+CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER),@orderId,@total,CASE WHEN @st=N'PendingPayment' THEN N'Pending' ELSE N'Approved' END,N'Cliente',N'MOCK-'+CAST(@i AS NVARCHAR(10)),CASE WHEN @st=N'PendingPayment' THEN NULL ELSE @now END,@now,@now);`);

  w(`  IF NOT EXISTS (SELECT 1 FROM shipments WHERE OrderId=@orderId) BEGIN`);
  w(`    DECLARE @shId UNIQUEIDENTIFIER = CAST('40000000-0000-4000-8000-' + RIGHT('000000000000' + CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`    INSERT INTO shipments (Id,OrderId,DriverId,Status,TrackingNumber,ShippedAt,CreatedAt,UpdatedAt) VALUES (@shId,@orderId,@drvId,@shipSt,N'TRK-'+RIGHT('000000'+CAST(@i AS VARCHAR(6)),6),@now,@now,@now);`);
  w(`    INSERT INTO dispatch_tickets (Id,ShipmentId,TicketNumber,CreatedAt,UpdatedAt) VALUES (CAST('50000000-0000-4000-8000-'+RIGHT('000000000000'+CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER),@shId,N'TKT-'+RIGHT('000000'+CAST(@i AS VARCHAR(6)),6),@now,@now);`);
  w(`  END;`);

  w(`  IF NOT EXISTS (SELECT 1 FROM product_reviews WHERE UserId=@custId AND ProductId='44444444-4444-4444-4444-444444444401') INSERT INTO product_reviews (Id,ProductId,UserId,Rating,Title,Comment,IsApproved,CreatedAt,UpdatedAt) VALUES (NEWID(),'44444444-4444-4444-4444-444444444401',@custId,3+(@i%3),N'Review',N'Comentario seed',1,@now,@now);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM wishlist_items WHERE UserId=@custId AND ProductId=@prodId) INSERT INTO wishlist_items (Id,UserId,ProductId,CreatedAt,UpdatedAt) VALUES (NEWID(),@custId,@prodId,@now,@now);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM carts WHERE UserId=@custId) BEGIN DECLARE @cartId UNIQUEIDENTIFIER=CAST('80000000-0000-4000-8000-'+RIGHT('000000000000'+CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER); INSERT INTO carts VALUES (@cartId,@custId,NULL,@now,@now); INSERT INTO cart_items VALUES (CAST('81000000-0000-4000-8000-'+RIGHT('000000000000'+CAST(@i AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER),@cartId,@varId,1,@now,@now); END;`);

  w(`  IF @i % 100 = 0 PRINT 'Seed progreso: ' + CAST(@i AS VARCHAR(10)) + ' / ' + CAST(@N AS VARCHAR(10));`);
  w(`  SET @i += 1;`);
  w(`END;`);
  w('');

  w(`-- Lotes y rutas de muestra (100 lotes)`);
  w(`DECLARE @b INT = 1; WHILE @b <= 100 BEGIN`);
  w(`  DECLARE @batchId UNIQUEIDENTIFIER = CAST('99999999-9999-9999-9999-' + RIGHT('000000000000'+CAST(@b AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`  IF NOT EXISTS (SELECT 1 FROM dispatch_batches WHERE Id=@batchId) INSERT INTO dispatch_batches VALUES (@batchId,N'BATCH-'+RIGHT('0000'+CAST(@b AS VARCHAR(4)),4),CASE WHEN @b%3=0 THEN N'Locked' ELSE N'Open' END,19.43,-99.14,2.5,20,@now,@now);`);
  w(`  DECLARE @bo INT = ((@b-1)*10)+1; IF @bo <= @N BEGIN`);
  w(`    DECLARE @boOrder UNIQUEIDENTIFIER = CAST('00000000-0000-4000-8000-' + RIGHT('000000000000'+CAST(@bo AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER);`);
  w(`    IF NOT EXISTS (SELECT 1 FROM dispatch_batch_orders WHERE OrderId=@boOrder) INSERT INTO dispatch_batch_orders (Id,BatchId,OrderId,DistanceKm,CreatedAt,UpdatedAt) VALUES (CAST('60000000-0000-4000-8000-'+RIGHT('000000000000'+CAST(@b AS VARCHAR(12)),12) AS UNIQUEIDENTIFIER),@batchId,@boOrder,1.5,@now,@now);`);
  w(`  END; SET @b += 1; END;`);
  w('');

  w('GO');
  w('DECLARE @u INT,@o INT,@s INT,@d INT,@p INT,@v INT;');
  w('SELECT @u=COUNT(*) FROM users; SELECT @o=COUNT(*) FROM orders; SELECT @s=COUNT(*) FROM shipments;');
  w('SELECT @d=COUNT(*) FROM drivers; SELECT @p=COUNT(*) FROM products; SELECT @v=COUNT(*) FROM variants;');
  w(`PRINT '=== Seed ${N}+ aplicado ===';`);
  w("PRINT 'Usuarios: '+CAST(@u AS VARCHAR(10))+' | Productos: '+CAST(@p AS VARCHAR(10))+' | Variantes: '+CAST(@v AS VARCHAR(10));");
  w("PRINT 'Pedidos: '+CAST(@o AS VARCHAR(10))+' | Envios: '+CAST(@s AS VARCHAR(10))+' | Conductores: '+CAST(@d AS VARCHAR(10));");
  w("PRINT 'Postman: admin@ecommerce.local / Admin123! | cliente@ecommerce.local / Cliente123!';");
  w('GO');

  return lines.join('\n');
}

function generateMySql(label = 'MySQL 8+') {
  const N = BULK_COUNT;
  return `-- ECOMMERCE API — Seed completo (${label})
-- Volumen: ${N}+ por entidad | mysql -u root -p ecommerce < seed.mysql.sql

USE ecommerce;
SET @now := UTC_TIMESTAMP(6);
SET @N := ${N};

${PERMISSIONS.map((p) => `INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'${p}','${p}',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='${p}');`).join('\n')}

INSERT IGNORE INTO roles VALUES ('11111111-1111-1111-1111-111111111101','Administrador','admin',@now,@now),('11111111-1111-1111-1111-111111111102','Cliente','customer',@now,@now),('11111111-1111-1111-1111-111111111103','Repartidor','driver',@now,@now);
INSERT IGNORE INTO users VALUES ('22222222-2222-2222-2222-222222222201','admin@ecommerce.local','${HASH_ADMIN}',NULL,0,'Admin','Sistema',NULL,1,@now,@now),('22222222-2222-2222-2222-222222222202','cliente@ecommerce.local','${HASH_CUSTOMER}',NULL,0,'Cliente','Demo','+5215550100',1,@now,@now),('22222222-2222-2222-2222-222222222203','repartidor@ecommerce.local','${HASH_DRIVER}',NULL,0,'Juan','Repartidor','+5215550001',1,@now,@now);
INSERT IGNORE INTO user_roles VALUES ('22222222-2222-2222-2222-222222222201','11111111-1111-1111-1111-111111111101'),('22222222-2222-2222-2222-222222222202','11111111-1111-1111-1111-111111111102'),('22222222-2222-2222-2222-222222222203','11111111-1111-1111-1111-111111111103');
INSERT IGNORE INTO role_permissions SELECT '11111111-1111-1111-1111-111111111101', Id FROM permissions;
INSERT IGNORE INTO families VALUES ('33333333-3333-3333-3333-333333333301','Electrónica','electronica',1,1,@now,@now);
INSERT IGNORE INTO categories VALUES ('33333333-3333-3333-3333-333333333302','33333333-3333-3333-3333-333333333301','Audio','audio',1,1,@now,@now);
INSERT IGNORE INTO subcategories VALUES ('33333333-3333-3333-3333-333333333303','33333333-3333-3333-3333-333333333302','Audífonos','audifonos',1,1,@now,@now),('33333333-3333-3333-3333-333333333304','33333333-3333-3333-3333-333333333302','General','general',2,1,@now,@now);
INSERT IGNORE INTO products VALUES ('44444444-4444-4444-4444-444444444401','33333333-3333-3333-3333-333333333303','Audífonos Pro X','audifonos-pro-x','Demo',199.99,1,@now,@now);
INSERT IGNORE INTO variants VALUES ('44444444-4444-4444-4444-444444444402','44444444-4444-4444-4444-444444444401','APX-001-BLK',199.99,1,@now,@now);
INSERT IGNORE INTO inventory VALUES ('44444444-4444-4444-4444-444444444402',5000,0);

DELIMITER //
DROP PROCEDURE IF EXISTS sp_seed_bulk //
CREATE PROCEDURE sp_seed_bulk()
BEGIN
  DECLARE i INT DEFAULT 1;
  DECLARE n DATETIME(6) DEFAULT UTC_TIMESTAMP(6);
  WHILE i <= ${N} DO
    SET @cust = CONCAT('cccccccc-cccc-cccc-cccc-', LPAD(i,12,'0'));
    INSERT IGNORE INTO users VALUES (@cust, CONCAT('cliente', LPAD(i,4,'0'), '@ecommerce.local'), '${HASH_CUSTOMER}', NULL, 0, 'Cliente', CONCAT('Usuario ', i), '+52155000000', 1, n, n);
    INSERT IGNORE INTO user_roles VALUES (@cust, '11111111-1111-1111-1111-111111111102');
    INSERT IGNORE INTO addresses (Id,UserId,Type,Label,ContactName,Street,ExternalNumber,City,State,PostalCode,Country,Phone,Latitude,Longitude,IsDefault,CreatedAt,UpdatedAt)
      VALUES (CONCAT('66666666-6666-6666-6666-', LPAD(i,12,'0')), @cust, 1, 'Casa', CONCAT('Cliente ', i), 'Calle', CAST(i AS CHAR), 'CDMX', 'CDMX', '06600', 'MX', '5550000000', 19.43, -99.14, IF(i=1,1,0), n, n);
    SET @prod = CONCAT('44444444-4444-4444-4444-', LPAD(i,12,'0'));
    INSERT IGNORE INTO products VALUES (@prod, IF(i%2=0,'33333333-3333-3333-3333-333333333303','33333333-3333-3333-3333-333333333304'), CONCAT('Producto ', i), CONCAT('producto-seed-', LPAD(i,4,'0')), 'Seed', 50+(i%500), 1, n, n);
    SET @var = CONCAT('55555555-aaaa-aaaa-aaaa-', LPAD(i,12,'0'));
    INSERT IGNORE INTO variants VALUES (@var, @prod, CONCAT('SKU-', LPAD(i,6,'0')), 50+(i%500), 1, n, n);
    INSERT IGNORE INTO inventory VALUES (@var, 500, 0);
    SET @drvU = CONCAT('dddddddd-dddd-dddd-dddd-', LPAD(i,12,'0'));
    SET @drv = CONCAT('55555555-5555-5555-5555-', LPAD(i,12,'0'));
    INSERT IGNORE INTO users VALUES (@drvU, CONCAT('repartidor', LPAD(i,4,'0'), '@ecommerce.local'), '${HASH_DRIVER}', NULL, 0, 'Repartidor', CONCAT('Cond ', i), '+52155000000', 1, n, n);
    INSERT IGNORE INTO user_roles VALUES (@drvU, '11111111-1111-1111-1111-111111111103');
    INSERT IGNORE INTO drivers (Id,UserId,Name,Phone,Email,LicenseNumber,VehicleType,VehiclePlate,Notes,IsActive,StartLatitude,StartLongitude,Capacity,CreatedAt,UpdatedAt)
      VALUES (@drv, @drvU, CONCAT('Conductor ', i), '+52155000000', CONCAT('repartidor', LPAD(i,4,'0'), '@ecommerce.local'), CONCAT('LIC-', i), 'Moto', CONCAT('P', i), 'Seed', 1, 19.43, -99.14, 20, n, n);
    SET @ord = CONCAT('00000000-0000-4000-8000-', LPAD(i,12,'0'));
    INSERT IGNORE INTO orders (Id,OrderNumber,UserId,Status,DispatchStatus,Subtotal,DiscountAmount,ShippingCost,Total,CreatedAt,UpdatedAt)
      VALUES (@ord, CONCAT('ORD-SEED-', LPAD(i,6,'0')), @cust, ELT(1+(i%10),'PendingPayment','Paid','ReadyToDispatch','ReadyToDispatch','ReadyToDispatch','ReadyToDispatch','Dispatched','Dispatched','Delivered','Delivered'), 'Paid', 149.99, 0, 99, 248.99, DATE_SUB(n, INTERVAL (i%365) DAY), DATE_SUB(n, INTERVAL (i%365) DAY));
    INSERT IGNORE INTO order_items (Id,OrderId,VariantId,ProductName,Sku,Quantity,UnitPrice,LineTotal,CreatedAt,UpdatedAt)
      VALUES (CONCAT('10000000-0000-4000-8000-', LPAD(i,12,'0')), @ord, @var, 'Producto', CONCAT('SKU-', LPAD(i,6,'0')), 1, 149.99, 149.99, n, n);
    INSERT IGNORE INTO payments (Id,OrderId,Amount,Status,CardHolderName,ProviderReference,PaidAt,CreatedAt,UpdatedAt)
      VALUES (CONCAT('30000000-0000-4000-8000-', LPAD(i,12,'0')), @ord, 248.99, IF(i%10=0,'Pending','Approved'), 'Cliente', CONCAT('MOCK-', i), IF(i%10=0,NULL,n), n, n);
    INSERT IGNORE INTO shipments (Id,OrderId,DriverId,Status,TrackingNumber,ShippedAt,CreatedAt,UpdatedAt)
      VALUES (CONCAT('40000000-0000-4000-8000-', LPAD(i,12,'0')), @ord, @drv, IF(i%10>=8,'Delivered',IF(i%2=0,'InTransit','Pending')), CONCAT('TRK-', LPAD(i,6,'0')), n, n, n);
    SET i = i + 1;
  END WHILE;
END //
DELIMITER ;
CALL sp_seed_bulk();
DROP PROCEDURE sp_seed_bulk;
SELECT COUNT(*) AS users FROM users;
SELECT COUNT(*) AS orders FROM orders;
SELECT COUNT(*) AS shipments FROM shipments;
`;
}

function generatePostgres() {
  const N = BULK_COUNT;
  return `-- ECOMMERCE API — Seed completo (PostgreSQL 14+)
-- Volumen: ${N}+ por entidad | psql -U postgres -d ecommerce -f seed.postgresql.sql

DO $$
DECLARE i INT; n TIMESTAMPTZ := NOW() AT TIME ZONE 'UTC';
BEGIN
  ${PERMISSIONS.map((p) => `INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'${p}','${p}',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='${p}');`).join('\n  ')}
  INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111101','Administrador','admin',n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111102','Cliente','customer',n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111103','Repartidor','driver',n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222201','admin@ecommerce.local','${HASH_ADMIN}',NULL,FALSE,'Admin','Sistema',NULL,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222202','cliente@ecommerce.local','${HASH_CUSTOMER}',NULL,FALSE,'Cliente','Demo','+5215550100',TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222203','repartidor@ecommerce.local','${HASH_DRIVER}',NULL,FALSE,'Juan','Repartidor','+5215550001',TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO families VALUES ('33333333-3333-3333-3333-333333333301','Electrónica','electronica',1,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO categories VALUES ('33333333-3333-3333-3333-333333333302','33333333-3333-3333-3333-333333333301','Audio','audio',1,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO subcategories VALUES ('33333333-3333-3333-3333-333333333303','33333333-3333-3333-3333-333333333302','Audífonos','audifonos',1,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO products VALUES ('44444444-4444-4444-4444-444444444401','33333333-3333-3333-3333-333333333303','Audífonos Pro X','audifonos-pro-x','Demo',199.99,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO variants VALUES ('44444444-4444-4444-4444-444444444402','44444444-4444-4444-4444-444444444401','APX-001-BLK',199.99,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO inventory VALUES ('44444444-4444-4444-4444-444444444402',5000,0) ON CONFLICT (VariantId) DO NOTHING;

  FOR i IN 1..${N} LOOP
    INSERT INTO users VALUES (('cccccccc-cccc-cccc-cccc-'||LPAD(i::text,12,'0'))::uuid, 'cliente'||LPAD(i::text,4,'0')||'@ecommerce.local','${HASH_CUSTOMER}',NULL,FALSE,'Cliente','Usuario '||i,'+52155000000',TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
    INSERT INTO user_roles VALUES (('cccccccc-cccc-cccc-cccc-'||LPAD(i::text,12,'0'))::uuid,'11111111-1111-1111-1111-111111111102') ON CONFLICT DO NOTHING;
    INSERT INTO products VALUES (('44444444-4444-4444-4444-'||LPAD(i::text,12,'0'))::uuid,'33333333-3333-3333-3333-333333333303','Producto '||i,'producto-seed-'||LPAD(i::text,4,'0'),'Seed',50+(i%500),TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
    INSERT INTO variants VALUES (('55555555-aaaa-aaaa-aaaa-'||LPAD(i::text,12,'0'))::uuid,('44444444-4444-4444-4444-'||LPAD(i::text,12,'0'))::uuid,'SKU-'||LPAD(i::text,6,'0'),50+(i%500),TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
    INSERT INTO inventory VALUES (('55555555-aaaa-aaaa-aaaa-'||LPAD(i::text,12,'0'))::uuid,500,0) ON CONFLICT (VariantId) DO NOTHING;
    INSERT INTO drivers VALUES (('55555555-5555-5555-5555-'||LPAD(i::text,12,'0'))::uuid,('dddddddd-dddd-dddd-dddd-'||LPAD(i::text,12,'0'))::uuid,'Conductor '||i,'+52155000000','rep@local','LIC', 'Moto','P','N',TRUE,19.43,-99.14,20,n,n) ON CONFLICT (Id) DO NOTHING;
    INSERT INTO orders (Id,OrderNumber,UserId,Status,DispatchStatus,Subtotal,DiscountAmount,ShippingCost,Total,CreatedAt,UpdatedAt)
      VALUES (('00000000-0000-4000-8000-'||LPAD(i::text,12,'0'))::uuid,'ORD-SEED-'||LPAD(i::text,6,'0'),('cccccccc-cccc-cccc-cccc-'||LPAD(i::text,12,'0'))::uuid,'Paid','Paid',149.99,0,99,248.99,n,n) ON CONFLICT (Id) DO NOTHING;
    INSERT INTO shipments (Id,OrderId,DriverId,Status,TrackingNumber,ShippedAt,CreatedAt,UpdatedAt)
      VALUES (('40000000-0000-4000-8000-'||LPAD(i::text,12,'0'))::uuid,('00000000-0000-4000-8000-'||LPAD(i::text,12,'0'))::uuid,('55555555-5555-5555-5555-'||LPAD(i::text,12,'0'))::uuid, CASE WHEN i%10>=8 THEN 'Delivered' WHEN i%2=0 THEN 'InTransit' ELSE 'Pending' END,'TRK',n,n,n) ON CONFLICT (Id) DO NOTHING;
  END LOOP;
END $$;
SELECT COUNT(*) AS users FROM users;
SELECT COUNT(*) AS orders FROM orders;
`;
}

// --- write ---
fs.writeFileSync(path.join(root, 'seed.sqlserver.sql'), generateSqlServer(), 'utf8');
fs.writeFileSync(path.join(root, 'seed.mysql.sql'), generateMySql('MySQL 8+'), 'utf8');
fs.writeFileSync(path.join(root, 'seed.mariadb.sql'), generateMySql('MariaDB 10.6+'), 'utf8');
fs.writeFileSync(path.join(root, 'seed.postgresql.sql'), generatePostgres(), 'utf8');

console.log(`Generated seeds with BULK_COUNT=${BULK_COUNT}`);
console.log('  seed.sqlserver.sql, seed.mysql.sql, seed.mariadb.sql, seed.postgresql.sql');
