# Guía para principiantes — Código, Minimal APIs, Program e inyección de dependencias

Esta guía está pensada para alguien que **empieza en .NET** y en **Minimal APIs**, y quiere entender **qué hace cada archivo y cada línea importante** del proyecto **Ecommerce API**.

Si ya tienes experiencia, usa el [índice técnico](./README.md).

---

## 1. ¿Qué es este proyecto?

Es una **API REST**: un programa que escucha peticiones HTTP (Postman, React, móvil…) y responde **JSON**.

- **Catálogo** de productos  
- **Carrito** (usuario o invitado)  
- **Checkout** y **pago simulado**  
- **Admin**: catálogo, inventario, pedidos, envíos, PDF  

Tecnología: **C#**, **.NET 10**, **Minimal APIs**, **SQL Server** o **SQLite**.

---

## 2. Conceptos básicos de .NET

| Concepto | Qué es |
|----------|--------|
| **Solución (.sln)** | Agrupa varios proyectos. |
| **Proyecto (.csproj)** | Un módulo que compila a DLL. |
| **Clase / archivo .cs** | Código C#. |
| **Namespace** | Organización (`Ecommerce.Api.Endpoints`). |
| **Interface** | Contrato (`IAuthService` = “algo que puede hacer login”). |
| **Dependency Injection (DI)** | .NET **crea y entrega** las dependencias automáticamente. |
| **async / await** | Operaciones que esperan BD/red sin bloquear el servidor. |

---

## 3. ¿Qué son Minimal APIs? (y por qué no hay Controllers)

En .NET “clásico” (MVC) tenías **Controllers** con métodos como `[HttpPost] Login()`.

En **Minimal APIs** (desde .NET 6+) defines rutas **directamente**, sin clase Controller:

```csharp
// Minimal API — una ruta = un handler
app.MapGet("/hola", () => "Hola mundo");

app.MapPost("/login", async (LoginRequest req, IAuthService svc) =>
{
    var result = await svc.LoginAsync(req.Email, req.Password);
    return result is null ? Results.Unauthorized() : Results.Ok(result);
});
```

| Minimal APIs | Controllers MVC |
|--------------|-----------------|
| Rutas en `Program.cs` o archivos `*Endpoints.cs` | Rutas en clases `XController` |
| Menos ceremonia, más compacto | Más estructura para apps muy grandes |
| Ideal para APIs pequeñas/medianas | Muy usado en APIs enterprise antiguas |

**En este proyecto:** todas las rutas viven en `Ecommerce.Api/Endpoints/*.cs`, no hay carpeta `Controllers/`.

---

## 4. `Program.cs` — el corazón de la aplicación

`Program.cs` es el **punto de entrada**. Hace dos cosas en orden:

1. **Configurar servicios** (`WebApplicationBuilder` → `builder.Services`) — antes de arrancar el servidor.  
2. **Configurar el pipeline HTTP** (`WebApplication` → `app`) — middleware, rutas, y `Run`.

Piensa: primero **preparas la cocina** (DI, BD, JWT), luego **abres el restaurante** (escuchar peticiones).

### 4.1 Fase 1: `var builder = WebApplication.CreateBuilder(args)`

| Línea / bloque | Qué hace |
|----------------|----------|
| `WebApplication.CreateBuilder(args)` | Crea el “constructor” de la app; lee `appsettings.json`, variables de entorno, argumentos. |
| `builder.Host.UseSerilog(...)` | Configura **logs** en consola y archivo `logs/ecommerce-*.log`. |
| `builder.Services.AddApplication()` | Registra servicios de negocio (ver sección 5). |
| `builder.Services.AddInfrastructure(...)` | Registra BD, repositorios, JWT, PDF. |
| `builder.Services.AddOpenApi()` | Genera documentación OpenAPI (Scalar la usa). |
| `AddCors(...)` | Permite que un frontend en `localhost:3000` llame a la API desde el navegador. |
| `AddAuthentication` + `AddJwtBearer` | Define cómo **validar** el token JWT en cada petición protegida. |
| `AddAuthorization` + políticas | Crea una política por cada permiso admin (`admin.products.manage`, etc.). |

