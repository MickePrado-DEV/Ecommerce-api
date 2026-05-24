# Arquitectura

## Diagrama de capas

```
┌─────────────────────────────────────────────────────────┐
│  Ecommerce.Api                                          │
│  Endpoints, Middleware, Program, Validación HTTP        │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│  Ecommerce.Application                                  │
│  Servicios, DTOs, Validators (FluentValidation),        │
│  Interfaces de repositorios y servicios                 │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│  Ecommerce.Infrastructure                               │
│  EF Core, Repositorios, JWT, PDF, DbSeeder              │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│  Ecommerce.Domain                                       │
│  Entidades, Enums, Excepciones de dominio               │
└─────────────────────────────────────────────────────────┘
```

## Proyectos y responsabilidad

### `Ecommerce.Api`
Punto de entrada HTTP.

| Carpeta / archivo | Qué hace |
|-------------------|----------|
| `Program.cs` | Pipeline: Serilog, CORS, JWT, Scalar, bootstrap BD, registro de endpoints |
| `Endpoints/` | Minimal APIs agrupadas por feature (`Auth`, `Catalog`, `Cart`, etc.) |
| `Filters/ValidationFilter.cs` | Ejecuta FluentValidation en body de POST/PUT |
| `Middleware/ExceptionMiddleware.cs` | Convierte excepciones de dominio en JSON HTTP |
| `Extensions/HttpContextExtensions.cs` | Lee `userId` del JWT y `guestToken` del header |

### `Ecommerce.Application`
Lógica de aplicación sin dependencias de infraestructura.

| Carpeta | Qué hace |
|---------|----------|
| `Services/` | Casos de uso: login, catálogo, carrito, checkout, pedidos, admin |
| `Abstractions/` | Contratos `I*Service` e `I*Repository` |
| `DTOs/` | Objetos de entrada/salida de la API |
| `Validators/` | Reglas FluentValidation por request |
| `Authorization/AdminPermissions.cs` | Constantes de permisos admin |
| `Mapping/` | Mapeo entidad → DTO de catálogo |

### `Ecommerce.Infrastructure`
Implementaciones técnicas.

| Carpeta | Qué hace |
|---------|----------|
| `Persistence/Sql/EcommerceDbContext.cs` | Modelo EF Core y configuración de tablas |
| `Persistence/Sql/Repositories/` | Acceso a datos por agregado |
| `Persistence/Sql/DbSeeder.cs` | Datos iniciales (usuarios, catálogo demo, conductor) |
| `Persistence/Sql/DatabaseBootstrap.cs` | `EnsureCreated` + seed al arrancar |
| `Identity/JwtTokenService.cs` | Genera access token y refresh token |
| `Documents/PdfTicketGenerator.cs` | PDF de ticket de despacho (QuestPDF) |

### `Ecommerce.Domain`
Núcleo del negocio.

| Carpeta | Qué hace |
|---------|----------|
| `Entities/` | Modelo relacional (User, Product, Order, Shipment, etc.) |
| `Emums/` | Estados de pedido, pago, envío, movimientos de stock |
| `Exceptions/` | `NotFoundException`, `InsufficientStockException` |
| `Common/BaseEntity.cs` | `Id`, `CreatedAt`, `UpdatedAt` en entidades |

## Principios aplicados

- **Clean Architecture:** Domain no referencia a nadie; Application define contratos; Infrastructure implementa.
- **Minimal APIs:** sin controllers MVC; un archivo estático por área (`*Endpoints.cs`).
- **Validación en dos capas:** FluentValidation (entrada) + reglas en servicios (negocio).
- **Transacciones:** checkout y pago usan `IUnitOfWork` con `BeginTransaction` / `Commit` / `Rollback`.

## Tests

| Proyecto | Alcance |
|----------|---------|
| `Ecommerce.UnitTests` | Placeholder |
| `Ecommerce.IntegrationTests` | Health, auth, 403 admin, flujo compra con WebApplicationFactory + SQLite |
