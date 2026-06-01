-- ECOMMERCE API — Seed completo (PostgreSQL 14+)
-- Volumen: 1000+ por entidad | psql -U postgres -d ecommerce -f seed.postgresql.sql

DO $$
DECLARE i INT; n TIMESTAMPTZ := NOW() AT TIME ZONE 'UTC';
BEGIN
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.dashboard.view','admin.dashboard.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.dashboard.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.covers.view','admin.covers.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.covers.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.covers.manage','admin.covers.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.covers.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.families.view','admin.families.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.families.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.families.manage','admin.families.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.families.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.categories.view','admin.categories.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.categories.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.categories.manage','admin.categories.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.categories.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.subcategories.view','admin.subcategories.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.subcategories.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.subcategories.manage','admin.subcategories.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.subcategories.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.products.view','admin.products.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.products.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.products.manage','admin.products.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.products.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.options.view','admin.options.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.options.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.options.manage','admin.options.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.options.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.stock.view','admin.stock.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.stock.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.stock.manage','admin.stock.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.stock.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.drivers.view','admin.drivers.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.drivers.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.drivers.manage','admin.drivers.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.drivers.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.orders.view','admin.orders.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.orders.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.orders.manage','admin.orders.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.orders.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.shipments.view','admin.shipments.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.shipments.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.shipments.manage','admin.shipments.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.shipments.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.dispatch.view','admin.dispatch.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.dispatch.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.dispatch.manage','admin.dispatch.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.dispatch.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.users.view','admin.users.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.users.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.users.manage','admin.users.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.users.manage');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.roles.view','admin.roles.view',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.roles.view');
  INSERT INTO permissions (Id,Code,Name,CreatedAt,UpdatedAt) SELECT gen_random_uuid(),'admin.roles.manage','admin.roles.manage',n,n WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE Code='admin.roles.manage');
  INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111101','Administrador','admin',n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111102','Cliente','customer',n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO roles VALUES ('11111111-1111-1111-1111-111111111103','Repartidor','driver',n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222201','admin@ecommerce.local','$2a$11$AiFO5/uMXLbByntOOjudIOYDU0uOGA56W0AXP/GZFK6NAaWagiZpG',NULL,FALSE,'Admin','Sistema',NULL,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222202','cliente@ecommerce.local','$2a$11$gfwpufX8e5IgdVd8RGcPWOfU6AEUs4iFSyeTjJCPd5CZZP15ibJOS',NULL,FALSE,'Cliente','Demo','+5215550100',TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO users VALUES ('22222222-2222-2222-2222-222222222203','repartidor@ecommerce.local','$2a$11$7Xhavc/I5a8uxYxaBuS8EO7HJjd5uwGeyay3.CU0b1RQMv4QZW.s.',NULL,FALSE,'Juan','Repartidor','+5215550001',TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO families VALUES ('33333333-3333-3333-3333-333333333301','Electrónica','electronica',1,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO categories VALUES ('33333333-3333-3333-3333-333333333302','33333333-3333-3333-3333-333333333301','Audio','audio',1,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO subcategories VALUES ('33333333-3333-3333-3333-333333333303','33333333-3333-3333-3333-333333333302','Audífonos','audifonos',1,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO products VALUES ('44444444-4444-4444-4444-444444444401','33333333-3333-3333-3333-333333333303','Audífonos Pro X','audifonos-pro-x','Demo',199.99,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO variants VALUES ('44444444-4444-4444-4444-444444444402','44444444-4444-4444-4444-444444444401','APX-001-BLK',199.99,TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
  INSERT INTO inventory VALUES ('44444444-4444-4444-4444-444444444402',5000,0) ON CONFLICT (VariantId) DO NOTHING;

  FOR i IN 1..1000 LOOP
    INSERT INTO users VALUES (('cccccccc-cccc-cccc-cccc-'||LPAD(i::text,12,'0'))::uuid, 'cliente'||LPAD(i::text,4,'0')||'@ecommerce.local','$2a$11$gfwpufX8e5IgdVd8RGcPWOfU6AEUs4iFSyeTjJCPd5CZZP15ibJOS',NULL,FALSE,'Cliente','Usuario '||i,'+52155000000',TRUE,n,n) ON CONFLICT (Id) DO NOTHING;
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