**`builder.Configuration`:** lee `appsettings.json` + `appsettings.{Entorno}.json`.  
Ejemplo: `Configuration.GetConnectionString("Default")` → cadena de conexión a la BD.

### 4.2 Fase 2: `var app = builder.Build()`

Convierte la configuración en una app ejecutable.

| Línea | Qué hace |
|-------|----------|
| `DatabaseBootstrap.InitializeAsync(app.Services)` | Al arrancar: crea tablas si faltan, corrige esquema viejo, ejecuta **seed** (usuarios demo). |
| `app.UseMiddleware<ExceptionMiddleware>()` | Envuelve todo: si hay error no controlado → JSON `{ "error": "..." }`. |

### 4.3 Documentación Scalar (solo no producción)

```csharp
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference(...);
}
```

En desarrollo puedes abrir `http://localhost:5088/scalar/v1` y probar endpoints desde el navegador.

### 4.4 Pipeline HTTP — orden importa

Los **middleware** se ejecutan **en el orden en que los registras**:

```
Petición entrante
    ↓
ExceptionMiddleware      ← try/catch global
    ↓
SerilogRequestLogging    ← log de cada request
    ↓
CORS                     ← headers para frontend
    ↓
Authentication           ← lee JWT del header Authorization
    ↓
Authorization            ← comprueba permisos / [Authorize]
    ↓
Endpoint (tu ruta)       ← MapGet / MapPost ...
    ↓
Respuesta saliente
```

| Middleware | Función |
|------------|---------|
| `UseSerilogRequestLogging` | Registra método, ruta, tiempo de respuesta. |
| `UseCors("Web")` | Aplica la política CORS definida en builder. |
| `UseAuthentication` | Identifica **quién** es el usuario (JWT). |
| `UseAuthorization` | Decide si **puede** acceder al endpoint. |

Si pones `UseAuthorization` **antes** de `UseAuthentication`, la auth no funcionará bien. El orden del proyecto es el correcto.

### 4.5 Registrar rutas (Minimal APIs)

```csharp
app.MapGet("/health", async (EcommerceDbContext db) => { ... });

var api = app.MapGroup("/api/v1");   // prefijo común
api.MapAuthEndpoints();
api.MapCatalogEndpoints();
// ...
await app.RunAsync();                // arranca Kestrel y escucha
```

- **`MapGroup("/api/v1")`:** todas las rutas hijas empiezan con `/api/v1`.  
- **`MapAuthEndpoints()`:** método de extensión definido en `AuthEndpoints.cs`.  
- **`RunAsync()`:** la app queda viva esperando peticiones.

### 4.6 `public partial class Program;`

Permite que los **tests de integración** arranquen la misma app en memoria (`WebApplicationFactory`). Sin esto, los tests no verían tus endpoints.

### 4.7 `try / catch / finally` con Serilog

Si la app crashea al iniciar, escribe el error fatal en log y cierra Serilog correctamente.

---

## 5. Inyección de dependencias (DI) — explicado para Minimal APIs

### 5.1 ¿Qué problema resuelve?

Sin DI harías esto en cada endpoint:

```csharp
// ❌ Malo: acoplamiento fuerte, difícil de testear
var db = new EcommerceDbContext(...);
var repo = new UserRepository(db);
var jwt = new JwtTokenService(...);
var auth = new AuthService(repo, jwt);
```

Con DI escribes:

```csharp
// ✅ El endpoint solo pide la interfaz
app.MapPost("/login", async (LoginRequest req, IAuthService auth) =>
    await auth.LoginAsync(req.Email, req.Password));
```

.NET **busca en un contenedor** quién implementa `IAuthService` y lo pasa solo.

### 5.2 ¿Dónde se registran las dependencias?

En métodos de extensión llamados desde `Program.cs`:

**`Application/DependencyInjection.cs`**

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<ICatalogService, CatalogService>();
    // ... más servicios
    return services;
}
```

**`Infrastructure/DependencyInjection.cs`**

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
{
    services.AddDbContextPool<EcommerceDbContext>(o => o.UseSqlServer(cs)); // o Sqlite
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    // ... más repositorios
    return services;
}
```

