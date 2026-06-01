/**
 * Genera schema + seed para MySQL, MariaDB y PostgreSQL
 * a partir de los archivos canónicos SQL Server.
 *
 * Uso: node scriptsSql/tools/build-scripts.mjs
 */
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.join(__dirname, '..');

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

const PRODUCTS = [
  ['44444444-4444-4444-4444-444444444411', '33333333-3333-3333-3333-333333333303', 'Smartwatch Fit', 'smartwatch-fit', 149.99, 'SWF'],
  ['44444444-4444-4444-4444-444444444412', '33333333-3333-3333-3333-333333333303', 'Parlante Bluetooth', 'parlante-bt', 89.99, 'PBT'],
  ['44444444-4444-4444-4444-444444444413', '33333333-3333-3333-3333-333333333303', 'Mouse Gamer RGB', 'mouse-gamer-rgb', 59.99, 'MGR'],
  ['44444444-4444-4444-4444-444444444414', '33333333-3333-3333-3333-333333333303', 'Teclado Mecánico', 'teclado-mecanico', 129.99, 'TMK'],
  ['44444444-4444-4444-4444-444444444421', '33333333-3333-3333-3333-333333333308', 'Sartén Antiadherente', 'sarten-antiadherente', 39.99, 'SAN'],
  ['44444444-4444-4444-4444-444444444422', '33333333-3333-3333-3333-333333333308', 'Lámpara LED', 'lampara-led', 29.99, 'LLD'],
  ['44444444-4444-4444-4444-444444444423', '33333333-3333-3333-3333-333333333308', 'Juego de Cuchillos', 'cuchillos-chef', 79.99, 'JCC'],
  ['44444444-4444-4444-4444-444444444431', '33333333-3333-3333-3333-333333333309', 'Mancuernas 10kg', 'mancuernas-10kg', 49.99, 'M10'],
  ['44444444-4444-4444-4444-444444444432', '33333333-3333-3333-3333-333333333309', 'Colchoneta Yoga', 'colchoneta-yoga', 24.99, 'CYG'],
  ['44444444-4444-4444-4444-444444444433', '33333333-3333-3333-3333-333333333309', 'Banda Elástica Set', 'banda-elastica-set', 19.99, 'BES'],
];

function readSqlServerTables() {
  const src = fs.readFileSync(path.join(root, 'schema.sqlserver.sql'), 'utf8');
  const start = src.indexOf('CREATE TABLE users');
  const end = src.lastIndexOf('GO');
  return src.slice(start, end > start ? end : undefined).replace(/\r\nGO\s*$/i, '').trim();
}

