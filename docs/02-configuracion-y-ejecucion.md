# Configuración y ejecución

## Requisitos

- .NET 10 SDK
- SQL Server LocalDB (perfil por defecto) o SQLite (perfil alternativo)

## Perfiles de launch (`Properties/launchSettings.json`)

| Perfil | Entorno | Base de datos | URL |
|--------|---------|---------------|-----|
| **SqlServer** | `SqlServer` | LocalDB → `ecommerce` | http://localhost:5088 |
| **SqlServer (localhost)** | `SqlServer` | Instancia `localhost` | http://localhost:5088 |
| **Sqlite** | `Sqlite` | Archivo `ecommerce-dev.db` | http://localhost:5089 |

## Archivos de configuración

| Archivo | Cuándo se carga |
|---------|-----------------|
| `appsettings.json` | Siempre (valores base) |
| `appsettings.{Environment}.json` | Según `ASPNETCORE_ENVIRONMENT` |
| `appsettings.SqlServer.json` | Perfil SqlServer |
| `appsettings.Sqlite.json` | Perfil Sqlite |

### Connection string SQL Server (LocalDB)

```text
Data Source=(localdb)\mssqllocaldb;Initial Catalog=ecommerce;Integrated Security=True;Encrypt=True;TrustServerCertificate=True
```

Equivalente a conectar en SSMS a `(localdb)\mssqllocaldb` y usar la base `ecommerce`.

### JWT (`appsettings.*.json`)

| Clave | Uso |
|-------|-----|
| `Jwt:Issuer` | Emisor del token |
| `Jwt:Audience` | Audiencia |
| `Jwt:Secret` | Clave simétrica HMAC (cambiar en producción) |
| `Jwt:AccessTokenMinutes` | Vida del access token (15 min) |
| `Jwt:RefreshTokenDays` | Vida del refresh token (7 días) |

### CORS

Orígenes permitidos en `Cors:Origins` (por defecto `http://localhost:3000` para el frontend).

## Poblar la base de datos (recomendado)

Para desarrollo con datos masivos (~1000+ registros por entidad), usa los scripts SQL **antes** o **después** de arrancar la API:

```powershell
cd scriptsSql
.\run-all.ps1
```

| Archivo | Qué hace |
|---------|----------|
| `schema.sqlserver.sql` | Borra y recrea tablas en LocalDB |
| `seed.sqlserver.sql` | Inserta usuarios, productos, pedidos, envíos, etc. |

Solo seed (esquema ya creado):

```powershell
.\run-all.ps1 -SeedOnly
```

Regenerar seeds con otro volumen: edita `BULK_COUNT` en `tools/generate-seed.mjs` y ejecuta `node scriptsSql/tools/generate-seed.mjs`.

Detalle multi-motor (MySQL, PostgreSQL…): [`../scriptsSql/README.md`](../scriptsSql/README.md)

> **Nota:** al arrancar, la API también ejecuta `EnsureCreated` + `DbSeeder` si la BD está vacía. Para pruebas de listados paginados y despacho, prefiere el seed SQL.

## Arrancar la API

```powershell
cd src/Ecommerce.Api
dotnet run --launch-profile SqlServer
```

Al iniciar:
1. `DatabaseBootstrap` ejecuta `EnsureCreatedAsync()` (crea tablas si no existen).
2. `DbSeeder` inserta datos mínimos demo **solo si no hay usuarios** (no sustituye al seed masivo SQL).

## Logs (Serilog)

- Consola
- Archivo: `src/Ecommerce.Api/logs/ecommerce-{entorno}-YYYYMMDD.log`

## Documentación interactiva

Con la API en marcha (no en Production):

- **Scalar:** http://localhost:5088/scalar/v1
- **OpenAPI JSON:** http://localhost:5088/openapi/v1.json

## Health y readiness

```http
GET /health
```

```json
{ "status": "ok" }
```

```http
GET /ready
```

```json
{ "status": "ready" }
```

Si la BD no responde, `/ready` devuelve **503**.

## Probar con Postman

1. Importar `postman/Ecommerce-API.postman_collection.json` + `Ecommerce-Local.postman_environment.json`
2. Ejecutar **00 - Setup** → `Ready (BD)` y **Login Cliente** o **Login Admin**
3. Ver carpetas por dominio (catálogo, carrito, direcciones, admin covers, etc.)

Detalle: [`../postman/README.md`](../postman/README.md) · Endpoints: [`03-api-endpoints.md`](03-api-endpoints.md)
