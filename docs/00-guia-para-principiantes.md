# Guía para principiantes — Código, Minimal APIs, CQRS y arquitectura

Esta guía está pensada para alguien que **empieza en .NET** y en **Minimal APIs**, y quiere entender **qué hace cada archivo y cada pieza importante** del proyecto **Ecommerce API**.

Si ya tienes experiencia, usa el [índice técnico](./README.md).

---

## 1. ¿Qué es este proyecto?

Es una **API REST**: un programa que escucha peticiones HTTP (Postman, React, móvil…) y responde **JSON**.

| Área | Qué hace |
|------|----------|
| **Tienda (público)** | Home, portadas, catálogo por slug, búsqueda y filtros |
| **Carrito** | Usuario logueado o invitado (`X-Guest-Token`) |
| **Direcciones** | CRUD del cliente |
| **Checkout y pedidos** | Crear pedido, reservar stock, pago simulado |
| **Admin** | Dashboard, covers, catálogo, inventario, pedidos, envíos, PDF |

**Tecnología:** C#, **.NET 10**, **Minimal APIs**, **MediatR** (CQRS), **FluentValidation**, **FluentResults**, **EF Core**, **SQL Server** o **SQLite**.

**URL base:** `http://localhost:5088/api/v1` (perfil SqlServer en `launchSettings.json`).

---

## 2. Conceptos básicos de .NET

| Concepto | Qué es |
|----------|--------|
| **Solución (.sln)** | Agrupa varios proyectos (`Api`, `Application`, `Domain`, `Infrastructure`, tests). |
| **Proyecto (.csproj)** | Un módulo que compila a DLL. |
| **Clase / archivo .cs** | Código C#. |
| **Namespace** | Organización lógica (`Ecommerce.Application.Features.Auth`). |
| **Interface** | Contrato (`IUserRepository` = “algo que lee/escribe usuarios en BD”). |
| **record** | Tipo inmutable ideal para DTOs y commands (`LoginCommand`, `LoginResponse`). |
| **Dependency Injection (DI)** | .NET **crea y entrega** dependencias automáticamente en cada petición. |
| **async / await** | Operaciones que esperan BD/red sin bloquear el hilo del servidor. |

---

## 3. ¿Qué son Minimal APIs?

En .NET “clásico” (MVC) tenías **Controllers** con métodos como `[HttpPost] Login()`.

En **Minimal APIs** defines rutas **directamente**, sin clase Controller:

```csharp
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/auth/login", async (LoginRequest req, ISender sender, CancellationToken ct) =>
    (await sender.Send(new LoginCommand(req.Email, req.Password), ct)).ToHttpResult());
```

| Minimal APIs | Controllers MVC |
|--------------|-----------------|
| Rutas en `Program.cs` o `Endpoints/*.cs` | Rutas en `XController` |
| Menos ceremonia | Más estructura en apps muy grandes |
| Ideal para APIs medianas | Muy usado en proyectos enterprise antiguos |

**En este proyecto:** no hay carpeta `Controllers/`. Todas las rutas están en `Ecommerce.Api/Endpoints/*.cs`.

---

## 4. CQRS y MediatR (cómo está organizada la lógica hoy)

**CQRS** (*Command Query Responsibility Segregation*) separa:

| Lado | Qué hace | Ejemplo en el repo |
|------|----------|-------------------|
| **Query** (lectura) | Solo leer datos, devolver DTOs | `GetFamiliesQuery`, `ListMyOrdersQuery` |
| **Command** (escritura) | Crear, actualizar, borrar, transacciones | `LoginCommand`, `CreateOrderCommand`, `SaveFamilyCommand` |

**MediatR** es la librería que **envía** cada command/query al handler correcto:

```
Endpoint  →  sender.Send(new LoginCommand(...))  →  LoginCommandHandler  →  repositorios / JWT
```

| Pieza | Archivo típico | Rol |
|-------|----------------|-----|
| **Command / Query** | `Features/Auth/AuthHandlers.cs` | Mensaje con los datos de la operación |
| **Handler** | Mismo archivo o `*Handler.cs` | Ejecuta la lógica y devuelve `Result` o `Result<T>` |
| **ISender** | Inyectado en endpoints | Es el “mediador”; llama a `Send(...)`. |

