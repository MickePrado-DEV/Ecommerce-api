-- ECOMMERCE API — Seed completo (MariaDB 10.6+)
-- Volumen: 1000+ por entidad | mysql -u root -p ecommerce < seed.mysql.sql

USE ecommerce;
SET @now := UTC_TIMESTAMP(6);
SET @N := 1000;

INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.dashboard.view','admin.dashboard.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.dashboard.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.covers.view','admin.covers.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.covers.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.covers.manage','admin.covers.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.covers.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.families.view','admin.families.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.families.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.families.manage','admin.families.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.families.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.categories.view','admin.categories.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.categories.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.categories.manage','admin.categories.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.categories.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.subcategories.view','admin.subcategories.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.subcategories.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.subcategories.manage','admin.subcategories.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.subcategories.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.products.view','admin.products.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.products.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.products.manage','admin.products.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.products.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.options.view','admin.options.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.options.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.options.manage','admin.options.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.options.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.stock.view','admin.stock.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.stock.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.stock.manage','admin.stock.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.stock.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.drivers.view','admin.drivers.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.drivers.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.drivers.manage','admin.drivers.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.drivers.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.orders.view','admin.orders.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.orders.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.orders.manage','admin.orders.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.orders.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.shipments.view','admin.shipments.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.shipments.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.shipments.manage','admin.shipments.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.shipments.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.dispatch.view','admin.dispatch.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.dispatch.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.dispatch.manage','admin.dispatch.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.dispatch.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.users.view','admin.users.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.users.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.users.manage','admin.users.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.users.manage');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.roles.view','admin.roles.view',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.roles.view');
INSERT IGNORE INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT UUID(),'admin.roles.manage','admin.roles.manage',@now,@now FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.roles.manage');

INSERT IGNORE INTO roles VALUES ('11111111-1111-1111-1111-111111111101','Administrador','admin',@now,@now),('11111111-1111-1111-1111-111111111102','Cliente','customer',@now,@now),('11111111-1111-1111-1111-111111111103','Repartidor','driver',@now,@now);
INSERT IGNORE INTO users VALUES ('22222222-2222-2222-2222-222222222201','admin@ecommerce.local','$2a$11$AiFO5/uMXLbByntOOjudIOYDU0uOGA56W0AXP/GZFK6NAaWagiZpG',NULL,0,'Admin','Sistema',NULL,1,@now,@now),('22222222-2222-2222-2222-222222222202','cliente@ecommerce.local','$2a$11$gfwpufX8e5IgdVd8RGcPWOfU6AEUs4iFSyeTjJCPd5CZZP15ibJOS',NULL,0,'Cliente','Demo','+5215550100',1,@now,@now),('22222222-2222-2222-2222-222222222203','repartidor@ecommerce.local','$2a$11$7Xhavc/I5a8uxYxaBuS8EO7HJjd5uwGeyay3.CU0b1RQMv4QZW.s.',NULL,0,'Juan','Repartidor','+5215550001',1,@now,@now);
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
  WHILE i <= 1000 DO
    SET @cust = CONCAT('cccccccc-cccc-cccc-cccc-', LPAD(i,12,'0'));
    INSERT IGNORE INTO users VALUES (@cust, CONCAT('cliente', LPAD(i,4,'0'), '@ecommerce.local'), '$2a$11$gfwpufX8e5IgdVd8RGcPWOfU6AEUs4iFSyeTjJCPd5CZZP15ibJOS', NULL, 0, 'Cliente', CONCAT('Usuario ', i), '+52155000000', 1, n, n);
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
    INSERT IGNORE INTO users VALUES (@drvU, CONCAT('repartidor', LPAD(i,4,'0'), '@ecommerce.local'), '$2a$11$7Xhavc/I5a8uxYxaBuS8EO7HJjd5uwGeyay3.CU0b1RQMv4QZW.s.', NULL, 0, 'Repartidor', CONCAT('Cond ', i), '+52155000000', 1, n, n);
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
