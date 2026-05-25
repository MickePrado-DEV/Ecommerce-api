# Arquitectura

## Diagrama de capas

```
┌─────────────────────────────────────────────────────────┐
│  Ecommerce.Api                                          │
│  Endpoints (Minimal APIs), Middleware, ToHttpResult     │
└──────────────────────────┬──────────────────────────────┘
                           │ ISender, DTOs
┌──────────────────────────▼──────────────────────────────┐
│  Ecommerce.Application                                  │
│  Features (CQRS/MediatR), Behaviors, DTOs, Abstractions │
└──────────────────────────┬──────────────────────────────┘
                           │ interfaces
┌──────────────────────────▼──────────────────────────────┐
│  Ecommerce.Infrastructure                               │
│  EF Core, Repositorios, JWT, PDF, DbSeeder              │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│  Ecommerce.Domain                                       │
│  Entidades, Enums, *Errors, reglas de dominio           │
└─────────────────────────────────────────────────────────┘
```

## Proyectos y responsabilidad

### `Ecommerce.Api`
Punto de entrada HTTP.

| Carpeta / archivo | Qué hace |
|-------------------|----------|
| `Program.cs` | Pipeline: Serilog, CORS, JWT, Scalar, bootstrap BD, `MapGroup("/api/v1")` |
| `Endpoints/` | Rutas por área; inyectan `ISender` y llaman `ToHttpResult()` |
| `Extensions/ResultExtensions.cs` | Mapea `FluentResults` → 200/400/401/404/409 |
| `Filters/ValidationFilter.cs` | Validación opcional en capa HTTP (legacy; no usada en rutas actuales) |
| `Middleware/ExceptionMiddleware.cs` | Excepciones → JSON HTTP |
| `Extensions/HttpContextExtensions.cs` | `GetUserId()`, `GetGuestToken()` |

### `Ecommerce.Application`
Lógica de aplicación sin EF ni HTTP.

| Carpeta | Qué hace |
|---------|----------|
| `Features/` | **CQRS:** commands, queries y handlers (MediatR) por módulo |
| `Common/Behaviors/ValidationBehavior.cs` | FluentValidation antes de cada handler |
| `Abstractions/` | `I*Repository`, `IJwtTokenService`, `IPdfTicketGenerator`, `IUnitOfWork` |
| `DTOs/` | Contratos JSON de entrada/salida |
| `Features/*/Validators/` | Validadores de commands (FluentValidation) |
| `Authorization/AdminPermissions.cs` | Constantes de permisos admin |

**Módulos CQRS:** `Auth`, `Catalog`, `Cart`, `Checkout`, `Orders`, `Addresses`, `Admin`.

No hay carpeta `Services/` ni interfaces `I*Service` de aplicación.

### `Ecommerce.Infrastructure`
Implementaciones técnicas.

| Carpeta | Qué hace |
|---------|----------|
| `Persistence/Sql/EcommerceDbContext.cs` | Modelo EF Core |
| `Persistence/Sql/Repositories/` | Repositorios de escritura y lectura (`*ReadRepository`) |
| `Persistence/Interceptors/AuditSaveChangesInterceptor.cs` | Placeholder para auditoría futura |
| `Persistence/Sql/DbSeeder.cs` | Datos iniciales |
| `Persistence/Sql/DatabaseBootstrap.cs` | `EnsureCreated` + seed al arrancar |
| `Identity/JwtTokenService.cs` | Access y refresh tokens |
| `Documents/PdfTicketGenerator.cs` | PDF de ticket (QuestPDF) |

### `Ecommerce.Domain`
Núcleo del negocio.

| Carpeta | Qué hace |
|---------|----------|
| `Entities/` | User, Product, Order, Shipment, Cart, etc. |
| `Emums/` | OrderStatus, PaymentStatus, ShipmentStatus, … |
| `Auth/`, `Orders/`, `Cart/`, `Addresses/`, `Admin/` | `*Errors` para FluentResults |
| `Addresses/AddressRules.cs` | Límites de longitud de dirección |
| `Exceptions/` | Excepciones para middleware (legacy / fallos graves) |
| `Common/BaseEntity.cs` | `Id`, `CreatedAt`, `UpdatedAt` |

## Flujo de una petición

```
HTTP → Endpoint → ISender.Send(Command/Query)
    → ValidationBehavior (FluentValidation)
    → Handler → I*Repository → EF
    → Result<T> → ToHttpResult() → JSON / PDF / 204
```

## Principios aplicados

- **Clean Architecture:** Domain sin dependencias; Application define contratos; Infrastructure implementa.
- **Minimal APIs:** un archivo estático por área (`*Endpoints.cs`).
- **CQRS:** lecturas y escrituras separadas; handlers por intención.
- **FluentResults:** fallos de negocio esperados sin excepción.
- **Validación:** pipeline MediatR (entrada) + reglas en Domain (negocio).
- **Lecturas optimizadas:** `ICatalogReadRepository`, `IOrderReadRepository`, `IAddressReadRepository` con proyecciones EF.
- **Transacciones:** checkout y pago con `IUnitOfWork`.

## Tests

| Proyecto | Alcance |
|----------|---------|
| `Ecommerce.UnitTests` | Placeholder |
| `Ecommerce.IntegrationTests` | Health, login, 403 admin, flujo compra (WebApplicationFactory + SQLite) |

## Documentación ampliada

- Principiantes: [00-guia-para-principiantes.md](./00-guia-para-principiantes.md)
- CQRS y evolución: [08-evolucion-cqrs-y-dominio.md](./08-evolucion-cqrs-y-dominio.md)