function toMySqlSchema(tablesSql) {
  let sql = tablesSql
    .replace(/\bUNIQUEIDENTIFIER\b/g, 'CHAR(36)')
    .replace(/\bNVARCHAR\s*\(\s*MAX\s*\)/gi, 'TEXT')
    .replace(/\bNVARCHAR\s*\((\d+)\)/gi, 'VARCHAR($1)')
    .replace(/\bDATETIME2\b/g, 'DATETIME(6)')
    .replace(/\bBIT\b/g, 'TINYINT(1)')
    .replace(/\[References\]/g, '`References`')
    .replace(/\bN'/g, "'")
    .replace(/\r\nGO\s*\r\n/g, ';\n\n')
    .replace(/CONSTRAINT DF_[^\s]+\s+/gi, '');

  return `-- ECOMMERCE API — Esquema completo (MySQL 8+)
-- mysql -u root -p < schema.mysql.sql

DROP DATABASE IF EXISTS ecommerce;
CREATE DATABASE ecommerce CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE ecommerce;
SET FOREIGN_KEY_CHECKS = 0;

${sql};

SET FOREIGN_KEY_CHECKS = 1;
SELECT 'Esquema ecommerce creado correctamente.' AS message;
`;
}

function toPostgresSchema(tablesSql) {
  let sql = tablesSql
    .replace(/\bUNIQUEIDENTIFIER\b/g, 'UUID')
    .replace(/\bNVARCHAR\s*\(\s*MAX\s*\)/gi, 'TEXT')
    .replace(/\bNVARCHAR\s*\((\d+)\)/gi, 'VARCHAR($1)')
    .replace(/\bDATETIME2\b/g, 'TIMESTAMPTZ')
    .replace(/\bBIT\b/g, 'BOOLEAN')
    .replace(/\[References\]/g, '"References"')
    .replace(/\bN'/g, "'")
    .replace(/\r\nGO\s*\r\n/g, ';\n\n')
    .replace(/CONSTRAINT DF_[^\s]+\s+/gi, '')
    .replace(/\bBIT\b/g, 'BOOLEAN')
    .replace(/BOOLEAN\s+NOT NULL\s+DEFAULT\s+0/gi, 'BOOLEAN NOT NULL DEFAULT FALSE')
    .replace(/BOOLEAN\s+NOT NULL\s+DEFAULT\s+1/gi, 'BOOLEAN NOT NULL DEFAULT TRUE');

  return `-- ECOMMERCE API — Esquema completo (PostgreSQL 14+)
-- psql -U postgres -f schema.postgresql.sql
-- Nota: conectar a postgres; el script crea/recrea la BD ecommerce.

SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'ecommerce' AND pid <> pg_backend_pid();
DROP DATABASE IF EXISTS ecommerce;
CREATE DATABASE ecommerce;
\\c ecommerce

${sql};

SELECT 'Esquema ecommerce creado correctamente.' AS message;
`;
}

function q(dialect, value) {
  if (dialect === 'mssql') return `N'${value.replace(/'/g, "''")}'`;
  return `'${value.replace(/'/g, "''")}'`;
}

function uuid(dialect, value) {
  if (dialect === 'pg') return `'${value}'::uuid`;
  return `'${value}'`;
}

function nowExpr(dialect) {
  if (dialect === 'mssql') return 'SYSUTCDATETIME()';
  if (dialect === 'pg') return 'NOW() AT TIME ZONE \'UTC\'';
  return 'UTC_TIMESTAMP(6)';
}

function boolVal(dialect, v) {
  if (dialect === 'pg') return v ? 'TRUE' : 'FALSE';
  return v ? '1' : '0';
}

function newId(dialect) {
  if (dialect === 'mssql') return 'NEWID()';
  if (dialect === 'pg') return 'gen_random_uuid()';
  return 'UUID()';
}

function generateSeed(dialect, label) {
  const n = nowExpr(dialect);
  const lines = [];
  const push = (s) => lines.push(s);

  push(`-- ECOMMERCE API — Seed completo (${label})`);
  push(`-- Datos masivos relacionados para pruebas E2E / Postman`);
  push('');

  if (dialect === 'mssql') {
    push('USE ecommerce;');
    push('GO');
    push('SET NOCOUNT ON;');
  } else if (dialect === 'mysql') {
    push('USE ecommerce;');
    push('SET FOREIGN_KEY_CHECKS = 0;');
    push('SET @now := UTC_TIMESTAMP(6);');
  } else {
    push('-- Ejecutar conectado a BD ecommerce');
  }

  const declareNow = dialect === 'mssql'
    ? `DECLARE @now DATETIME2 = ${n};`
    : dialect === 'mysql'
      ? ''
      : `DECLARE now_ts TIMESTAMPTZ := ${n};`;

  if (declareNow) push(declareNow);

  // --- Permissions ---
  if (dialect === 'mssql') {
    push(`INSERT INTO permissions (Id, Code, Name, CreatedAt, UpdatedAt)
SELECT ${newId('mssql')}, v.Code, v.Code, @now, @now
FROM (VALUES ${PERMISSIONS.map((p) => `('${p}')`).join(', ')}) AS v(Code)
WHERE NOT EXISTS (SELECT 1 FROM permissions p WHERE p.Code = v.Code);`);
  } else if (dialect === 'mysql') {
    for (const code of PERMISSIONS) {
      push(`INSERT INTO permissions (Id, Code, Name, CreatedAt, UpdatedAt)
SELECT ${newId('mysql')}, '${code}', '${code}', @now, @now FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM permissions p WHERE p.Code = '${code}');`);
    }
  } else {
    for (const code of PERMISSIONS) {
      push(`INSERT INTO permissions (Id, Code, Name, CreatedAt, UpdatedAt)
SELECT ${newId('pg')}, '${code}', '${code}', ${n}, ${n}
WHERE NOT EXISTS (SELECT 1 FROM permissions p WHERE p.Code = '${code}');`);
    }
  }

  push('');
  push('-- Roles y usuarios principales');
  const coreUsers = [
    ['11111111-1111-1111-1111-111111111101', 'Administrador', 'admin', '22222222-2222-2222-2222-222222222201', 'admin@ecommerce.local', HASH_ADMIN, 'Admin', 'Sistema', null, '11111111-1111-1111-1111-111111111101'],
    ['11111111-1111-1111-1111-111111111102', 'Cliente', 'customer', '22222222-2222-2222-2222-222222222202', 'cliente@ecommerce.local', HASH_CUSTOMER, 'Cliente', 'Demo', '+5215550100', '11111111-1111-1111-1111-111111111102'],
    ['11111111-1111-1111-1111-111111111103', 'Repartidor', 'driver', '22222222-2222-2222-2222-222222222203', 'repartidor@ecommerce.local', HASH_DRIVER, 'Juan', 'Repartidor', '+5215550001', '11111111-1111-1111-1111-111111111103'],
  ];

  for (const [roleId, roleName, roleCode, userId, email, hash, fn, ln, phone, urRole] of coreUsers) {
    if (dialect === 'mssql') {
      push(`IF NOT EXISTS (SELECT 1 FROM roles WHERE Id = ${uuid('mssql', roleId)})
    INSERT INTO roles VALUES (${uuid('mssql', roleId)}, ${q('mssql', roleName)}, ${q('mssql', roleCode)}, @now, @now);`);
      push(`IF NOT EXISTS (SELECT 1 FROM users WHERE Id = ${uuid('mssql', userId)})
    INSERT INTO users (Id, Email, PasswordHash, TemporaryPasswordPlain, MustChangePassword, FirstName, LastName, Phone, IsActive, CreatedAt, UpdatedAt)
    VALUES (${uuid('mssql', userId)}, ${q('mssql', email)}, ${q('mssql', hash)}, NULL, 0, ${q('mssql', fn)}, ${q('mssql', ln)}, ${phone ? q('mssql', phone) : 'NULL'}, 1, @now, @now);`);
      push(`IF NOT EXISTS (SELECT 1 FROM user_roles WHERE UserId = ${uuid('mssql', userId)})
    INSERT INTO user_roles VALUES (${uuid('mssql', userId)}, ${uuid('mssql', urRole)});`);
    } else if (dialect === 'mysql') {
      push(`INSERT IGNORE INTO roles VALUES (${uuid('mysql', roleId)}, '${roleName}', '${roleCode}', @now, @now);`);
      push(`INSERT IGNORE INTO users VALUES (${uuid('mysql', userId)}, '${email}', '${hash}', NULL, 0, '${fn}', '${ln}', ${phone ? `'${phone}'` : 'NULL'}, 1, @now, @now);`);
      push(`INSERT IGNORE INTO user_roles VALUES (${uuid('mysql', userId)}, ${uuid('mysql', urRole)});`);
    } else {
      push(`INSERT INTO roles VALUES (${uuid('pg', roleId)}, '${roleName}', '${roleCode}', ${n}, ${n}) ON CONFLICT (Id) DO NOTHING;`);
      push(`INSERT INTO users (Id, Email, PasswordHash, TemporaryPasswordPlain, MustChangePassword, FirstName, LastName, Phone, IsActive, CreatedAt, UpdatedAt)
VALUES (${uuid('pg', userId)}, '${email}', '${hash}', NULL, FALSE, '${fn}', '${ln}', ${phone ? `'${phone}'` : 'NULL'}, TRUE, ${n}, ${n}) ON CONFLICT (Id) DO NOTHING;`);
      push(`INSERT INTO user_roles VALUES (${uuid('pg', userId)}, ${uuid('pg', urRole)}) ON CONFLICT DO NOTHING;`);
    }
  }

  push('');
  push('-- Catálogo base + opciones');
  if (dialect === 'mssql') {
    push(`IF NOT EXISTS (SELECT 1 FROM families WHERE Id = '33333333-3333-3333-3333-333333333301')
    INSERT INTO families VALUES ('33333333-3333-3333-3333-333333333301', N'Electrónica', N'electronica', 1, 1, @now, @now);
-- ... bulk via same logic as seed.sqlserver.sql - use included file for mssql`);
  }

  // For mysql/pg emit procedural bulk seed
  if (dialect !== 'mssql') {
    push(generateBulkSeedProcedure(dialect));
  }

  push('');
  push(`-- Fin seed ${label}`);
  if (dialect === 'mysql') push('SET FOREIGN_KEY_CHECKS = 1;');

  return lines.join('\n');
}

function generateBulkSeedProcedure(dialect) {
  if (dialect === 'mysql') {
    return `-- Ver seed.sqlserver.sql como referencia canónica del volumen de datos.
-- Este seed MySQL inserta el núcleo + datos masivos vía procedimiento.

DELIMITER //
DROP PROCEDURE IF EXISTS sp_seed_ecommerce //
CREATE PROCEDURE sp_seed_ecommerce()
BEGIN
  DECLARE i INT DEFAULT 1;
  DECLARE n DATETIME(6) DEFAULT UTC_TIMESTAMP(6);

  INSERT IGNORE INTO families VALUES ('33333333-3333-3333-3333-333333333301', 'Electrónica', 'electronica', 1, 1, n, n);
  INSERT IGNORE INTO categories VALUES ('33333333-3333-3333-3333-333333333302', '33333333-3333-3333-3333-333333333301', 'Audio', 'audio', 1, 1, n, n);
  INSERT IGNORE INTO subcategories VALUES ('33333333-3333-3333-3333-333333333303', '33333333-3333-3333-3333-333333333302', 'Audífonos', 'audifonos', 1, 1, n, n);
  INSERT IGNORE INTO families VALUES ('33333333-3333-3333-3333-333333333304', 'Hogar', 'hogar', 2, 1, n, n);
  INSERT IGNORE INTO families VALUES ('33333333-3333-3333-3333-333333333305', 'Deportes', 'deportes', 3, 1, n, n);
  INSERT IGNORE INTO categories VALUES ('33333333-3333-3333-3333-333333333306', '33333333-3333-3333-3333-333333333304', 'Cocina', 'cocina', 1, 1, n, n);
  INSERT IGNORE INTO categories VALUES ('33333333-3333-3333-3333-333333333307', '33333333-3333-3333-3333-333333333305', 'Fitness', 'fitness', 1, 1, n, n);
  INSERT IGNORE INTO subcategories VALUES ('33333333-3333-3333-3333-333333333308', '33333333-3333-3333-3333-333333333306', 'Utensilios', 'utensilios', 1, 1, n, n);
  INSERT IGNORE INTO subcategories VALUES ('33333333-3333-3333-3333-333333333309', '33333333-3333-3333-3333-333333333307', 'Entrenamiento', 'entrenamiento', 1, 1, n, n);

  INSERT IGNORE INTO products VALUES ('44444444-4444-4444-4444-444444444401', '33333333-3333-3333-3333-333333333303', 'Audífonos Pro X', 'audifonos-pro-x', 'Producto demo', 199.99, 1, n, n);
  INSERT IGNORE INTO product_options VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', 'Color', 2, 2, n, n);
  INSERT IGNORE INTO option_values VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', '#000000', 'black', 1, n, n);
  INSERT IGNORE INTO variants VALUES ('44444444-4444-4444-4444-444444444402', '44444444-4444-4444-4444-444444444401', 'APX-001-BLK', 199.99, 1, n, n);
  INSERT IGNORE INTO inventory VALUES ('44444444-4444-4444-4444-444444444402', 500, 0);
  INSERT IGNORE INTO variants VALUES ('44444444-4444-4444-4444-444444444403', '44444444-4444-4444-4444-444444444401', 'APX-001-WHT', 209.99, 1, n, n);
  INSERT IGNORE INTO inventory VALUES ('44444444-4444-4444-4444-444444444403', 500, 0);

  INSERT IGNORE INTO addresses (Id, UserId, Type, Label, ContactName, Street, ExternalNumber, City, State, PostalCode, Country, Phone, Latitude, Longitude, IsDefault, CreatedAt, UpdatedAt)
  VALUES ('66666666-6666-6666-6666-666666666601', '22222222-2222-2222-2222-222222222202', 1, 'Casa', 'Cliente Demo', 'Av. Reforma', '123', 'Ciudad de México', 'CDMX', '06600', 'MX', '5551234567', 19.4326000, -99.1332000, 1, n, n);

  INSERT IGNORE INTO drivers (Id, UserId, Name, Phone, Email, LicenseNumber, VehicleType, VehiclePlate, Notes, IsActive, StartLatitude, StartLongitude, Capacity, CreatedAt, UpdatedAt)
  VALUES ('55555555-5555-5555-5555-555555555501', '22222222-2222-2222-2222-222222222203', 'Juan Repartidor', '+5215550001', 'repartidor@ecommerce.local', 'LIC-DEMO-001', 'Moto', 'ABC-123', 'Principal', 1, 19.4326000, -99.1332000, 25, n, n);

  INSERT IGNORE INTO dispatch_settings VALUES ('99999999-9999-9999-9999-999999999901', 2.5, 20, 20, 'Centroid', 1, n, n);
  INSERT IGNORE INTO coupons VALUES ('88888888-8888-8888-8888-888888888801', 'WELCOME10', 'Percent', 10, 50, 1000, 0, NULL, NULL, 1, n, n);

  INSERT IGNORE INTO role_permissions (RoleId, PermissionId)
  SELECT '11111111-1111-1111-1111-111111111101', p.Id FROM permissions p;

  WHILE i <= 20 DO
    SET @uid = CONCAT('cccccccc-cccc-cccc-cccc-', LPAD(i, 12, '0'));
    INSERT IGNORE INTO users VALUES (@uid, CONCAT('cliente', LPAD(i, 2, '0'), '@ecommerce.local'), '${HASH_CUSTOMER}', NULL, 0, 'Cliente', CONCAT('Usuario ', i), CONCAT('+52155501', LPAD(i, 3, '0')), 1, n, n);
    INSERT IGNORE INTO user_roles VALUES (@uid, '11111111-1111-1111-1111-111111111102');
    INSERT IGNORE INTO addresses (Id, UserId, Type, Label, ContactName, Street, ExternalNumber, City, State, PostalCode, Country, Phone, Latitude, Longitude, IsDefault, CreatedAt, UpdatedAt)
    VALUES (CONCAT('66666666-6666-6666-6666-', LPAD(600 + i, 12, '0')), @uid, 1, 'Casa', CONCAT('Cliente ', i), 'Calle Seed', CAST(i AS CHAR), 'Ciudad de México', 'CDMX', '06600', 'MX', CONCAT('555000', LPAD(i, 3, '0')), 19.4200 + (i * 0.002), -99.1500 + (i * 0.0015), IF(i=1,1,0), n, n);
    SET i = i + 1;
  END WHILE;

  SET i = 2;
  WHILE i <= 7 DO
    SET @uid = CONCAT('dddddddd-dddd-dddd-dddd-', LPAD(i, 12, '0'));
    SET @did = CONCAT('55555555-5555-5555-5555-', LPAD(i, 12, '0'));
    INSERT IGNORE INTO users VALUES (@uid, CONCAT('repartidor', LPAD(i, 2, '0'), '@ecommerce.local'), '${HASH_DRIVER}', NULL, 0, 'Repartidor', CONCAT('Conductor ', i), CONCAT('+52155500', i), 1, n, n);
    INSERT IGNORE INTO user_roles VALUES (@uid, '11111111-1111-1111-1111-111111111103');
    INSERT IGNORE INTO drivers (Id, UserId, Name, Phone, Email, LicenseNumber, VehicleType, VehiclePlate, Notes, IsActive, StartLatitude, StartLongitude, Capacity, CreatedAt, UpdatedAt)
    VALUES (@did, @uid, CONCAT('Conductor ', i), CONCAT('+52155500', i), CONCAT('repartidor', LPAD(i, 2, '0'), '@ecommerce.local'), CONCAT('LIC-SEED-', LPAD(i, 3, '0')), IF(i%2=0,'Moto','Auto'), CONCAT('PLT-', i), 'Seed', 1, 19.4300 + (i * 0.003), -99.1400 - (i * 0.002), 20, n, n);
    SET i = i + 1;
  END WHILE;

  SET i = 1;
  WHILE i <= 80 DO
    SET @orderId = CONCAT('00000000-0000-4000-8000-', LPAD(i, 12, '0'));
    SET @status = CASE
      WHEN i <= 10 THEN 'PendingPayment'
      WHEN i <= 25 THEN 'Paid'
      WHEN i <= 40 THEN 'ReadyToDispatch'
      WHEN i <= 48 THEN 'ReadyToDispatch'
      WHEN i <= 55 THEN 'ReadyToDispatch'
      WHEN i <= 65 THEN 'Dispatched'
      ELSE 'Delivered' END;
    SET @dispatch = CASE
      WHEN i <= 10 THEN 'Pending'
      WHEN i <= 25 THEN 'Paid'
      WHEN i <= 40 THEN 'Ready'
      WHEN i <= 48 THEN 'Batched'
      WHEN i <= 55 THEN 'Routed'
      WHEN i <= 65 THEN 'Assigned'
      ELSE 'Delivered' END;
    SET @total = 199.99 + 99.00;
    INSERT IGNORE INTO orders (Id, OrderNumber, UserId, Status, DispatchStatus, Subtotal, DiscountAmount, ShippingCost, Total, CreatedAt, UpdatedAt)
    VALUES (@orderId, CONCAT('ORD-SEED-', LPAD(i, 6, '0')), '22222222-2222-2222-2222-222222222202', @status, @dispatch, 199.99, 0, 99.00, @total, DATE_SUB(n, INTERVAL (i % 45) DAY), DATE_SUB(n, INTERVAL (i % 45) DAY));
    INSERT IGNORE INTO order_items (Id, OrderId, VariantId, ProductName, Sku, Quantity, UnitPrice, LineTotal, CreatedAt, UpdatedAt)
    VALUES (CONCAT('10000000-0000-4000-8000-', LPAD(i, 12, '0')), @orderId, '44444444-4444-4444-4444-444444444402', 'Audífonos Pro X', 'APX-001-BLK', 1, 199.99, 199.99, n, n);
    INSERT IGNORE INTO order_addresses (Id, OrderId, FullName, Street, City, State, PostalCode, Country, Phone, Latitude, Longitude, AddressText, CreatedAt, UpdatedAt)
    VALUES (CONCAT('20000000-0000-4000-8000-', LPAD(i, 12, '0')), @orderId, 'Cliente Demo', CONCAT('Calle ', i), 'Ciudad de México', 'CDMX', '06600', 'MX', '5550000000', 19.4326, -99.1332, CONCAT('Entrega ', i), n, n);
    INSERT IGNORE INTO payments (Id, OrderId, Amount, Status, CardHolderName, ProviderReference, PaidAt, CreatedAt, UpdatedAt)
    VALUES (CONCAT('30000000-0000-4000-8000-', LPAD(i, 12, '0')), @orderId, @total, IF(i<=10,'Pending','Approved'), 'Cliente Demo', CONCAT('MOCK-', i), IF(i<=10,NULL,n), n, n);
    IF i >= 56 THEN
      INSERT IGNORE INTO shipments (Id, OrderId, DriverId, Status, TrackingNumber, ShippedAt, CreatedAt, UpdatedAt)
      VALUES (CONCAT('40000000-0000-4000-8000-', LPAD(i, 12, '0')), @orderId, '55555555-5555-5555-5555-555555555501', IF(@status='Delivered','Delivered',IF(i%2=0,'InTransit','Pending')), CONCAT('TRK-', i), n, n, n);
    END IF;
    SET i = i + 1;
  END WHILE;
END //
DELIMITER ;

CALL sp_seed_ecommerce();
DROP PROCEDURE sp_seed_ecommerce;`;
  }

  // PostgreSQL
  return `DO $$
DECLARE
  i INT;
  n TIMESTAMPTZ := NOW() AT TIME ZONE 'UTC';
  order_id UUID;
  ord_status TEXT;
  disp_status TEXT;
  ord_total NUMERIC(18,2) := 298.99;
BEGIN
  INSERT INTO families VALUES ('33333333-3333-3333-3333-333333333301', 'Electrónica', 'electronica', 1, TRUE, n, n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO categories VALUES ('33333333-3333-3333-3333-333333333302', '33333333-3333-3333-3333-333333333301', 'Audio', 'audio', 1, TRUE, n, n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO subcategories VALUES ('33333333-3333-3333-3333-333333333303', '33333333-3333-3333-3333-333333333302', 'Audífonos', 'audifonos', 1, TRUE, n, n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO products VALUES ('44444444-4444-4444-4444-444444444401', '33333333-3333-3333-3333-333333333303', 'Audífonos Pro X', 'audifonos-pro-x', 'Producto demo', 199.99, TRUE, n, n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO variants VALUES ('44444444-4444-4444-4444-444444444402', '44444444-4444-4444-4444-444444444401', 'APX-001-BLK', 199.99, TRUE, n, n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO inventory VALUES ('44444444-4444-4444-4444-444444444402', 500, 0) ON CONFLICT (VariantId) DO NOTHING;
  INSERT INTO addresses (Id, UserId, Type, Label, ContactName, Street, ExternalNumber, City, State, PostalCode, Country, Phone, Latitude, Longitude, IsDefault, CreatedAt, UpdatedAt)
  VALUES ('66666666-6666-6666-6666-666666666601', '22222222-2222-2222-2222-222222222202', 1, 'Casa', 'Cliente Demo', 'Av. Reforma', '123', 'Ciudad de México', 'CDMX', '06600', 'MX', '5551234567', 19.4326000, -99.1332000, TRUE, n, n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO drivers (Id, UserId, Name, Phone, Email, LicenseNumber, VehicleType, VehiclePlate, Notes, IsActive, StartLatitude, StartLongitude, Capacity, CreatedAt, UpdatedAt)
  VALUES ('55555555-5555-5555-5555-555555555501', '22222222-2222-2222-2222-222222222203', 'Juan Repartidor', '+5215550001', 'repartidor@ecommerce.local', 'LIC-DEMO-001', 'Moto', 'ABC-123', 'Principal', TRUE, 19.4326000, -99.1332000, 25, n, n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO dispatch_settings VALUES ('99999999-9999-9999-9999-999999999901', 2.5, 20, 20, 'Centroid', TRUE, n, n) ON CONFLICT (Id) DO NOTHING;

  INSERT INTO role_permissions (RoleId, PermissionId)
  SELECT '11111111-1111-1111-1111-111111111101'::uuid, p.Id FROM permissions p
  ON CONFLICT DO NOTHING;

  FOR i IN 1..20 LOOP
    INSERT INTO users VALUES (('cccccccc-cccc-cccc-cccc-' || LPAD(i::text, 12, '0'))::uuid,
      'cliente' || LPAD(i::text, 2, '0') || '@ecommerce.local', '${HASH_CUSTOMER}', NULL, FALSE, 'Cliente', 'Usuario ' || i, '+52155501000', TRUE, n, n) ON CONFLICT (Id) DO NOTHING;
    INSERT INTO user_roles VALUES (('cccccccc-cccc-cccc-cccc-' || LPAD(i::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111102') ON CONFLICT DO NOTHING;
  END LOOP;

  FOR i IN 1..80 LOOP
    order_id := ('00000000-0000-4000-8000-' || LPAD(i::text, 12, '0'))::uuid;
    IF i <= 10 THEN ord_status := 'PendingPayment'; disp_status := 'Pending';
    ELSIF i <= 25 THEN ord_status := 'Paid'; disp_status := 'Paid';
    ELSIF i <= 40 THEN ord_status := 'ReadyToDispatch'; disp_status := 'Ready';
    ELSIF i <= 55 THEN ord_status := 'ReadyToDispatch'; disp_status := 'Batched';
    ELSIF i <= 65 THEN ord_status := 'Dispatched'; disp_status := 'Assigned';
    ELSE ord_status := 'Delivered'; disp_status := 'Delivered'; END IF;

    INSERT INTO orders (Id, OrderNumber, UserId, Status, DispatchStatus, Subtotal, DiscountAmount, ShippingCost, Total, CreatedAt, UpdatedAt)
    VALUES (order_id, 'ORD-SEED-' || LPAD(i::text, 6, '0'), '22222222-2222-2222-2222-222222222202', ord_status, disp_status, 199.99, 0, 99.00, ord_total, n - (i % 45) * INTERVAL '1 day', n - (i % 45) * INTERVAL '1 day')
    ON CONFLICT (Id) DO NOTHING;

    INSERT INTO order_items (Id, OrderId, VariantId, ProductName, Sku, Quantity, UnitPrice, LineTotal, CreatedAt, UpdatedAt)
    VALUES (('10000000-0000-4000-8000-' || LPAD(i::text, 12, '0'))::uuid, order_id, '44444444-4444-4444-4444-444444444402', 'Audífonos Pro X', 'APX-001-BLK', 1, 199.99, 199.99, n, n) ON CONFLICT (Id) DO NOTHING;

    INSERT INTO payments (Id, OrderId, Amount, Status, CardHolderName, ProviderReference, PaidAt, CreatedAt, UpdatedAt)
    VALUES (('30000000-0000-4000-8000-' || LPAD(i::text, 12, '0'))::uuid, order_id, ord_total, CASE WHEN i <= 10 THEN 'Pending' ELSE 'Approved' END, 'Cliente Demo', 'MOCK-' || i, CASE WHEN i <= 10 THEN NULL ELSE n END, n, n) ON CONFLICT (Id) DO NOTHING;

    IF i >= 56 THEN
      INSERT INTO shipments (Id, OrderId, DriverId, Status, TrackingNumber, ShippedAt, CreatedAt, UpdatedAt)
      VALUES (('40000000-0000-4000-8000-' || LPAD(i::text, 12, '0'))::uuid, order_id, '55555555-5555-5555-5555-555555555501',
        CASE WHEN ord_status = 'Delivered' THEN 'Delivered' WHEN i % 2 = 0 THEN 'InTransit' ELSE 'Pending' END, 'TRK-' || i, n, n, n) ON CONFLICT (Id) DO NOTHING;
    END IF;
  END LOOP;
END $$;`;
}

// --- Main ---
const tablesSql = readSqlServerTables();

fs.writeFileSync(path.join(root, 'schema.mysql.sql'), toMySqlSchema(tablesSql));
fs.writeFileSync(path.join(root, 'schema.mariadb.sql'), toMySqlSchema(tablesSql).replace('MySQL 8+', 'MariaDB 10.6+'));
fs.writeFileSync(path.join(root, 'schema.postgresql.sql'), toPostgresSchema(tablesSql));

// SQL Server seed stays as merged seed.sqlserver.sql (canonical)
fs.writeFileSync(path.join(root, 'seed.mysql.sql'), generateSeed('mysql', 'MySQL 8+'));
fs.writeFileSync(path.join(root, 'seed.mariadb.sql'), generateSeed('mysql', 'MariaDB 10.6+').replace('seed.mysql.sql', 'seed.mariadb.sql'));
fs.writeFileSync(path.join(root, 'seed.postgresql.sql'), generateSeed('pg', 'PostgreSQL 14+'));

console.log('Generated: schema.mysql.sql, schema.mariadb.sql, schema.postgresql.sql');
console.log('Generated: seed.mysql.sql, seed.mariadb.sql, seed.postgresql.sql');
console.log('Canonical: schema.sqlserver.sql + seed.sqlserver.sql');