| Registro | Significado |
|----------|-------------|
| `AddScoped<IAuthService, AuthService>()` | Por cada **petición HTTP**, una instancia nueva de `AuthService`. |
| `AddDbContextPool<EcommerceDbContext>` | Contexto EF optimizado (pool de conexiones). |
| `AddValidatorsFromAssembly...` | Registra todos los validadores FluentValidation del proyecto Application. |

**Ciclo de vida (lo básico):**

| Tipo | Cuándo se usa |
|------|----------------|
| **Scoped** | Una instancia por request HTTP (servicios, repositorios, DbContext). |
| **Singleton** | Una instancia para toda la app (poco usado aquí). |
| **Transient** | Nueva instancia cada vez que se pide (poco usado aquí). |

### 5.3 ¿Qué se puede inyectar en un endpoint Minimal API?

En el handler, los parámetros que no vienen de la ruta/body se resuelven por DI:

```csharp
catalog.MapGet("/families", async (ICatalogService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetFamiliesAsync(ct)));
```

| Parámetro | De dónde sale |
|-----------|----------------|
| `ICatalogService svc` | Contenedor DI → `CatalogService` |
| `CancellationToken ct` | ASP.NET Core (cancelación si el cliente cierra) |
| `string slug` en `/products/{slug}` | Segmento de la URL |
| `LoginRequest req` en POST | Body JSON deserializado |
| `HttpContext ctx` | Contexto de la petición actual |
| `EcommerceDbContext db` | Solo en `/health` del Program (ejemplo directo) |

**Regla:** la Api **no debería** usar `EcommerceDbContext` en endpoints normales; usa **servicios** (`ICatalogService`). El health check es una excepción simple.

### 5.4 Interfaces: Application define, Infrastructure implementa

```
AuthEndpoints  →  IAuthService  →  AuthService (Application)
                      ↓ usa
                 IUserRepository  →  UserRepository (Infrastructure)
                      ↓ usa
                 EcommerceDbContext
```

La capa **Api** solo conoce interfaces de **Application**, no las clases concretas de Infrastructure. Eso es **Clean Architecture**.

---

## 6. Archivos `Endpoints/` — anatomía de una ruta

Cada archivo es una **clase estática** con un método de extensión `MapXEndpoints`.

### 6.1 Ejemplo real: catálogo público

```csharp
public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogEndpoints(this RouteGroupBuilder group)
    {
        var catalog = group.MapGroup("/catalog").WithTags("Catalog");

        catalog.MapGet("/families", async (ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetFamiliesAsync(ct)));

        catalog.MapGet("/products/{slug}", async (string slug, ICatalogService svc, CancellationToken ct) =>
        {
            var product = await svc.GetProductBySlugAsync(slug, ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        return group;
    }
}
```

| Pieza | Qué hace |
|-------|----------|
| `this RouteGroupBuilder group` | Recibe el grupo `/api/v1` desde Program. |
| `MapGroup("/catalog")` | Prefijo: `/api/v1/catalog`. |
| `WithTags("Catalog")` | Agrupa en Scalar/OpenAPI. |
| `MapGet("/families", ...)` | GET `/api/v1/catalog/families`. |
| `Results.Ok(...)` | HTTP 200 + JSON. |
| `Results.NotFound()` | HTTP 404 sin body. |
| `return group` | Permite encadenar más extensiones. |

### 6.2 Ejemplo: auth con validación

```csharp
auth.MapPost("/login", async (LoginRequest req, IAuthService service, CancellationToken ct) =>
{
    var result = await service.LoginAsync(req.Email, req.Password, ct);
    return result is null ? Results.Unauthorized() : Results.Ok(result);
}).WithValidation<LoginRequest>();
```

| Pieza | Qué hace |
|-------|----------|
| `LoginRequest req` | Body JSON → objeto C#. |
| `.WithValidation<LoginRequest>()` | Añade filtro que valida antes del handler (sección 7). |
| `Results.Unauthorized()` | HTTP 401. |

### 6.3 Rutas protegidas con JWT

```csharp
// Todo el grupo exige usuario autenticado
var orders = group.MapGroup("/orders").RequireAuthorization();

// Admin: autenticado + permiso concreto
admin.MapPost("/catalog/families", ...)
    .RequireAuthorization(AdminPermissions.FamiliesManage)
    .WithValidation<SaveFamilyRequest>();
```