**No hay carpeta `Services/`** con `AuthService`, `CartService`, etc. La lógica vive en **`Application/Features/`**.

### Mapa de módulos en `Features/`

| Carpeta | Commands / Queries (resumen) |
|---------|------------------------------|
| `Auth/` | Login, register, refresh, logout, me |
| `Catalog/Queries/` | Home, covers, familias, slugs, productos, búsqueda |
| `Cart/` | Get, add, update, remove, clear, merge |
| `Checkout/` | CreateOrder |
| `Orders/` | List, get, pay (cliente) |
| `Addresses/` | CRUD + default |
| `Admin/` | Dashboard, covers, catálogo CRUD, inventario, pedidos, envíos, opciones |

---

## 5. FluentResults — errores de negocio sin excepciones

En lugar de `throw` para “email ya existe” o “pedido no encontrado”, los handlers devuelven **`Result`** o **`Result<T>`**:

```csharp
// Handler
if (user is null)
    return Result.Fail<LoginResponse>(AuthErrors.InvalidCredentials());
return Result.Ok(await BuildLoginResponseAsync(user, ...));
```

```csharp
// Endpoint
(await sender.Send(new LoginCommand(email, password), ct)).ToHttpResult();
```

**`ToHttpResult()`** (`Api/Extensions/ResultExtensions.cs`) traduce el código de error a HTTP:

| Metadata `Code` | HTTP |
|-----------------|------|
| `Unauthorized` | 401 |
| `NotFound`, `Address.NotFound`, `Catalog.NotFound` | 404 |
| `Validation` | 400 (ValidationProblem) |
| `Conflict`, `InsufficientStock` | 409 |
| (otros) | 400 |

Las **excepciones** (`NotFoundException`, errores de BD) siguen yendo al **`ExceptionMiddleware`** → 404/409/500.

Errores reutilizables viven en **Domain**, por ejemplo:

- `Domain/Auth/AuthErrors.cs`
- `Domain/Orders/OrderErrors.cs`
- `Domain/Admin/AdminErrors.cs`
- `Domain/Addresses/AddressErrors.cs`

---

## 6. `Program.cs` — el corazón de la aplicación

`Program.cs` es el **punto de entrada**. Hace dos fases:

1. **Configurar servicios** (`WebApplicationBuilder` → `builder.Services`) — antes de arrancar.
2. **Configurar el pipeline HTTP** (`WebApplication` → `app`) — middleware, rutas, `Run`.

### 6.1 Fase builder

| Bloque | Qué hace |
|--------|----------|
| `WebApplication.CreateBuilder(args)` | Lee `appsettings.json`, entorno, argumentos. |
| `UseSerilog(...)` | Logs en consola y `logs/ecommerce-*.log`. |
| `AddApplication()` | MediatR, FluentValidation, `ValidationBehavior`. |
| `AddInfrastructure(...)` | EF Core, repositorios, JWT, PDF. |
| `AddOpenApi()` | Documentación OpenAPI (Scalar). |
| `AddCors("Web")` | Frontend en `localhost:3000` (configurable). |
| `AddAuthentication` + `AddJwtBearer` | Valida JWT en rutas protegidas. |
| `AddAuthorization` + políticas | Una política por permiso admin (`admin.products.manage`, …). |

### 6.2 Fase app

| Línea | Qué hace |
|-------|----------|
| `DatabaseBootstrap.InitializeAsync` | Crea tablas si faltan, corrige esquema viejo, **seed** (usuarios demo). |
| `UseMiddleware<ExceptionMiddleware>` | Errores no controlados → JSON HTTP. |
| `MapOpenApi` + Scalar | Solo fuera de producción: `http://localhost:5088/scalar/v1`. |
| `UseSerilogRequestLogging` | Log por request. |
| `UseCors` → `UseAuthentication` → `UseAuthorization` | **Orden importa.** |
| `MapGroup("/api/v1")` + `Map*Endpoints()` | Registra todas las rutas de la API. |
| `RunAsync()` | Kestrel escucha peticiones. |

### 6.3 Orden del pipeline HTTP

