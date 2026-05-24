# Scripts SQL Server — Ecommerce API

Scripts alineados con el modelo **Entity Framework Core** (`EcommerceDbContext`).

| Archivo | Descripción |
|---------|-------------|
| `schema.sqlserver.sql` | Crea la BD `ecommerce` (recrea desde cero) y todas las tablas |
| `seed.sqlserver.sql` | Datos iniciales: permisos, roles, usuarios, catálogo demo |
| `tools/HashGen/` | Utilidad para regenerar hashes BCrypt de contraseñas |

## Requisitos

- SQL Server 2019+ o **LocalDB** (`(localdb)\mssqllocaldb`)
- `sqlcmd` (incluido con SQL Server tools)

## Ejecución (LocalDB)

Desde la carpeta `scriptsSql`:

```powershell
sqlcmd -S "(localdb)\mssqllocaldb" -E -i schema.sqlserver.sql
sqlcmd -S "(localdb)\mssqllocaldb" -E -d ecommerce -i seed.sqlserver.sql
```

## Ejecución (instancia localhost)

```powershell
sqlcmd -S localhost -E -i schema.sqlserver.sql
sqlcmd -S localhost -E -d ecommerce -i seed.sqlserver.sql
```

## Connection string en la API

Tras ejecutar los scripts, usa en `appsettings.SqlServer.json` o perfil **SqlServer**:

```
Data Source=(localdb)\mssqllocaldb;Initial Catalog=ecommerce;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

**Importante:** Si ejecutas los scripts SQL, desactiva la recreación automática de EF al arrancar o usa una BD vacía. La API con `EnsureCreated` también puede crear el esquema; no mezcles un esquema antiguo (snake_case en columnas) con el modelo actual (columnas **PascalCase**).

## Usuarios de prueba (seed)

| Rol | Email | Password |
|-----|-------|----------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |

Producto demo: slug `audifonos-pro-x`, variante SKU `APX-001`.

## Regenerar contraseñas BCrypt

```powershell
cd tools/HashGen
dotnet run
```

Copia los hashes al `INSERT` de usuarios en `seed.sqlserver.sql`.