| Método | Efecto |
|--------|--------|
| `.RequireAuthorization()` | Necesita JWT válido (cualquier usuario logueado). |
| `.RequireAuthorization("admin.products.manage")` | JWT + claim `permission` con ese valor. |

### 6.4 Listado de archivos Endpoints

| Archivo | Rutas bajo `/api/v1` | Auth |
|---------|----------------------|------|
| `AuthEndpoints.cs` | `/auth/*` | login/register públicos; logout/me con token |
| `CatalogEndpoints.cs` | `/catalog/*` | Público |
| `CartEndpoints.cs` | `/cart/*` | Opcional (guest o usuario) |
| `CheckoutEndpoints.cs` | `/checkout` | Requiere login |
| `OrderEndpoints.cs` | `/orders/*` | Requiere login |
| `AdminEndpoints.cs` | `/admin/*` | JWT + permisos admin |

---

## 7. Filtros de endpoint — `ValidationFilter`

Los **endpoint filters** en Minimal APIs son como “middleware solo para una ruta”.

**`Filters/ValidationFilter.cs`:**

1. Toma el body ya convertido a `T` (ej. `LoginRequest`).  
2. Busca `IValidator<T>` (FluentValidation).  
3. Si hay errores → `Results.ValidationProblem` (HTTP 400 con detalles).  
4. Si todo bien → ejecuta el handler.

**`ValidationExtensions.cs`:**

```csharp
public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) =>
    builder.AddEndpointFilter<ValidationFilter<T>>();
```

**Validadores** viven en `Application/Validators/`:

```csharp
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

Flujo: **Postman envía JSON → deserializa → ValidationFilter → tu handler**.

---

## 8. Middleware y helpers de la Api

### 8.1 `ExceptionMiddleware`

- Envuelve `await next(context)`.  
- Si algo lanza `NotFoundException` → 404 con mensaje.  
- Si `InsufficientStockException` → 409.  
- Si error genérico → 500 `"Error interno del servidor"` (en Development puede mostrar más detalle).

### 8.2 `HttpContextExtensions`

```csharp
ctx.GetUserId()      // Guid del JWT (claim NameIdentifier)
ctx.GetGuestToken()  // Guid del header X-Guest-Token (carrito invitado)
```

Usado en `AuthEndpoints` (logout), `CartEndpoints`, etc.

---

## 9. JWT en `Program.cs` (resumen)

1. **Login** genera token con `JwtTokenService`.  
2. Cliente envía: `Authorization: Bearer eyJhbG...`  
3. `AddJwtBearer` valida firma, issuer, audience, expiración.  
4. `UseAuthentication` llena `HttpContext.User` con claims.  
5. `RequireAuthorization` comprueba políticas/permisos.

Config en `appsettings`: sección `Jwt` (Secret, Issuer, Audience, minutos).

Más detalle: [04-autenticacion-y-permisos.md](./04-autenticacion-y-permisos.md)

---

## 10. Estructura de carpetas del repositorio

```
ecommerce-api/
├── src/
│   ├── Ecommerce.Api/           ← Program, Endpoints, Middleware, Filters
│   ├── Ecommerce.Application/   ← Services, DTOs, Validators, DI
│   ├── Ecommerce.Infrastructure/← EF, Repositories, JWT, PDF, DI
│   └── Ecommerce.Domain/        ← Entities, Enums, Exceptions
├── tests/
├── docs/
├── postman/
└── scriptsSql/
```

---

## 11. Las cuatro capas (resumen)

### Domain — recetario del negocio

`Entities/`, `Emums/`, `Exceptions/`, `BaseEntity.cs`. Sin referencias a otras capas.

### Application — cocina

`Services/` (casos de uso), `Abstractions/` (interfaces), `DTOs/`, `Validators/`, `DependencyInjection.cs`.

### Infrastructure — almacén técnico

`EcommerceDbContext`, `Repositories/`, `DbSeeder`, `DatabaseBootstrap`, `JwtTokenService`, `PdfTicketGenerator`, `DependencyInjection.cs`.

### Api — mesero HTTP

`Program.cs`, `Endpoints/`, `Filters/`, `Middleware/`, `Extensions/`, `appsettings*.json`.

---

## 12. Flujo completo: Login en Minimal API

```
POST /api/v1/auth/login
Body: { "email": "...", "password": "..." }