```
Petición
  → ExceptionMiddleware
  → SerilogRequestLogging
  → CORS
  → Authentication (JWT)
  → Authorization (permisos)
  → Endpoint (MapGet / MapPost)
  → Respuesta
```

### 6.4 Rutas registradas

```csharp
var api = app.MapGroup("/api/v1");
api.MapAuthEndpoints();
api.MapCatalogEndpoints();
api.MapCartEndpoints();
api.MapAddressEndpoints();
api.MapCheckoutEndpoints();
api.MapOrderEndpoints();
api.MapAdminEndpoints();
```

También existen **`GET /health`** y **`GET /ready`** (sin prefijo `/api/v1`).

### 6.5 `public partial class Program;`

Permite que los **tests de integración** arranquen la misma app en memoria (`WebApplicationFactory`).

---

## 7. Inyección de dependencias (DI)

### 7.1 ¿Qué problema resuelve?

Sin DI tendrías que instanciar repositorios y handlers a mano en cada ruta. Con DI el endpoint solo pide lo que necesita:

```csharp
catalog.MapGet("/families", async (ISender sender, CancellationToken ct) =>
    (await sender.Send(new GetFamiliesQuery(), ct)).ToHttpResult());
```

### 7.2 ¿Dónde se registran?

**`Application/DependencyInjection.cs`**

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

- **MediatR** descubre todos los handlers en `Application`.
- **FluentValidation** registra validadores de **commands** (no solo DTOs del body).
- **`ValidationBehavior`** valida **antes** de ejecutar cada handler.

**`Infrastructure/DependencyInjection.cs`**

```csharp
services.AddDbContextPool<EcommerceDbContext>(...);
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ICatalogReadRepository, CatalogReadRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
// ... más repositorios
services.AddScoped<IJwtTokenService, JwtTokenService>();
services.AddScoped<IPdfTicketGenerator, PdfTicketGenerator>();
```

| Registro | Significado |
|----------|-------------|
| `AddScoped<IUserRepository, UserRepository>()` | Una instancia del repositorio **por petición HTTP**. |
| `AddDbContextPool<EcommerceDbContext>` | Contexto EF optimizado (pool). |
| `AddTransient<ValidationBehavior>` | Pipeline MediatR: se ejecuta en cada `Send`. |

**`ISender`** lo registra MediatR automáticamente; es lo que inyectas en los endpoints.

### 7.3 ¿Qué se puede inyectar en un endpoint?

| Parámetro | Origen |
|-----------|--------|
| `ISender sender` | MediatR |
| `LoginRequest req` | Body JSON |
| `string slug` | Segmento de URL |
| `HttpContext ctx` | Petición actual (claims, headers) |
| `CancellationToken ct` | Cancelación si el cliente cierra |
| `Guid orderId` | Ruta `{orderId:guid}` |

**Regla:** la capa **Api** no usa `EcommerceDbContext` en rutas de negocio (solo `/ready` y health usan BD directamente).

### 7.4 Flujo de dependencias (ejemplo login)

```
AuthEndpoints
    → ISender.Send(LoginCommand)
        → LoginCommandHandler
            → IUserRepository (Infrastructure)
            → IJwtTokenService (Infrastructure)
                → EcommerceDbContext
```

La **Api** solo conoce **Application** (commands, DTOs, `ISender`). **Infrastructure** implementa repositorios y JWT.

---

## 8. Archivos `Endpoints/` — anatomía de una ruta

Cada archivo es una **clase estática** con `MapXEndpoints(this RouteGroupBuilder group)`.

### 8.1 Catálogo público (solo lectura)

```csharp
catalog.MapGet("/families", async (ISender sender, CancellationToken ct) =>
    (await sender.Send(new GetFamiliesQuery(), ct)).ToHttpResult());

catalog.MapGet("/products/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
    (await sender.Send(new GetProductBySlugQuery(slug), ct)).ToHttpResult());
```

| Pieza | Qué hace |
|-------|----------|
| `MapGroup("/catalog")` | Prefijo `/api/v1/catalog`. |
| `WithTags("Catalog")` | Agrupa en Scalar/OpenAPI. |
| `ToHttpResult()` | 200 con JSON o error 404/400 según `Result`. |

