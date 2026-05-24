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

## Arrancar la API

```powershell
cd src/Ecommerce.Api
dotnet run --launch-profile SqlServer
```

Al iniciar:
1. `DatabaseBootstrap` ejecuta `EnsureCreatedAsync()` (crea tablas si no existen).
2. `DbSeeder` inserta datos demo si no hay usuarios.

## Logs (Serilog)

- Consola
- Archivo: `src/Ecommerce.Api/logs/ecommerce-{entorno}-YYYYMMDD.log`

## Documentación interactiva

Con la API en marcha (no en Production):

- **Scalar:** http://localhost:5088/scalar/v1
- **OpenAPI JSON:** http://localhost:5088/openapi/v1.json

## Health check

```http
GET /health
```

Respuesta esperada:

```json
{ "status": "ok", "database": "connected" }
```

## Probar con Postman

Ver `../postman/README.md` — importar colección + entorno **Ecommerce - Local**.
