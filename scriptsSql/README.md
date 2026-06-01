# Scripts SQL — Ecommerce API

Un **script de esquema** y un **script de seed** por motor. Sin carpetas modulares.

## Archivos

| Motor | Crear BD + tablas | Seed masivo |
|-------|-------------------|-------------|
| **SQL Server** | `schema.sqlserver.sql` | `seed.sqlserver.sql` |
| **MySQL 8+** | `schema.mysql.sql` | `seed.mysql.sql` |
| **MariaDB 10.6+** | `schema.mariadb.sql` | `seed.mariadb.sql` |
| **PostgreSQL 14+** | `schema.postgresql.sql` | `seed.postgresql.sql` |

## Ejecución rápida

### SQL Server (LocalDB — perfil actual de la API)

```powershell
cd scriptsSql
.\run-all.ps1
# o solo seed:
.\run-all.ps1 -SeedOnly
```

Manual:

```powershell
sqlcmd -S "(localdb)\mssqllocaldb" -E -i schema.sqlserver.sql
sqlcmd -S "(localdb)\mssqllocaldb" -E -d ecommerce -i seed.sqlserver.sql
```

### MySQL

```powershell
.\run-all.ps1 -Provider MySql -MySqlUser root -MySqlPassword tu_password
```

```bash
mysql -u root -p < schema.mysql.sql
mysql -u root -p ecommerce < seed.mysql.sql
```

### MariaDB

```powershell
.\run-all.ps1 -Provider MariaDb -MySqlUser root -MySqlPassword tu_password
```

### PostgreSQL

```powershell
.\run-all.ps1 -Provider PostgreSql -PgUser postgres -PgPassword tu_password
```

```bash
psql -U postgres -f schema.postgresql.sql
psql -U postgres -d ecommerce -f seed.postgresql.sql
```

## Contenido del seed (mínimo 1000 c/u)

| Entidad | Cantidad |
|---------|----------|
| Usuarios totales | ~2003 (1000 clientes + 1000 repartidores + 3 fijos) |
| Productos | 1001 (+ `audifonos-pro-x`) |
| Variantes / inventario | 1001 |
| Direcciones | 1000 |
| Pedidos + pagos + ítems | 1000 |
| Envíos + tickets | 1000 |
| Conductores | 1001 |
| Reseñas / wishlist / carritos | 1000 c/u |
| Lotes despacho | 100 |

Estados repartidos en pedidos: `PendingPayment`, `Paid`, `ReadyToDispatch`, `Dispatched`, `Delivered`, etc.

Cambiar volumen: `BULK_COUNT` en `tools/generate-seed.mjs` → `node scriptsSql/tools/generate-seed.mjs`

## Credenciales demo

| Rol | Email | Password |
|-----|-------|----------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente Postman | `cliente@ecommerce.local` | `Cliente123!` |
| Clientes bulk | `cliente0001@` … `cliente1000@ecommerce.local` | `Cliente123!` |
| Repartidores bulk | `repartidor0001@` … `repartidor1000@ecommerce.local` | `Repartidor123!` |

Variables Postman: `productSlug=audifonos-pro-x`, `addressId=66666666-6666-6666-6666-666666666601`, `couponCode=WELCOME10`.

## Regenerar seeds y esquemas

```bash
node scriptsSql/tools/generate-seed.mjs    # seed.sqlserver + mysql + mariadb + postgresql
node scriptsSql/tools/build-scripts.mjs    # schema mysql/mariadb/postgresql desde SQL Server
```

SQL Server es **canónico** para el esquema; los seeds se generan desde `generate-seed.mjs`.

## Hash BCrypt

```powershell
cd tools/HashGen
dotnet run
```

## Nota API

La API .NET usa hoy **SQL Server** (`EnsureCreated` + perfil SqlServer). MySQL/MariaDB/PostgreSQL quedan listos para cuando Pomelo/Npgsql estén habilitados en el proyecto.