### 8.2 Auth

```csharp
auth.MapPost("/login", async (LoginRequest req, ISender sender, CancellationToken ct) =>
    (await sender.Send(new LoginCommand(req.Email, req.Password), ct)).ToHttpResult());
```

El **body** sigue siendo `LoginRequest` (DTO). El endpoint **mapea** a `LoginCommand` para el handler.

**No hace falta** `.WithValidation<LoginRequest>()` en la ruta: la validación ocurre en el **pipeline MediatR** (`LoginCommandValidator`).

### 8.3 PDF (respuesta distinta a JSON)

```csharp
(await sender.Send(new GenerateShipmentTicketPdfQuery(shipmentId), ct))
    .ToHttpResult(pdf => Results.File(pdf, "application/pdf", $"ticket-{shipmentId}.pdf"));
```

Si el `Result` falla → mismo mapeo de errores; si ok → archivo PDF.

### 8.4 Rutas protegidas

```csharp
var orders = group.MapGroup("/orders").RequireAuthorization();

admin.MapPost("/catalog/families", ...)
    .RequireAuthorization(AdminPermissions.FamiliesManage);
```

| Método | Efecto |
|--------|--------|
| `.RequireAuthorization()` | JWT válido (cualquier usuario). |
| `.RequireAuthorization("admin.products.manage")` | JWT + claim `permission`. |

### 8.5 Tabla de archivos Endpoints

| Archivo | Prefijo | Auth |
|---------|---------|------|
| `AuthEndpoints.cs` | `/auth` | login/register/refresh públicos; logout/me con JWT |
| `CatalogEndpoints.cs` | `/catalog` | Público |
| `CartEndpoints.cs` | `/cart` | Opcional (guest o usuario) |
| `AddressEndpoints.cs` | `/addresses` | JWT obligatorio |
| `CheckoutEndpoints.cs` | `/checkout` | JWT obligatorio |
| `OrderEndpoints.cs` | `/orders` | JWT obligatorio |
| `AdminEndpoints.cs` | `/admin` | JWT + permisos granulares |

---

## 9. Validación — dos capas

### 9.1 Entrada HTTP: FluentValidation en el pipeline (principal)

1. El endpoint llama `sender.Send(command)`.
2. **`ValidationBehavior`** busca `IValidator<TCommand>`.
3. Si hay errores → `Result.Fail` con código `Validation` → HTTP 400.
4. Si ok → ejecuta el **handler**.

Validadores viven junto al módulo, por ejemplo:

- `Features/Auth/Validators/AuthCommandValidators.cs`
- `Features/Cart/Validators/CartCommandValidators.cs`
- `Features/Admin/Validators/AdminCommandValidators.cs`