1. Kestrel recibe HTTP
2. ExceptionMiddleware (try)
3. CORS, Auth (login es público → no exige token)
4. Endpoint filter: ValidationFilter<LoginRequest>
5. Handler en AuthEndpoints:
      parámetros inyectados: LoginRequest, IAuthService, CancellationToken
6. AuthService → UserRepository → EF → tabla users
7. BCrypt verifica password; JwtTokenService crea tokens
8. Results.Ok(LoginResponse) → JSON 200
```

---

## 13. Configuración (`appsettings` y perfiles)

| Archivo | Uso |
|---------|-----|
| `appsettings.json` | Base (JWT, logging). |
| `appsettings.SqlServer.json` | LocalDB, puerto 5088. |
| `appsettings.Sqlite.json` | Archivo `.db`, puerto 5089. |
| `launchSettings.json` | Perfil Visual Studio + `ASPNETCORE_ENVIRONMENT`. |

---

## 14. Base de datos, tests, Postman

- **Automática:** `DatabaseBootstrap` + `DbSeeder` al arrancar.  
- **Manual:** `scriptsSql/schema.sqlserver.sql` + `seed.sqlserver.sql`.  
- **Tests:** `dotnet test` en `Ecommerce.IntegrationTests`.  
- **Postman:** `postman/` — colección con sesión automática.

Usuarios demo: `admin@ecommerce.local` / `Admin123!`, `cliente@ecommerce.local` / `Cliente123!`.

---

## 15. Orden para leer código (Minimal API primero)

1. **`Program.cs`** — builder, middleware, `MapGroup`, `Run`.  
2. **`Application/DependencyInjection.cs`** + **`Infrastructure/DependencyInjection.cs`**.  
3. **`Endpoints/AuthEndpoints.cs`** — ruta + DI + validación.  
4. **`Filters/ValidationFilter.cs`**.  
5. **`Application/Services/AuthService.cs`**.  
6. **`Endpoints/CatalogEndpoints.cs`** — ejemplo público simple.  
7. **`Endpoints/AdminEndpoints.cs`** — permisos + muchas rutas.  
8. **`Application/Services/CheckoutService.cs`** — lógica más compleja.

---

## 16. Glosario

| Término | Significado |
|---------|-------------|
| **Minimal API** | Ruta definida con `MapGet`/`MapPost` sin Controller. |
| **Handler** | Función lambda/async que ejecuta la ruta. |
| **RouteGroupBuilder** | Agrupador de rutas con prefijo común. |
| **Endpoint filter** | Código antes/después del handler de una ruta. |
| **IServiceCollection** | Contenedor donde registras DI en `builder.Services`. |
| **WebApplication** | App lista para middleware y rutas. |
| **Results.Ok** | Helper para respuestas HTTP tipadas. |
| **DTO** | Objeto de transferencia para API. |
| **Scoped** | Una instancia por request HTTP. |

---

## 17. Preguntas frecuentes

**¿Dónde pongo una ruta nueva?**  
Crea o edita un archivo en `Endpoints/`, registra el método en `Program.cs` con `api.MapTuEndpoints()`.

**¿Por qué `IAuthService` y no `AuthService` en el endpoint?**  
Para desacoplar: la Api no depende de la implementación concreta.

**¿Qué es `CancellationToken` en el handler?**  
Lo inyecta ASP.NET; pásalo a servicios/repositorios para cancelar si el cliente aborta.

**¿Controllers vs Minimal?**  
Este proyecto es 100 % Minimal APIs en `Endpoints/`.

**Error 500 en Postman**  
API corriendo, `baseUrl` correcto, BD con esquema actual (reinicia API o `scriptsSql`).

---

## Siguiente paso

- [01-arquitectura.md](./01-arquitectura.md) — diagrama de capas  
- [03-api-endpoints.md](./03-api-endpoints.md) — listado de rutas  
- [06-flujos-de-negocio.md](./06-flujos-de-negocio.md) — checkout, pago, despacho  