```csharp
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

### 9.2 Dominio: reglas de negocio

Además del JSON bien formado, existen reglas en **Domain** (invariantes):

- `AddressRules` — longitudes máximas de dirección.
- `OrderErrors`, `CartErrors`, `AuthErrors`, `AdminErrors` — mensajes y códigos para `Result`.

Ejemplo: una dirección puede pasar FluentValidation pero fallar una regla de dominio en el handler.

### 9.3 Filtro `ValidationFilter` (legacy, opcional)

En `Api/Filters/ValidationFilter.cs` existe `.WithValidation<T>()` para validar **DTOs del body** en la capa HTTP.

**Hoy casi ningún endpoint lo usa:** la validación está en el **pipeline MediatR**. El filtro queda por compatibilidad o rutas futuras que no pasen por MediatR.

---

## 10. Middleware y extensiones de la Api

### 10.1 `ExceptionMiddleware`

- `NotFoundException` → 404  
- `InsufficientStockException` → 409  
- `InvalidOperationException` → 400  
- Otros → 500 (`Error interno del servidor` en producción)

Los flujos nuevos prefieren **`Result`** en handlers; las excepciones cubren fallos inesperados o código legacy.

### 10.2 `HttpContextExtensions`

```csharp
ctx.GetUserId()      // Guid del JWT (claim NameIdentifier)
ctx.GetGuestToken()  // Guid del header X-Guest-Token (carrito invitado)
```

Usado en carrito, checkout, pedidos, logout.

### 10.3 Carrito invitado

| Header | Uso |
|--------|-----|
| `Authorization: Bearer ...` | Usuario logueado; carrito por `userId`. |
| `X-Guest-Token: {guid}` | Carrito invitado; si falta, la API puede crear uno nuevo. |

Tras login: `POST /cart/merge` con `{ "guestToken": "..." }`.

---

## 11. JWT (resumen)

1. **Login** → `LoginCommandHandler` genera access + refresh token.  
2. Cliente envía: `Authorization: Bearer eyJhbG...`  
3. `AddJwtBearer` valida firma, issuer, audience, expiración.  
4. `UseAuthentication` rellena `HttpContext.User`.  
5. `RequireAuthorization` comprueba políticas/permisos.

Config en `appsettings`: sección `Jwt` (Secret, Issuer, Audience, minutos).

Más detalle: [04-autenticacion-y-permisos.md](./04-autenticacion-y-permisos.md).

---

## 12. Estructura de carpetas del repositorio

```
ecommerce-api/
├── src/
│   ├── Ecommerce.Api/
│   │   ├── Program.cs
│   │   ├── Endpoints/          ← rutas Minimal API
│   │   ├── Extensions/         ← ToHttpResult, HttpContext
│   │   ├── Filters/            ← ValidationFilter (opcional)
│   │   └── Middleware/
│   ├── Ecommerce.Application/
│   │   ├── Features/           ← CQRS: handlers, commands, queries
│   │   ├── Common/Behaviors/   ← ValidationBehavior
│   │   ├── Abstractions/       ← IUserRepository, IJwtTokenService, …
│   │   ├── DTOs/
│   │   └── Authorization/
│   ├── Ecommerce.Infrastructure/
│   │   ├── Persistence/Sql/    ← DbContext, Repositories, Seed
│   │   ├── Identity/           ← JwtTokenService
│   │   └── Documents/          ← PDF tickets
│   └── Ecommerce.Domain/
│       ├── Entities/
│       ├── Emums/
│       ├── Auth/, Orders/, Cart/, Addresses/, Admin/  ← Errors, Rules
│       └── Exceptions/
├── tests/
│   ├── Ecommerce.IntegrationTests/   ← flujo compra, auth, 403
│   └── Ecommerce.UnitTests/
├── docs/                       ← esta guía y el índice técnico
├── postman/                    ← colección + entorno local
└── scriptsSql/                 ← schema y seed manual
```

### Dentro de `Application/Features/` (detalle)

```
Features/
├── Auth/
│   ├── AuthHandlers.cs
│   └── Validators/
├── Catalog/Queries/
├── Cart/
├── Checkout/
├── Orders/
├── Addresses/
│   ├── Commands/
│   ├── Queries/
│   └── Validators/
└── Admin/
    ├── AdminHandlers.cs
    └── Validators/
```

---

## 13. Las cuatro capas (Clean Architecture)

```
┌─────────────────────────────────────┐
│  Api          HTTP, JWT, endpoints  │
└──────────────────┬──────────────────┘
                   │ ISender, DTOs
┌──────────────────▼──────────────────┐
│  Application  Handlers, validators  │
└──────────────────┬──────────────────┘
                   │ interfaces
┌──────────────────▼──────────────────┐
│  Infrastructure  EF, repos, JWT, PDF │
└──────────────────┬──────────────────┘
                   │
┌──────────────────▼──────────────────┐
│  Domain       Entidades, reglas      │
└─────────────────────────────────────┘
```

| Capa | Contenido clave |
|------|-----------------|
| **Domain** | Entidades, enums, `*Errors`, `AddressRules`. Sin referencias a otras capas. |
| **Application** | Commands/queries, handlers, `I*Repository`, DTOs. Sin EF directo. |
| **Infrastructure** | `EcommerceDbContext`, implementación de repos, seed, bootstrap. |
| **Api** | `Program.cs`, endpoints finos, `ToHttpResult`, middleware. |

---

## 14. Flujo completo: login

```
POST /api/v1/auth/login
Body: { "email": "cliente@ecommerce.local", "password": "Cliente123!" }

1. Kestrel recibe HTTP
2. ExceptionMiddleware (try)
3. CORS — ruta pública, sin JWT obligatorio
4. AuthEndpoints: deserializa LoginRequest
5. sender.Send(new LoginCommand(...))
6. ValidationBehavior → LoginCommandValidator
7. LoginCommandHandler:
      UserRepository.GetByEmailWithRolesAsync
      BCrypt.Verify(password)
      JwtTokenService → access + refresh
      guardar hash refresh en BD
8. Result.Ok(LoginResponse)
9. ToHttpResult() → HTTP 200 + JSON
```

Si el password falla → `AuthErrors.InvalidCredentials()` → **401 Unauthorized**.

---

## 15. Flujo completo: compra (cliente)

```
1. POST /auth/login        → accessToken
2. GET  /catalog/products/audifonos-pro-x
3. POST /cart/items        { variantId, quantity }
4. POST /addresses         (opcional)
5. POST /checkout          { addressId, shippingCost }
       → CreateOrderCommand + transacción + reserva stock
6. POST /orders/{id}/pay   → PayOrderCommand + commit stock
```

Diagrama y estados: [06-flujos-de-negocio.md](./06-flujos-de-negocio.md).

---

## 16. Flujo admin: pedido listo para despacho

```
1. Pedido en estado Paid
2. PATCH /admin/orders/{id}/ready-to-dispatch  → ReadyToDispatch
3. POST /admin/shipments                       → Shipment + ticket
4. GET  /admin/shipments/{id}/ticket.pdf       → PDF
5. PATCH .../in-transit  y  .../delivered
```

---

## 17. Configuración, base de datos, tests, Postman

### 17.1 `appsettings`

| Archivo | Uso |
|---------|-----|
| `appsettings.json` | Base (JWT, logging, CORS). |
| `appsettings.SqlServer.json` | LocalDB, puerto **5088**. |
| `appsettings.Sqlite.json` | Archivo `.db`, puerto **5089** (tests). |
| `launchSettings.json` | Perfil VS + `ASPNETCORE_ENVIRONMENT`. |

`Persistence:Provider` → `SqlServer` o `Sqlite`.

### 17.2 Base de datos

- **Automática:** `DatabaseBootstrap` + `DbSeeder` al arrancar la API.  
- **Manual:** `scriptsSql/schema.sqlserver.sql` + `seed.sqlserver.sql`.

### 17.3 Tests

```bash
cd ecommerce-api
dotnet test
```

| Proyecto | Qué prueba |
|----------|------------|
| `Ecommerce.IntegrationTests` | Health, login admin, 403 dashboard cliente, flujo catálogo → carrito → checkout → pay |
| `Ecommerce.UnitTests` | Placeholder |

### 17.4 Postman

Carpeta `postman/`:

- `Ecommerce-API.postman_collection.json` — rutas con scripts de sesión (guarda tokens).  
- `Ecommerce-Local.postman_environment.json` — `baseUrl`, credenciales demo.

Lista de rutas: [03-api-endpoints.md](./03-api-endpoints.md).

### 17.5 Usuarios demo (seed)

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |

Producto demo: slug `audifonos-pro-x`, SKU `APX-001`.

---

## 18. Orden recomendado para leer código

| Paso | Archivo | Por qué |
|------|---------|---------|
| 1 | `Api/Program.cs` | Pipeline, DI, registro de rutas |
| 2 | `Application/DependencyInjection.cs` | MediatR + validación |
| 3 | `Infrastructure/DependencyInjection.cs` | Repositorios y BD |
| 4 | `Api/Endpoints/AuthEndpoints.cs` | Endpoint fino + `ISender` |
| 5 | `Application/Features/Auth/AuthHandlers.cs` | Handler + `Result` |
| 6 | `Application/Common/Behaviors/ValidationBehavior.cs` | Pipeline |
| 7 | `Api/Extensions/ResultExtensions.cs` | Result → HTTP |
| 8 | `Api/Endpoints/CatalogEndpoints.cs` | Solo queries |
| 9 | `Application/Features/Cart/CartHandlers.cs` | Escritura + guest |
| 10 | `Application/Features/Checkout/CheckoutHandlers.cs` | Transacción |
| 11 | `Api/Endpoints/AdminEndpoints.cs` | Permisos admin |
| 12 | `Infrastructure/Persistence/Sql/Repositories/CatalogReadRepository.cs` | Proyecciones EF en lecturas |

---

## 19. Glosario ampliado

| Término | Significado |
|---------|-------------|
| **Minimal API** | Ruta con `MapGet`/`MapPost` sin Controller. |
| **Handler** | Clase que ejecuta un command/query (`IRequestHandler`). |
| **Command** | Operación que cambia estado (POST, PUT, DELETE lógicos). |
| **Query** | Operación de solo lectura (GET). |
| **MediatR** | Librería que enruta `Send(command)` al handler. |
| **ISender** | Interfaz del mediador inyectada en endpoints. |
| **FluentResults `Result`** | Éxito o lista de errores sin excepción. |
| **DTO** | Objeto para JSON de entrada/salida. |
| **Repositorio** | Abstracción de acceso a datos (`IOrderRepository`). |
| **Read repository** | Solo lecturas optimizadas (`ICatalogReadRepository`). |
| **Proyección EF** | `Select` en LINQ para traer solo campos del DTO. |
| **Scoped** | Una instancia por petición HTTP. |
| **Seed** | Datos iniciales al crear la BD. |
| **JWT** | Token firmado que identifica al usuario. |
| **Claim** | Dato dentro del JWT (email, permisos). |
| **RouteGroupBuilder** | Prefijo común de rutas (`/api/v1/catalog`). |

---

## 20. Preguntas frecuentes

**¿Dónde pongo una ruta nueva?**  
En `Endpoints/`, crea el command/query + handler en `Features/`, registra el endpoint y, si hace falta, un validador.

**¿Por qué `ISender` y no el handler directo en el endpoint?**  
El endpoint no debe conocer la implementación; MediatR resuelve el handler y ejecuta el pipeline (validación).

**¿Dónde está la lógica de login?**  
`Application/Features/Auth/AuthHandlers.cs` → `LoginCommandHandler`, no en un `AuthService`.

**¿Qué es `CancellationToken`?**  
Lo inyecta ASP.NET; pásalo a `Send(..., ct)` y a repositorios para cancelar operaciones largas.

**Login devuelve 401 sin body detallado**  
Es intencional para credenciales inválidas (`Unauthorized`). Register con email duplicado → **409 Conflict**.

**Error 500 en Postman**  
Comprueba API en marcha, `baseUrl` correcto, BD con esquema actual (reinicia API o ejecuta `scriptsSql`).

**¿Controllers o Minimal?**  
100 % Minimal APIs en este repo.

**¿Cómo depuro un handler?**  
Pon breakpoint en el `Handle` del handler, llama la ruta desde Postman o Scalar.

**¿Dónde están los permisos admin?**  
`Application/Authorization/AdminPermissions.cs` + `.RequireAuthorization(...)` en `AdminEndpoints.cs`.

---

## 21. Documentación relacionada

| Documento | Contenido |
|-----------|-----------|
| [01-arquitectura.md](./01-arquitectura.md) | Diagrama de capas y carpetas |
| [02-configuracion-y-ejecucion.md](./02-configuracion-y-ejecucion.md) | Arrancar API, perfiles BD |
| [03-api-endpoints.md](./03-api-endpoints.md) | Listado completo de rutas |
| [04-autenticacion-y-permisos.md](./04-autenticacion-y-permisos.md) | JWT, refresh, permisos |
| [05-dominio-y-base-de-datos.md](./05-dominio-y-base-de-datos.md) | Entidades y relaciones |
| [06-flujos-de-negocio.md](./06-flujos-de-negocio.md) | Checkout, stock, despacho |
| [08-evolucion-cqrs-y-dominio.md](./08-evolucion-cqrs-y-dominio.md) | Decisiones CQRS y plan histórico |

---

## Siguiente paso práctico

1. Arranca la API (perfil SqlServer).  
2. Abre Scalar: `http://localhost:5088/scalar/v1`.  
3. Importa Postman desde `postman/`.  
4. Ejecuta **Login admin** → **Dashboard stats** → flujo cliente en la colección.  
5. Lee `AuthHandlers.cs` y sigue el flujo con el depurador.
