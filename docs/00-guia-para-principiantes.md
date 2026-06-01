# Tutorial completo — De cero al código, carpeta por carpeta

Esta guía es **un solo recorrido de inicio a fin** del backend del proyecto **[MickePrado-DEV/Ecommerce-api](https://github.com/MickePrado-DEV/Ecommerce-api)** — e-commerce full-stack personal, no derivado de ningún curso.

1. **Parte I** — Instalar, clonar, seed, API, Postman, frontend (hands-on).
2. **Parte II** — Por qué el proyecto está separado en dos repos y por qué esta estructura.
3. **Parte III** — El repositorio del API **carpeta por carpeta, archivo por archivo**.
4. **Parte IV** — Código real: `Program.cs`, login, carrito, checkout, pago, JWT.
5. **Parte V** — Catálogo de **cada endpoint** → command/query → permiso.
6. **Parte VI** — Resumen frontend + enlace a la [guía en ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md).
7. **Parte VII** — Referencia, ejercicios finales y siguientes pasos.

Si es tu **primera vez** con el proyecto, **sigue el orden**. Si ya tienes experiencia en .NET, salta a la [Parte II](#parte-ii-por-qué-está-así-el-proyecto).

---

## Mapa del tutorial

| Parte | Contenido |
|-------|-----------|
| [I — Práctica](#parte-i-práctica-de-cero-a-que-funcione) | Requisitos → seed → API → Postman → web |
| [II — Decisiones](#parte-ii-por-qué-está-así-el-proyecto) | Dos repos, cuatro capas, CQRS, Minimal API |
| [III — Inventario del código](#parte-iii-inventario-del-repositorio-api) | Cada carpeta y cada archivo explicado |
| [IV — Código a código](#parte-iv--recorrido-código-a-código) | Program.cs, login, carrito, checkout, pago |
| [V — Catálogo endpoints](#parte-v--catálogo-completo-de-endpoints) | Cada ruta → command/query → permiso |
| [VI — Frontend (resumen)](#parte-vi--frontend-resumen) | Enlace a guía frontend completa |
| [VII — Referencia](#parte-vii--referencia-y-siguientes-pasos) | Glosario, ejercicios, docs |

---

# Parte I — Práctica: de cero a que funcione

## Paso 0: Qué vamos a construir

Un **e-commerce completo** con API .NET y frontend Next.js en **dos repositorios GitHub**:

| Repo | URL |
|------|-----|
| **API** | [github.com/MickePrado-DEV/Ecommerce-api](https://github.com/MickePrado-DEV/Ecommerce-api) |
| **Web** | [github.com/MickePrado-DEV/ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web) |

```
┌─────────────┐     HTTP/JSON      ┌──────────────────┐
│  Next.js    │ ◄──────────────► │  Ecommerce API   │
│  (tienda +  │   JWT + CORS     │  .NET 10         │
│   admin)    │                  │  localhost:5088  │
└─────────────┘                  └────────┬─────────┘
                                          │
                                 ┌────────▼─────────┐
                                 │  SQL Server      │
                                 │  (LocalDB)       │
                                 └──────────────────┘
```

| Área | Qué hace |
|------|----------|
| **Tienda** | Catálogo, carrito (invitado o logueado), checkout, pedidos |
| **Admin** | Productos, inventario, usuarios, pedidos, envíos, despacho por lotes |
| **Repartidor** | Ver envíos asignados, marcar entregado |
| **API** | REST en `/api/v1`, JWT, CQRS con MediatR |

**URL base API:** `http://localhost:5088/api/v1`  
**Frontend:** `http://localhost:3000`

---

## Paso 1: Requisitos

| Herramienta | Versión | Comprobar |
|-------------|---------|-----------|
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.x** | `dotnet --version` |
| [Node.js](https://nodejs.org/) | **20+** | `node --version` |
| SQL Server **LocalDB** | (viene con VS) | `(localdb)\mssqllocaldb` |
| [Postman](https://www.postman.com/) | Cualquiera | — |
| Git | Reciente | `git --version` |

> **Sin SQL Server:** perfil `Sqlite` (puerto 5089). Este tutorial usa **SqlServer** + `scriptsSql/`.

---

## Paso 2: Clonar y preparar

```powershell
git clone https://github.com/MickePrado-DEV/Ecommerce-api.git
cd Ecommerce-api
dotnet restore
dotnet build
```

Frontend (repo aparte):

```powershell
git clone https://github.com/MickePrado-DEV/ecommerce-web.git
```

**Checkpoint:** `dotnet build` sin errores.

---

## Paso 3: Poblar la base de datos

```powershell
cd scriptsSql
.\run-all.ps1
```

Ejecuta `schema.sqlserver.sql` (recrea tablas) y `seed.sqlserver.sql` (~1000+ registros por entidad).

**Checkpoint:** conteos finales tipo `Usuarios: 2003 | Pedidos: 1000`.

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |
| Repartidor | `repartidor@ecommerce.local` | `Repartidor123!` |

Producto demo: slug `audifonos-pro-x` · Cupón: `WELCOME10`

---

## Paso 4: Arrancar la API

```powershell
cd src\Ecommerce.Api
dotnet run --launch-profile SqlServer
```

| URL | Esperado |
|-----|----------|
| http://localhost:5088/health | `{ "status": "ok" }` |
| http://localhost:5088/ready | `{ "status": "ready" }` |
| http://localhost:5088/scalar/v1 | UI OpenAPI |

---

## Paso 5: Postman

Importar:

1. `postman/Ecommerce-API.postman_collection.json`
2. `postman/Ecommerce-Local.postman_environment.json`

Ejecutar **00 - Setup** → Health, Ready, Login Admin/Cliente.

**Checkpoint:** Dashboard stats responde 200 con admin; 403 con cliente.

---

## Paso 6: Flujo cliente (compra)

| # | Request | Resultado |
|---|---------|-----------|
| 1 | Login Cliente | JWT |
| 2 | Producto `audifonos-pro-x` | `variantId` |
| 3 | Agregar carrito | Carrito actualizado |
| 4 | Checkout | Pedido `PendingPayment`, stock reservado |
| 5 | Pagar | Pedido `Paid`, stock descontado |

Carpeta **Flujo Completo E2E** para automatizar con Collection Runner.

---

## Paso 7: Flujo admin (despacho)

| # | Request | Resultado |
|---|---------|-----------|
| 1 | Listar pedidos pagados | Encuentra `orderId` |
| 2 | Marcar listo despacho | `ReadyToDispatch` |
| 3 | Crear envío + conductor | `Dispatched` + ticket |
| 4 | PDF | Descarga autenticada (JWT) |

---

## Paso 8: Frontend

```powershell
cd ecommerce-web
copy .env.example .env.local
npm install
npm run dev
```

`.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5088/api/v1
```

---

## Paso 9: Navegador

- **Cliente:** catálogo → carrito → checkout → pagar (`cliente@ecommerce.local`)
- **Admin:** `/admin/products`, `/admin/orders`, `/admin/shipments`, `/admin/dispatch/queue`
- **Repartidor:** panel envíos (`repartidor@ecommerce.local`)

**Checkpoint:** compra visible en admin; PDF descarga bien.

---

# Parte II — Por qué está así el proyecto

## ¿Por qué dos repositorios (API y Web)?

El sistema se divide en **backend** y **frontend** porque son tecnologías, ciclos de vida y equipos distintos:

| Motivo | Explicación |
|--------|-------------|
| **Responsabilidades** | La API expone JSON + reglas de negocio + BD. El web solo consume HTTP y pinta UI. |
| **Clientes múltiples** | La misma API sirve tienda web, panel admin, app repartidor y futura app mobile. |
| **Despliegue independiente** | Puedes publicar la API en un servidor y el frontend en Vercel/Netlify sin mezclar builds. |
| **Portafolio / open source** | Dos repos en GitHub con README, issues e historial propios |

```
                    ┌─────────────────┐
                    │  ecommerce-web  │  Next.js, React, FSD
                    │  (solo UI)      │
                    └────────┬────────┘
                             │ fetch + JWT
              ┌──────────────┼──────────────┐
              │              │              │
     ┌────────▼───┐  ┌───────▼──────┐  ┌───▼────────┐
     │ Web tienda │  │ Panel admin  │  │ App driver │
     └────────────┘  └──────────────┘  └────────────┘
                             │
                    ┌────────▼────────┐
                    │  Ecommerce-api  │  .NET 10, EF, JWT
                    │  (única verdad) │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  SQL Server     │
                    └─────────────────┘
```

**Regla de oro:** el frontend **nunca** accede a la BD directamente. Todo pasa por `/api/v1`.

---

## ¿Por qué cuatro proyectos dentro del API?

Dentro del repo [Ecommerce-api](https://github.com/MickePrado-DEV/Ecommerce-api) hay **cuatro proyectos .NET** (solución `Ecommerce.slnx`):

```
Ecommerce.Api            ← HTTP (entrada/salida)
Ecommerce.Application    ← Casos de uso (handlers)
Ecommerce.Infrastructure ← EF Core, JWT, PDF, repos
Ecommerce.Domain           ← Entidades y reglas puras
```

Esto es **Clean Architecture** (arquitectura limpia):

| Capa | Depende de | No conoce |
|------|------------|-----------|
| **Domain** | nada | HTTP, EF, JWT |
| **Application** | Domain | SQL concreto, ASP.NET |
| **Infrastructure** | Application + Domain | Minimal API |
| **Api** | Application (+ DI de Infrastructure) | Detalle SQL en handlers |

**Beneficios para principiantes:**

1. Sabes **dónde buscar**: endpoint en `Api`, lógica en `Application`, SQL en `Infrastructure`.
2. Puedes **testear** handlers sin levantar servidor web.
3. Si cambias de SQL Server a PostgreSQL, solo tocas `Infrastructure`.

---

## ¿Por qué CQRS + MediatR?

**CQRS** = separar **comandos** (escriben) de **queries** (leen).

| Tipo | Ejemplo | Cambia BD |
|------|---------|-----------|
| Command | `LoginCommand`, `CreateOrderCommand` | Sí |
| Query | `GetFamiliesQuery`, `ListOrdersAdminQuery` | No (solo lectura) |

**MediatR** es el “bus” que conecta endpoints con handlers:

```
Endpoint → ISender.Send(LoginCommand) → LoginCommandHandler
```

**¿Por qué no poner todo en el endpoint?** Porque un endpoint de 200 líneas con SQL mezclado es imposible de mantener. Cada **intención de usuario** = un handler.

---

## ¿Por qué Minimal API y no Controllers?

En .NET clásico tenías `AuthController.cs` con `[HttpPost("login")]`.

Aquí usamos **Minimal API**: rutas en archivos estáticos `Endpoints/*.cs`:

```csharp
auth.MapPost("/login", async (LoginRequest req, ISender sender, CancellationToken ct) =>
    (await sender.Send(new LoginCommand(req.Email, req.Password), ct)).ToHttpResult());
```

| Ventaja | Detalle |
|---------|---------|
| Menos ceremonia | Sin herencia de `ControllerBase` |
| Un archivo por área | `AuthEndpoints.cs`, `AdminEndpoints.cs`… |
| Endpoint fino | Solo deserializa JSON y llama a MediatR |

---

## ¿Por qué FluentResults?

En lugar de `throw new Exception("Stock insuficiente")` en cada regla:

```csharp
return Result.Fail<CheckoutResultDto>(OrderErrors.InsufficientStock());
```

`ToHttpResult()` traduce el código de error a HTTP 409, 404, 401…

Las excepciones quedan para **fallos graves** (bug, BD caída) → `ExceptionMiddleware`.

---

## ¿Por qué scripts SQL aparte del DbSeeder?

| Mecanismo | Cuándo se usa | Volumen |
|-----------|---------------|---------|
| `DbSeeder` (C# al arrancar) | BD vacía, desarrollo rápido | Mínimo (usuarios demo) |
| `scriptsSql/run-all.ps1` | Desarrollo serio, pruebas admin | ~1000+ registros |

El seed SQL permite probar **paginación**, **despacho masivo** y **listados admin** sin escribir C# de datos fake.

---

# Parte III — Inventario del repositorio API

Árbol completo de lo que importa (sin `bin/`, `obj/`, logs):

```
Ecommerce-api/
├── src/
│   ├── Ecommerce.Api/           ← Capa HTTP
│   ├── Ecommerce.Application/   ← Handlers, DTOs, validación
│   ├── Ecommerce.Domain/        ← Entidades, enums, errores
│   └── Ecommerce.Infrastructure/← EF Core, repos, JWT, PDF
├── tests/
│   ├── Ecommerce.IntegrationTests/
│   └── Ecommerce.UnitTests/
├── scriptsSql/                  ← Schema + seed multi-motor
├── postman/                     ← Colección E2E
├── docs/                        ← Esta guía y resto de docs
├── Ecommerce.slnx
└── README.md
```

---

## 3.1 Proyecto `Ecommerce.Domain`

**Propósito:** el núcleo del negocio. **Cero dependencias** de ASP.NET o EF.

### `Common/`

| Archivo | Qué hace |
|---------|----------|
| `BaseEntity.cs` | Propiedades comunes: `Id` (Guid), `CreatedAt`, `UpdatedAt`. Todas las entidades heredan de aquí. |

### `Entities/` — modelo de datos

Agrupadas por concepto de negocio:

#### Catálogo

| Entidad | Representa |
|---------|------------|
| `Family.cs` | Familia de productos (ej. Electrónica) |
| `Category.cs` | Categoría dentro de familia |
| `Subcategory.cs` | Subcategoría (nivel más fino) |
| `Product.cs` | Producto: nombre, slug, precio base, descripción |
| `ProductImage.cs` | URL de imagen por producto |
| `Variant.cs` | Variante vendible (SKU, precio, stock por variante) |
| `ProductOption.cs` | Opción configurable (Color, Talla) |
| `OptionValue.cs` | Valor de opción (Negro, M, XL) |
| `ProductOptionAssignment.cs` | Qué opciones tiene un producto |
| `VariantOptionValue.cs` | Qué valores tiene cada variante |

#### Carrito y pedidos

| Entidad | Representa |
|---------|------------|
| `Cart.cs` | Carrito de usuario o invitado (`GuestToken`) |
| `CartItem.cs` | Línea: variante + cantidad |
| `Order.cs` | Pedido: estados, totales, cupón, dirección |
| `OrderItem.cs` | Línea del pedido (snapshot de precio/nombre) |
| `OrderAddress.cs` | Dirección congelada en el pedido |
| `Payment.cs` | Pago asociado al pedido (mock) |

#### Inventario

| Entidad | Representa |
|---------|------------|
| `Inventory.cs` | Stock disponible por variante |
| `StockReservation.cs` | Reserva temporal en checkout |
| `StockMovement.cs` | Historial: entrada, venta, ajuste, devolución |

#### Envíos y despacho

| Entidad | Representa |
|---------|------------|
| `Shipment.cs` | Envío de un pedido a un conductor |
| `Driver.cs` | Repartidor (licencia, placa, usuario vinculado) |
| `DispatchTicket.cs` | Número de ticket para PDF |
| `DispatchBatch.cs` | Lote de pedidos para despacho |
| `DispatchBatchOrder.cs` | Pedido dentro de un lote |
| `DispatchSettings.cs` | Configuración de despacho |
| `DeliveryRoute.cs` | Ruta de entrega planificada |
| `DeliveryRouteStop.cs` | Parada en una ruta |

#### Usuarios y seguridad

| Entidad | Representa |
|---------|------------|
| `User.cs` | Usuario: email, hash BCrypt, roles, teléfono |
| `Role.cs` | Rol: admin, customer, driver |
| `Permission.cs` | Permiso granular admin (`products.read`, …) |
| `UserRole.cs` | Usuario ↔ rol |
| `RolePermission.cs` | Rol ↔ permiso |
| `RefreshToken.cs` | Token de refresco JWT |

#### Otros

| Entidad | Representa |
|---------|------------|
| `Address.cs` | Dirección del cliente |
| `Cover.cs` | Portada/banner del home |
| `Coupon.cs` | Cupón de descuento |
| `WishlistItem.cs` | Producto en lista de deseos |
| `ProductReview.cs` | Reseña de producto |

### `Emums/` (sic — nombre histórico del proyecto)

| Archivo | Valores importantes |
|---------|---------------------|
| `OrderStatus.cs` | `PendingPayment` → `Paid` → `ReadyToDispatch` → `Dispatched` → `Delivered` |
| `PaymentStatus.cs` | `Pending`, `Approved`, `Declined`, `Refunded` |
| `ShipmentStatus.cs` | `Pending`, `InTransit`, `Delivered` |
| `StockMovementType.cs` | `In`, `Reservation`, `Sale`, `Return`, `Adjustment` |
| `DispatchStatus.cs` | Estado de despacho del pedido |
| `DispatchBatchStatus.cs` | Estado del lote |
| `DeliveryRouteStatus.cs` | Estado de ruta |
| `CouponDiscountType.cs` | Porcentaje o monto fijo |

### Errores de dominio (`*Errors.cs`)

Archivos que devuelven `Error` de FluentResults con metadata `Code`:

| Archivo | Errores típicos |
|---------|-----------------|
| `Auth/AuthErrors.cs` | Credenciales inválidas, email duplicado |
| `Orders/OrderErrors.cs` | Pedido no encontrado, no pagable, sin stock |
| `Cart/CartErrors.cs` | Carrito vacío |
| `Addresses/AddressErrors.cs` | Dirección no encontrada |
| `Admin/AdminErrors.cs` | Recurso admin, transición de estado inválida |
| `Driver/DriverErrors.cs` | Repartidor no encontrado |

### Reglas y servicios

| Archivo | Qué hace |
|---------|----------|
| `Addresses/AddressRules.cs` | Longitudes máximas de campos de dirección |
| `Covers/CoverRules.cs` | Validación de portadas |
| `Services/CouponCalculator.cs` | Calcula descuento de cupón (dominio puro) |
| `Authorization/RoleCodes.cs` | Constantes: `admin`, `customer`, `driver` |

### `Exceptions/` (legacy / fallos graves)

| Archivo | Cuándo |
|---------|--------|
| `NotFoundException.cs` | Recurso inexistente (middleware) |
| `InsufficientStockException.cs` | Stock crítico en transacciones |

---

## 3.2 Proyecto `Ecommerce.Application`

**Propósito:** casos de uso. Conoce Domain y define **contratos** (`I*Repository`), pero **no** implementa SQL.

### `DependencyInjection.cs`

Registra MediatR, FluentValidation y `ValidationBehavior` (valida antes de cada handler).

### `Common/`

| Archivo | Qué hace |
|---------|----------|
| `PagedResult.cs` | Respuesta paginada genérica (`Items`, `Total`, `Page`, `PageSize`) |
| `PaginationRules.cs` | Límites de pageSize, defaults |
| `OptionFeatureSnapshot.cs` | Snapshot de opciones en DTOs |
| `ReviewErrors.cs` | Errores de reseñas |
| `Behaviors/ValidationBehavior.cs` | Pipeline MediatR: ejecuta validadores FluentValidation |

### `Authorization/`

| Archivo | Qué hace |
|---------|----------|
| `AdminPermissions.cs` | Constantes de permisos (`admin.dashboard.read`, `admin.products.write`, …). `Program.cs` crea una política JWT por cada una. |

### `Abstractions/` — interfaces que Infrastructure implementa

| Interfaz | Responsabilidad |
|----------|-----------------|
| `IJwtTokenService` | Generar access/refresh token |
| `IPdfTicketGenerator` | PDF de ticket de envío |
| `ICoverImageStorage` | Guardar imágenes de portadas |
| `IUnitOfWork` | `SaveChanges` + transacciones |

#### `Abstractions/Persistence/`

| Interfaz | Responsabilidad |
|----------|-----------------|
| `IUserRepository` | Login, registro, refresh tokens, permisos |
| `ICatalogReadRepository` | Lecturas públicas optimizadas |
| `ICartRepository` | Carrito guest/usuario |
| `IOrderRepository` / `IOrderReadRepository` | Escritura / lectura pedidos |
| `IAddressReadRepository` / `IAddressWriteRepository` | Direcciones |
| `IInventoryRepository` | Stock, reservas, ajustes |
| `IAdminCatalogRepository` | CRUD catálogo admin |
| `IAdminUserRepository` | CRUD usuarios admin |
| `IAdminRoleRepository` | Roles y permisos |
| `IShipmentRepository` | Envíos |
| `IDriverRepository` | Repartidores |
| `IDispatchRepository` | Lotes y rutas de despacho |
| `ICoverRepository` | Portadas |
| `IDashboardRepository` | Stats del dashboard |
| `IProductOptionRepository` | Opciones por producto |
| `IWishlistRepository` | Wishlist |
| `IProductReviewRepository` | Reseñas |
| `ICouponRepository` | Cupones |

### `DTOs/` — contratos JSON

Carpetas por área: `Auth/`, `Catalog/`, `Cart/`, `Checkout/`, `Orders/`, `Addresses/`, `Admin/`, `Shipments/`, `Dispatch/`.

Los endpoints reciben/devuelven estos tipos (o records del handler mapeados a ellos).

### `Features/` — handlers CQRS (corazón de la lógica)

Cada módulo agrupa commands, queries y handlers:

| Carpeta | Contenido principal |
|---------|---------------------|
| **`Auth/`** | `AuthHandlers.cs` — Login, Register, Refresh, Logout. `AuthProfileHandlers.cs` — perfil y cambio de contraseña. `Validators/` — reglas de email, password. |
| **`Catalog/`** | `CatalogQueries.cs` — home, familias, productos, detalle, reseñas. |
| **`Cart/`** | `CartHandlers.cs` — get, add, update, remove, merge guest. `CartMapping.cs`, `Validators/`. |
| **`Checkout/`** | `CheckoutHandlers.cs` — `CreateOrderCommand`: carrito → pedido + reserva stock. `Validators/CreateOrderCommandValidator.cs`. |
| **`Orders/`** | `OrderHandlers.cs` — listar, pagar, cancelar, tracking. `OrderMapping.cs`. |
| **`Addresses/`** | Commands y queries CRUD + default. |
| **`Wishlist/`** | `WishlistHandlers.cs` — add, remove, list. |
| **`Driver/`** | `DriverHandlers.cs` — envíos del repartidor, marcar entregado. |
| **`Admin/`** | `AdminHandlers.cs` — dashboard, catálogo, inventario, pedidos, envíos, covers. `AdminUserCommandHandlers.cs`, `AdminUserQueryHandlers.cs`, `AdminRoleHandlers.cs`. **`Lists/`** — handlers paginados: `ListProductsAdminHandler`, `ListOrdersAdminHandler`, `ListShipmentsAdminHandler`, `ListInventoryAdminHandler`, `ListUsersAdminHandler`, `ListDriversAdminHandler`, `ListDriversOptionsHandler`, `GetInventoryByVariantHandler`. |
| **`Dispatch/`** | `DispatchHandlers.cs` — cola, lotes. `Services/` — `DispatchBatchService`, `RoutePlannerService`, `RouteAssignmentService`, `RouteExecutionService`, `GeoMath.cs`. |

**Patrón de cada handler:**

```csharp
public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public class LoginCommandHandler(...) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        // 1. Leer repos
        // 2. Validar reglas de negocio
        // 3. return Result.Ok(...) o Result.Fail(...)
    }
}
```

---

## 3.3 Proyecto `Ecommerce.Infrastructure`

**Propósito:** implementaciones técnicas. Aquí vive EF Core, SQL, JWT, PDF.

### `DependencyInjection.cs`

- Configura `EcommerceDbContext` (SqlServer o Sqlite según `appsettings`).
- Registra **todos** los repositorios como `Scoped` (una instancia por petición HTTP).
- Registra `JwtTokenService`, `PdfTicketGenerator`, `UnitOfWork`.

### `Persistence/Sql/`

| Archivo | Qué hace |
|---------|----------|
| `EcommerceDbContext.cs` | DbContext EF: `DbSet<>` de cada entidad, convención snake_case, enums como string |
| `DatabaseBootstrap.cs` | Al arrancar: `EnsureCreated`, detecta esquema viejo, llama `DbSeeder` |
| `DbSeeder.cs` | Inserta usuarios demo si la BD está vacía |
| `UnitOfWork.cs` | Wrapper de `SaveChangesAsync` + transacciones |
| `DatabaseProvider.cs` | Enum SqlServer / Sqlite / MySql / MariaDb |

#### `Persistence/Sql/Repositories/` — implementación de interfaces

| Archivo | Implementa |
|---------|------------|
| `UserRepository.cs` | `IUserRepository` |
| `CatalogReadRepository.cs` | `ICatalogReadRepository` — proyecciones EF para catálogo |
| `CartRepository.cs` | `ICartRepository` |
| `OrderRepository.cs` | `IOrderRepository` |
| `OrderReadRepository.cs` | `IOrderReadRepository` |
| `InventoryRepository.cs` | `IInventoryRepository` |
| `InventoryListQueries.cs` | Queries SQL/EF para listado admin inventario |
| `InventoryReservationQueries.cs` | Lógica de reservas |
| `AdminCatalogRepository.cs` | `IAdminCatalogRepository` |
| `AdminUserRepository.cs` | `IAdminUserRepository` |
| `AdminRoleRepository.cs` | `IAdminRoleRepository` |
| `ShipmentRepository.cs` | `IShipmentRepository` — incluye Order+Items para PDF |
| `ShipmentListQueries.cs` | Listado paginado envíos |
| `ShipmentDriverQueries.cs` | Queries de conductores |
| `DriverRepository.cs` | `IDriverRepository` |
| `DispatchRepository.cs` | `IDispatchRepository` |
| `AddressReadRepository.cs` / `AddressWriteRepository.cs` | Direcciones |
| `CoverRepository.cs` | Portadas |
| `DashboardRepository.cs` | Agregados del dashboard |
| `ProductOptionRepository.cs` | Opciones producto |
| `WishlistRepository.cs` | Wishlist |
| `ProductReviewRepository.cs` | Reseñas |
| `CouponRepository.cs` | Cupones |
| `ProductRepository.cs` | Productos (legacy/escritura) |
| `CatalogRepository.cs` | Catálogo escritura |

#### `Persistence/Sql/Common/`

| Archivo | Qué hace |
|---------|----------|
| `QueryableSortHelper.cs` | Ordenamiento dinámico para tablas admin (`sortBy`, `sortDir`) |

#### `Persistence/Sql/Seed/`

| Archivo | Qué hace |
|---------|----------|
| `CatalogSeeder.cs` | Seed mínimo de catálogo en C# |
| `CatalogSeedModels.cs` | Modelos auxiliares del seed |
| `CatalogOptionTemplates.cs` | Plantillas de opciones |
| `SlugHelper.cs` | Genera slugs URL-friendly |

### Otros

| Carpeta / archivo | Qué hace |
|-------------------|----------|
| `Identity/JwtTokenService.cs` | Crea JWT con claims de rol y permisos; genera refresh token |
| `Documents/PdfTicketGenerator.cs` | QuestPDF: ticket de envío |
| `Storage/CoverImageStorage.cs` | Guarda archivos en `/uploads` |
| `Persistence/Interceptors/AuditSaveChangesInterceptor.cs` | Placeholder auditoría futura |

---

## 3.4 Proyecto `Ecommerce.Api`

**Propósito:** entrada HTTP. **No** contiene lógica de negocio.

### `Program.cs` — el arranque de todo

Línea por línea (resumen):

| Bloque | Qué hace |
|--------|----------|
| `WebApplication.CreateBuilder` | Crea la app ASP.NET |
| `UseSerilog` | Logs a consola + archivo `logs/` |
| `AddApplication()` | MediatR + validación |
| `AddInfrastructure()` | EF + repos + JWT |
| `AddCors("Web")` | Permite `localhost:3000` |
| `AddAuthentication(JwtBearer)` | Valida Bearer token |
| `AddAuthorization` | Una política por permiso admin |
| `DatabaseBootstrap.InitializeAsync` | Crea BD / seed mínimo |
| `UseMiddleware<ExceptionMiddleware>` | Errores → JSON |
| `MapOpenApi` + Scalar | Docs interactivas (no prod) |
| `UseStaticFiles("/uploads")` | Sirve imágenes subidas |
| Pipeline CORS → Auth → Authorization | Orden obligatorio |
| `MapGet("/health")`, `MapGet("/ready")` | Health checks |
| `MapGroup("/api/v1")` + `Map*Endpoints()` | Rutas de negocio |

### `Endpoints/` — un archivo por área

| Archivo | Prefijo | Auth |
|---------|---------|------|
| `AuthEndpoints.cs` | `/auth` | Mixto (login público, `/me` protegido) |
| `CatalogEndpoints.cs` | `/catalog` | Público |
| `CartEndpoints.cs` | `/cart` | Guest header o JWT |
| `AddressEndpoints.cs` | `/addresses` | JWT cliente |
| `CheckoutEndpoints.cs` | `/checkout` | JWT cliente |
| `OrderEndpoints.cs` | `/orders` | JWT cliente |
| `WishlistEndpoints.cs` | `/wishlist` | JWT cliente |
| `AdminEndpoints.cs` | `/admin` | JWT admin + permiso |
| `DriverEndpoints.cs` | `/driver` | JWT repartidor |

**Patrón de cada ruta:**

```csharp
admin.MapGet("/products", async (ISender sender, ...) =>
    (await sender.Send(new ListProductsAdminQuery(...), ct)).ToHttpResult())
    .RequireAuthorization(AdminPermissions.ProductsRead);
```

### `Extensions/`

| Archivo | Qué hace |
|---------|----------|
| `ResultExtensions.cs` | `Result` → 200/204/400/401/403/404/409 según metadata `Code` |
| `HttpContextExtensions.cs` | `GetUserId()`, `GetGuestToken()` desde JWT/headers |
| `AdminTableQueryBinding.cs` | Enlaza query string `page`, `pageSize`, `search`, `sortBy` a DTOs admin |

### `Middleware/`

| Archivo | Qué hace |
|---------|----------|
| `ExceptionMiddleware.cs` | Captura excepciones no controladas |
| `ApiExceptionMapper.cs` | Mapea excepciones de dominio a HTTP |

### `Models/`

| Archivo | Qué hace |
|---------|----------|
| `ApiErrorResponse.cs` | JSON de error estándar `{ errors: [{ message, code }] }` |

### `Filters/`

| Archivo | Qué hace |
|---------|----------|
| `ValidationFilter.cs` | Legacy; la validación principal está en `ValidationBehavior` |

### Configuración (`appsettings*.json`)

| Archivo | Contenido |
|---------|-----------|
| `appsettings.json` | Base: JWT, CORS, logging |
| `appsettings.SqlServer.json` | Connection string LocalDB |
| `appsettings.Sqlite.json` | SQLite file |
| `Properties/launchSettings.json` | Perfiles SqlServer (:5088) y Sqlite (:5089) |

---

## 3.5 Carpeta `scriptsSql/`

| Archivo | Qué hace |
|---------|----------|
| `schema.sqlserver.sql` | DROP + CREATE tablas SQL Server |
| `seed.sqlserver.sql` | INSERT masivo (~1000+ por entidad) |
| `schema.mysql.sql` / `seed.mysql.sql` | Equivalente MySQL |
| `schema.mariadb.sql` / `seed.mariadb.sql` | Equivalente MariaDB |
| `schema.postgresql.sql` / `seed.postgresql.sql` | Equivalente PostgreSQL |
| `run-all.ps1` | Ejecuta schema + seed según `-Provider` |
| `tools/generate-seed.mjs` | Regenera seeds (variable `BULK_COUNT`) |
| `tools/build-scripts.mjs` | Genera schemas de otros motores desde SQL Server |

---

## 3.6 Carpeta `postman/`

| Archivo | Qué hace |
|---------|----------|
| `Ecommerce-API.postman_collection.json` | Todas las rutas + flujos E2E |
| `Ecommerce-Local.postman_environment.json` | `baseUrl`, tokens, IDs demo |
| `README.md` | Cómo importar y ejecutar flujos |
| `scripts/sync-collection-v2.js` | Regenera colección desde OpenAPI |

---

## 3.7 Carpeta `tests/`

| Proyecto | Qué prueba |
|----------|------------|
| `Ecommerce.IntegrationTests` | HTTP real con `WebApplicationFactory`: health, login, checkout, 403 admin |
| `Ecommerce.UnitTests` | Placeholder para tests unitarios de dominio |

```powershell
dotnet test
```

---

## 3.8 Frontend (`ecommerce-web`) — resumen

Repo: [github.com/MickePrado-DEV/ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web)

**Guía completa (repo frontend):** [ecommerce-web/docs/00-guia-para-principiantes.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md)

Arquitectura **Feature-Sliced Design (FSD)**:

```
src/
  app/       → rutas Next.js (App Router)
  views/     → páginas completas (admin-products-page, catalog-index…)
  widgets/   → bloques compuestos (AdminDataTable, sidebar, layout)
  features/  → acciones (login, useAdminTableQuery)
  entities/  → llamadas API por dominio (admin-api, catalog-api)
  shared/    → client HTTP, componentes UI, utilidades
```

| Capa FSD | Equivalente en el API |
|----------|----------------------|
| `entities/admin-api` | Endpoints `/admin/*` |
| `features/auth` | Endpoints `/auth/*` |
| `widgets/admin/admin-data-table` | Listados paginados (`PagedResult`) |
| `shared/api/client.ts` | JWT en headers, `downloadAuthenticatedFile` para PDF |

Guía detallada: [ecommerce-web/docs/00-guia-para-principiantes.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md) · Referencia técnica: [10-referencia-fsd-completo.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/10-referencia-fsd-completo.md)

---

# Parte IV — Recorrido código a código

## 4.1 Flujo completo: POST `/api/v1/auth/login`

### Paso 1 — Cliente envía JSON

```http
POST /api/v1/auth/login
Content-Type: application/json

{ "email": "cliente@ecommerce.local", "password": "Cliente123!" }
```

### Paso 2 — `AuthEndpoints.cs`

```csharp
auth.MapPost("/login", async (LoginRequest req, ISender sender, CancellationToken ct) =>
    (await sender.Send(new LoginCommand(req.Email, req.Password), ct)).ToHttpResult());
```

- Deserializa body → `LoginRequest`.
- Crea `LoginCommand`.
- `ISender` = MediatR.

### Paso 3 — `ValidationBehavior`

Si existiera `LoginCommandValidator` con reglas (email requerido, etc.), falla aquí → 400 sin entrar al handler.

### Paso 4 — `LoginCommandHandler` (`AuthHandlers.cs`)

```csharp
var user = await users.GetByEmailWithRolesAsync(request.Email, ct);
if (user is null || !VerifyPassword(user, request.Password))
    return Result.Fail<LoginResponse>(AuthErrors.InvalidCredentials());
```

- `IUserRepository` → `UserRepository.cs` → EF query a tabla `users`.
- `VerifyPassword` usa BCrypt (o contraseña temporal si `MustChangePassword`).

Si OK:

```csharp
var permissions = await users.GetPermissionsAsync(user.Id, ct);
var access = jwt.GenerateAccessToken(user, permissions);
var refresh = jwt.GenerateRefreshToken();
await users.SaveRefreshTokenAsync(user.Id, refresh.Hash, refresh.ExpiresAt, ct);
return Result.Ok(new LoginResponse(access, refresh.Token, userDto, permissions, ...));
```

### Paso 5 — `JwtTokenService.cs`

Genera JWT con claims: `sub` (userId), roles, permisos admin.

### Paso 6 — `ResultExtensions.ToHttpResult()`

`Result.Ok` → **200** + JSON:

```json
{
  "accessToken": "eyJ...",
  "refreshToken": "...",
  "user": { "email": "cliente@ecommerce.local", "roles": ["customer"] }
}
```

Si credenciales mal → **401** + `{ "errors": [{ "code": "Unauthorized", ... }] }`.

---

## 4.2 Flujo completo: POST `/api/v1/checkout`

### Paso 1 — Cliente autenticado

```http
POST /api/v1/checkout
Authorization: Bearer eyJ...
Content-Type: application/json

{ "addressId": "66666666-...", "shippingCost": 10, "couponCode": "WELCOME10" }
```

### Paso 2 — `CheckoutEndpoints.cs`

Extrae `userId` del JWT → `CreateOrderCommand.FromRequest(userId, body)` → `ISender.Send`.

### Paso 3 — `CreateOrderCommandHandler` (`CheckoutHandlers.cs`)

Secuencia dentro del handler:

1. **Carrito** — `carts.GetOrCreateAsync(userId)`. Si vacío → `CartErrors.EmptyCart()`.
2. **Subtotal** — suma precio × cantidad de cada ítem.
3. **Dirección** — `addressId` existente o campos inline → `OrderAddress`.
4. **Cupón** — `CouponCalculator` + `ICouponRepository` → descuento.
5. **Crear entidad `Order`** — estado `PendingPayment`, líneas desde carrito.
6. **Transacción** (`IUnitOfWork`):
   - `inventory.ReserveStockAsync(...)` — reserva stock.
   - `orders.AddAsync(order)`.
   - `carts.ClearAsync(...)`.
   - `uow.SaveChangesAsync()`.

### Paso 4 — Respuesta

```json
{
  "orderId": "...",
  "orderNumber": "ORD-...",
  "total": 179.99,
  "status": "PendingPayment"
}
```

### Paso 5 — Pago mock

`POST /api/v1/orders/{id}/pay` → `PayOrderCommandHandler`:

- Verifica estado `PendingPayment`.
- Confirma reserva → descuenta stock real.
- `Payment.Status = Approved`, `Order.Status = Paid`.
- Transacción con `IUnitOfWork`.

---

## 4.3 Flujo admin: listado paginado

Ejemplo: `GET /api/v1/admin/products?page=1&pageSize=20&search=audifonos&sortBy=name&sortDir=asc`

1. `AdminEndpoints.cs` → `ListProductsAdminQuery`.
2. `ListProductsAdminHandler.cs` → llama `IAdminCatalogRepository.ListPagedAsync(...)`.
3. `AdminCatalogRepository.cs` → EF con `QueryableSortHelper`.
4. Devuelve `PagedResult<ProductAdminDto>`.
5. Frontend `AdminDataTable` + `useAdminTableQuery` renderiza tabla + paginación sticky.

---

## 4.4 Flujo admin: PDF de envío

```http
GET /api/v1/admin/shipments/{shipmentId}/ticket.pdf
Authorization: Bearer eyJ...
```

**Endpoint** (`AdminEndpoints.cs`):

```csharp
shipments.MapGet("/{shipmentId:guid}/ticket.pdf", async (Guid shipmentId, ISender sender, CancellationToken ct) =>
    (await sender.Send(new GenerateShipmentTicketPdfQuery(shipmentId), ct))
        .ToHttpResult(pdf => Results.File(pdf, "application/pdf", $"ticket-{shipmentId}.pdf")))
    .RequireAuthorization(AdminPermissions.ShipmentsView);
```

**Handler** → `ShipmentRepository.GetByIdAsync` incluye `Order.Items` y `Order.Address` → `PdfTicketGenerator` (QuestPDF) devuelve `byte[]`.

**Frontend** (`shared/api/client.ts`):

```typescript
export async function downloadAuthenticatedFile(path: string, filename: string) {
  const res = await fetch(`${API_URL}${path}`, {
    headers: { Authorization: `Bearer ${getAccessToken()}` },
  });
  const blob = await res.blob();
  // crea enlace temporal y dispara descarga
}
```

Sin JWT en la URL → Postman con enlace directo da **401**. Eso es correcto.

---

## 4.5 `Program.cs` — línea por línea

Abre `src/Ecommerce.Api/Program.cs` y sigue este mapa:

| Líneas | Código | Qué hace |
|--------|--------|----------|
| 20 | `WebApplication.CreateBuilder` | Crea el host ASP.NET |
| 23–29 | `UseSerilog` | Logs a consola + `logs/ecommerce-*.log` |
| 32 | `AddApplication()` | MediatR + FluentValidation + ValidationBehavior |
| 34 | `AddInfrastructure()` | EF Core + todos los repos + JWT + PDF |
| 36 | `CoverImageStorage` | Singleton para subir portadas a `/uploads` |
| 40–42 | `AddCors("Web")` | Orígenes del frontend (`Cors:Origins`) |
| 46–59 | `AddAuthentication(JwtBearer)` | Valida firma, issuer, audience del token |
| 62–66 | `AddAuthorization` | Una política por cada `AdminPermissions.*` |
| 72 | `DatabaseBootstrap.InitializeAsync` | Crea tablas + seed mínimo si vacío |
| 75 | `ExceptionMiddleware` | Excepciones → JSON |
| 78–86 | Scalar + OpenAPI | Solo si no es Production |
| 89–94 | `UseStaticFiles("/uploads")` | Sirve imágenes subidas |
| 97–100 | CORS → Auth → Authorization | Orden obligatorio del pipeline |
| 103–109 | `/health`, `/ready` | Health checks fuera de `/api/v1` |
| 112–121 | `MapGroup("/api/v1")` | Registra los 9 archivos de endpoints |
| 135 | `public partial class Program` | Necesario para tests de integración |

---

## 4.6 `ValidationBehavior` y `ToHttpResult` — el puente errores → HTTP

**ValidationBehavior** (`Application/Common/Behaviors/ValidationBehavior.cs`):

```csharp
// Antes de ejecutar el handler:
var failures = validators.Select(v => v.Validate(context))...
if (failures.Count == 0)
    return await next();  // pasa al handler

// Si falla validación → Result.Fail con Code = "Validation" → HTTP 400
return ResultFactory.CreateFailure<TResponse>(failures);
```

**ResultExtensions** (`Api/Extensions/ResultExtensions.cs`):

```csharp
public static IResult ToHttpResult<T>(this Result<T> result) =>
    result.IsSuccess ? Results.Ok(result.Value) : ToErrorResult(result);
```

Mapeo de códigos:

| Metadata `Code` | HTTP |
|-----------------|------|
| `Unauthorized` | 401 |
| `Forbidden` | 403 |
| `NotFound`, `Order.NotFound`… | 404 |
| `Conflict`, `InsufficientStock` | 409 |
| `Validation` | 400 |
| `Database.Unavailable` | 503 |

---

## 4.7 Carrito — código completo del flujo

**Endpoint** (`CartEndpoints.cs`):

```csharp
cart.MapPost("/items", async (AddCartItemRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
    (await sender.Send(new AddCartItemCommand(
        ctx.GetUserId(), ctx.GetGuestToken(), req.VariantId, req.Quantity), ct)).ToHttpResult());
```

**Handler** (`CartHandlers.cs`):

```csharp
public async Task<Result<CartDto>> Handle(AddCartItemCommand request, CancellationToken ct)
{
    if (request.Quantity <= 0)
        return Result.Fail<CartDto>(CartErrors.InvalidQuantity());

    var cart = await carts.GetOrCreateAsync(request.UserId, request.GuestToken, ct);
    var variant = await carts.GetVariantAsync(request.VariantId, ct);
    if (variant is null)
        return Result.Fail<CartDto>(CartErrors.VariantNotFound(request.VariantId));

    // Si ya existe la variante en el carrito, suma cantidad
    var existing = cart.Items.FirstOrDefault(i => i.VariantId == request.VariantId);
    if (existing is not null)
        existing.Quantity += request.Quantity;
    else
        cart.Items.Add(new CartItem { CartId = cart.Id, VariantId = variant.Id, Quantity = request.Quantity });

    await uow.SaveChangesAsync(ct);
    return Result.Ok(CartMapping.ToDto(cart));
}
```

**Invitado vs usuario:**

| Situación | Cómo identifica el carrito |
|-----------|----------------------------|
| Sin login | Header `X-Guest-Token: {guid}` |
| Con login | Claim `NameIdentifier` del JWT |
| Tras login | `POST /cart/merge` con `{ "guestToken": "..." }` fusiona carritos |

**HttpContextExtensions.cs:**

```csharp
public static Guid? GetUserId(this HttpContext ctx) =>
    Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

public static Guid? GetGuestToken(this HttpContext ctx) =>
    Guid.TryParse(ctx.Request.Headers["X-Guest-Token"].FirstOrDefault(), out var id) ? id : null;
```

---

## 4.8 Checkout — transacción real (código del handler)

Tras el endpoint (`CheckoutEndpoints.cs`):

```csharp
checkout.MapPost("/", async (CheckoutRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
{
    var userId = ctx.GetUserId();
    if (userId is null) return Results.Unauthorized();
    var cmd = CreateOrderCommandMapping.FromRequest(userId.Value, req);
    return (await sender.Send(cmd, ct)).ToHttpResult();
});
```

**Bloque transaccional** (`CheckoutHandlers.cs`):

```csharp
await uow.BeginTransactionAsync(ct);
try
{
    await orders.AddAsync(order, ct);
    await uow.SaveChangesAsync(ct);                    // genera order.Id
    await inventory.ReserveAsync(order.Id,            // reserva stock 30 min
        order.Items.Select(i => (i.VariantId, i.Quantity)),
        DateTime.UtcNow.AddMinutes(30), ct);
    await carts.ClearAsync(cart.Id, ct);               // vacía carrito
    if (couponId.HasValue)
        await coupons.IncrementUsedAsync(couponId.Value, ct);
    await uow.CommitAsync(ct);
}
catch (InsufficientStockException ex)
{
    await uow.RollbackAsync(ct);
    return Result.Fail<CheckoutResultDto>(OrderErrors.InsufficientStock(ex.VariantId));
}
```

**Cupón** usa dominio puro (`Domain/Services/CouponCalculator.cs`):

```csharp
var coupon = await coupons.GetByCodeAsync(normalized, ct);
if (!CouponCalculator.IsValidFor(coupon, subtotal, DateTime.UtcNow))
    return Result.Fail(...);
var discount = CouponCalculator.ComputeDiscount(coupon, subtotal);
```

---

## 4.9 Pago mock — `PayOrderCommandHandler`

```csharp
public async Task<Result<PaymentResultDto>> Handle(PayOrderCommand request, CancellationToken ct)
{
    var order = await orders.GetByIdForUserAsync(request.OrderId, request.UserId, ct);
    if (order is null)
        return Result.Fail<PaymentResultDto>(OrderErrors.NotFound(request.OrderId));

    if (order.Status is not OrderStatus.PendingPayment and not OrderStatus.PaymentFailed)
        return Result.Fail<PaymentResultDto>(OrderErrors.NotPayable());

    // Simulación: tarjeta terminada en 0002 → rechazada
    var declined = request.Card?.Number?.EndsWith("0002") ?? false;

    await uow.BeginTransactionAsync(ct);
    if (declined)
    {
        order.Payment!.Status = PaymentStatus.Declined;
        order.Status = OrderStatus.PaymentFailed;
    }
    else
    {
        order.Payment!.Status = PaymentStatus.Approved;
        order.Payment.ProviderReference = $"MOCK-{Guid.NewGuid():N}";
        order.Status = OrderStatus.Paid;
        await inventory.CommitReservationAsync(order.Id, ct);  // stock real descontado
    }
    await uow.CommitAsync(ct);
}
```

Rutas que llaman al mismo handler:

- `POST /api/v1/orders/{id}/pay`
- `POST /api/v1/checkout/{id}/pay`

---

## 4.10 JWT — qué lleva el token

`JwtTokenService.GenerateAccessToken`:

```csharp
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new(ClaimTypes.Email, user.Email),
};
claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
claims.AddRange(permissions.Select(p => new Claim("permission", p)));
```

| Claim | Uso |
|-------|-----|
| `NameIdentifier` | `GetUserId()` en endpoints |
| `Role` | `.RequireRole("driver")` en `/driver/*` |
| `permission` | `.RequireAuthorization(AdminPermissions.ProductsView)` |

---

## 4.11 Entidad `Order` y `EcommerceDbContext`

**Domain** (`Entities/Order.cs`):

```csharp
public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = null!;
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public DispatchStatus DispatchStatus { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public OrderAddress? Address { get; set; }
    public Payment? Payment { get; set; }
    public Shipment? Shipment { get; set; }
}
```

**DbContext** (`EcommerceDbContext.cs`) — convenciones clave:

```csharp
modelBuilder.Entity<Order>().ToTable("orders");
modelBuilder.Entity<OrderItem>().ToTable("order_items");
// Enums guardados como string en SQL
// Decimales: decimal(18,2)
// Claves compuestas: user_roles, role_permissions, variant_option_values
```

Cada `DbSet<>` corresponde a una tabla del seed SQL (`scriptsSql/schema.sqlserver.sql`).

---

## 4.12 Listado admin paginado — ejemplo real

**Endpoint:**

```csharp
catalog.MapGet("/products", async (int page, int pageSize, string? search, string? sortBy, string? sortDirection,
    ISender sender, CancellationToken ct) =>
    (await sender.Send(new ListProductsAdminQuery(
        page, pageSize, search, sortBy, sortDirection ?? "asc"), ct)).ToHttpResult())
    .RequireAuthorization(AdminPermissions.ProductsView);
```

**Handler** (`ListProductsAdminHandler.cs`):

```csharp
public async Task<Result<PagedResult<ProductAdminDto>>> Handle(ListProductsAdminQuery request, CancellationToken ct)
{
    var paging = PaginationRules.NormalizeOrDefault(request.Page, request.PageSize);
    if (paging.IsFailed) return Result.Fail<PagedResult<ProductAdminDto>>(paging.Errors);

    var sort = PaginationRules.ValidateSort(request.SortBy, request.SortDirection, SortKeys);
    if (sort.IsFailed) return Result.Fail<PagedResult<ProductAdminDto>>(sort.Errors);

    var result = await repo.ListProductsAsync(page, pageSize, request.Search, request.SortBy, request.SortDirection, ct);
    return Result.Ok(PaginationRules.Create(items, result.Total, page, pageSize));
}
```

**Respuesta JSON:**

```json
{
  "items": [ { "id": "...", "name": "Audífonos Pro X", "slug": "audifonos-pro-x", ... } ],
  "total": 1001,
  "page": 1,
  "pageSize": 20
}
```

---

# Parte V — Catálogo completo de endpoints

Cada fila: **Ruta → Command/Query → Permiso/Rol**

## Auth (`AuthEndpoints.cs`) — prefijo `/api/v1/auth`

| Método | Ruta | MediatR | Auth |
|--------|------|---------|------|
| POST | `/register/customer` | `RegisterCustomerCommand` | Público |
| POST | `/register/driver` | `RegisterDriverCommand` | Público |
| POST | `/register` | `RegisterCommand` | Público (alias) |
| POST | `/login` | `LoginCommand` | Público |
| POST | `/refresh` | `RefreshTokenCommand` | Público |
| POST | `/logout` | `LogoutCommand` | JWT |
| GET | `/me` | `GetMeQuery` | JWT |
| PATCH | `/me` | `UpdateProfileCommand` | JWT |
| POST | `/change-password` | `ChangePasswordCommand` | JWT |
| POST | `/change-password/mandatory` | `MandatoryChangePasswordCommand` | JWT |

## Catálogo (`CatalogEndpoints.cs`) — `/api/v1/catalog`

| Método | Ruta | Query/Command | Auth |
|--------|------|---------------|------|
| GET | `/home` | `GetCatalogHomeQuery` | Público |
| GET | `/covers` | `GetCoversQuery` | Público |
| GET | `/products/latest` | `GetLatestProductsQuery` | Público |
| GET | `/families` | `GetFamiliesQuery` | Público |
| GET | `/families/{slug}` | `GetFamilyBySlugQuery` | Público |
| GET | `/categories/{slug}` | `GetCategoryBySlugQuery` | Público |
| GET | `/subcategories/{slug}` | `GetSubcategoryBySlugQuery` | Público |
| GET | `/products/{slug}` | `GetProductBySlugQuery` | Público |
| GET | `/products/{slug}/reviews` | `GetProductReviewsQuery` | Público |
| GET | `/products/{slug}/reviews/eligibility` | `GetProductReviewEligibilityQuery` | JWT |
| POST | `/products/{slug}/reviews` | `CreateProductReviewCommand` | JWT |
| POST | `/products/{slug}/resolve-variant` | `ResolveProductVariantQuery` | Público |
| GET | `/products/filter-options` | `GetCatalogFilterOptionsQuery` | Público |
| GET | `/products` | `ListProductsQuery` | Público |
| GET | `/search` | `ListProductsQuery` | Público |

## Carrito (`CartEndpoints.cs`) — `/api/v1/cart`

| Método | Ruta | Command/Query | Auth |
|--------|------|---------------|------|
| GET | `/` | `GetCartQuery` | Guest o JWT |
| POST | `/items` | `AddCartItemCommand` | Guest o JWT |
| PUT/PATCH | `/items/{itemId}` | `UpdateCartItemCommand` | Guest o JWT |
| DELETE | `/items/{itemId}` | `RemoveCartItemCommand` | Guest o JWT |
| DELETE | `/` | `ClearCartCommand` | Guest o JWT |
| POST | `/merge` | `MergeCartCommand` | JWT |

## Checkout (`CheckoutEndpoints.cs`) — `/api/v1/checkout`

| Método | Ruta | Command | Auth |
|--------|------|---------|------|
| POST | `/` | `CreateOrderCommand` | JWT |
| POST | `/{orderId}/pay` | `PayOrderCommand` | JWT |

## Pedidos (`OrderEndpoints.cs`) — `/api/v1/orders`

| Método | Ruta | Command/Query | Auth |
|--------|------|---------------|------|
| GET | `/` | `ListMyOrdersQuery` | JWT |
| GET | `/{orderId}` | `GetMyOrderQuery` | JWT |
| GET | `/{orderId}/tracking` | `GetOrderTrackingQuery` | JWT |
| POST | `/{orderId}/cancel` | `CancelOrderCommand` | JWT |
| POST | `/{orderId}/pay` | `PayOrderCommand` | JWT |
| POST | `/{orderId}/retry-payment` | `PayOrderCommand` | JWT |

## Admin — Dashboard

| Método | Ruta | Query | Permiso |
|--------|------|-------|---------|
| GET | `/admin/dashboard/stats` | `GetDashboardStatsQuery` | `admin.dashboard.view` |
| GET | `/admin/dashboard` | `GetDashboardStatsQuery` | `admin.dashboard.view` |

## Admin — Covers (`/admin/covers`)

| Método | Ruta | Command/Query | Permiso |
|--------|------|---------------|---------|
| GET | `/` | `ListCoversAdminQuery` | `admin.covers.view` |
| GET | `/paged` | `ListCoversPagedAdminQuery` | `admin.covers.view` |
| POST | `/upload` | `ICoverImageStorage` directo | `admin.covers.manage` |
| GET/POST/PUT/DELETE | `/{id}` | CRUD covers | view/manage |
| PATCH | `/reorder` | `ReorderCoversCommand` | `admin.covers.manage` |

## Admin — Catálogo (`/admin/catalog/...`)

| Recurso | GET listado | POST crear | PUT editar | DELETE |
|---------|-------------|------------|------------|--------|
| Familias | `ListFamiliesAdminQuery` / paged | `SaveFamilyCommand` | `SaveFamilyCommand` | `DeleteFamilyCommand` |
| Categorías | `ListCategoriesPagedAdminQuery` | `SaveCategoryCommand` | idem | `DeleteCategoryCommand` |
| Subcategorías | `ListSubcategoriesPagedAdminQuery` | `SaveSubcategoryCommand` | idem | `DeleteSubcategoryCommand` |
| Productos | `ListProductsAdminQuery` | `SaveProductCommand` | idem | `DeleteProductCommand` |
| Variantes | — | `SaveVariantCommand` | idem | `DeleteVariantCommand` |

Alias en `/admin/families` y `/admin/products` (compatibilidad Laravel).

## Admin — Inventario (`/admin/inventory`)

| Método | Ruta | Command/Query | Permiso |
|--------|------|---------------|---------|
| GET | `/` | `ListInventoryAdminQuery` | `admin.stock.view` |
| GET | `/{variantId}` | `GetInventoryByVariantQuery` | `admin.stock.view` |
| PUT/PATCH | `/{variantId}` | `SetInventoryCommand` | `admin.stock.manage` |

## Admin — Pedidos (`/admin/orders`)

| Método | Ruta | Command/Query | Permiso |
|--------|------|---------------|---------|
| GET | `/` | `ListOrdersAdminQuery` | `admin.orders.view` |
| GET | `/{orderId}` | `GetAdminOrderQuery` | `admin.orders.view` |
| GET | `/{orderId}/ticket` | `GenerateOrderTicketPdfQuery` | `admin.orders.view` |
| PATCH/POST | `/{orderId}/ready-to-dispatch` | `MarkOrderReadyToDispatchCommand` | `admin.orders.manage` |

## Admin — Envíos y conductores

| Método | Ruta | Command/Query | Permiso |
|--------|------|---------------|---------|
| GET | `/admin/shipments` | `ListShipmentsAdminQuery` | `admin.shipments.view` |
| POST | `/admin/shipments` | `CreateShipmentCommand` | `admin.shipments.manage` |
| GET | `/admin/shipments/{id}/ticket.pdf` | `GenerateShipmentTicketPdfQuery` | `admin.shipments.view` |
| PATCH | `.../in-transit` | `MarkShipmentInTransitCommand` | `admin.shipments.manage` |
| PATCH | `.../delivered` | `MarkShipmentDeliveredCommand` | `admin.shipments.manage` |
| GET | `/admin/drivers` | `ListDriversAdminQuery` | `admin.drivers.view` |
| GET | `/admin/drivers/options` | `ListDriversOptionsQuery` | `admin.drivers.view` |
| POST/PUT/DELETE | `/admin/drivers/...` | `SaveDriverCommand`, etc. | `admin.drivers.manage` |

## Admin — Despacho (`/admin/dispatch/...`)

| Área | Rutas clave | Commands/Queries |
|------|-------------|------------------|
| Settings | GET/POST `/settings` | `GetDispatchSettingsQuery`, `UpdateDispatchSettingsCommand` |
| Cola | GET `/queue` | `GetDispatchQueueQuery` |
| Lotes | POST `/batches/auto`, `/manual` | `AutoCreateDispatchBatchesCommand`, `ManualCreateDispatchBatchCommand` |
| Rutas | GET/POST `/routes/...` | `ListDeliveryRoutesQuery`, `AssignDeliveryRouteCommand`, `StartDeliveryRouteCommand` |
| Paradas | POST `/stops/{id}/delivered` | `MarkStopDeliveredCommand` |

## Admin — Usuarios y roles

| Método | Ruta | Command/Query | Permiso |
|--------|------|---------------|---------|
| GET | `/admin/users` | `ListUsersAdminQuery` | `admin.users.view` |
| POST | `/admin/users` | `CreateUserAdminCommand` | `admin.users.manage` |
| PUT | `/admin/users/{id}` | `UpdateUserAdminCommand` | `admin.users.manage` |
| GET | `/admin/roles` | `ListRolesAdminQuery` | `admin.roles.view` |
| PUT | `/admin/roles/{id}/permissions` | `UpdateRolePermissionsCommand` | `admin.roles.manage` |
| GET | `/admin/permissions` | `ListPermissionsAdminQuery` | `admin.roles.view` |

## Repartidor (`DriverEndpoints.cs`) — `/api/v1/driver`

Requiere JWT + rol `driver`.

| Método | Ruta | Query/Command |
|--------|------|---------------|
| GET | `/me` | `GetDriverProfileQuery` |
| GET | `/shipments` | `ListMyShipmentsQuery` |
| PATCH | `/shipments/{id}/in-transit` | `DriverUpdateShipmentStatusCommand` |
| PATCH | `/shipments/{id}/delivered` | `DriverUpdateShipmentStatusCommand` |

---

# Parte VI — Frontend (enlace al repo web)

> La documentación del frontend vive en **[ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web)** — no en este repo del API.

| Documento | Enlace |
|-----------|--------|
| **Tutorial completo** | [ecommerce-web/docs/00-guia-para-principiantes.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md) |
| Referencia FSD | [10-referencia-fsd-completo.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/10-referencia-fsd-completo.md) |
| Inventario archivos | [INVENTARIO-ARCHIVOS.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/INVENTARIO-ARCHIVOS.md) |
| Índice docs web | [docs/README.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/README.md) |

**Resumen:** Next.js 16 + FSD (`shared` → `entities` → `features` → `widgets` → `views` → `app`). Auth con cookie `accessToken` + JWT en fetch. Admin con `useAdminTableQuery` + `AdminDataTable` + configs `domain/*.table.ts`.

---

# Parte VII — Referencia y siguientes pasos

## Glosario

| Término | Significado |
|---------|-------------|
| **Handler** | Clase que ejecuta un command/query |
| **DTO** | Objeto de transferencia (JSON) |
| **Repositorio** | Abstrae acceso a datos |
| **Scoped** | Una instancia por petición HTTP |
| **Seed** | Datos iniciales de BD |
| **Minimal API** | Rutas sin controllers clásicos |
| **FluentResults** | Resultado éxito/fallo sin excepciones |
| **FSD** | Feature-Sliced Design en el frontend |

## Orden de lectura del código (recomendado)

| # | Archivo | Por qué |
|---|---------|---------|
| 1 | `Api/Program.cs` | Ves todo el pipeline |
| 2 | `Endpoints/AuthEndpoints.cs` | Endpoint fino típico |
| 3 | `Features/Auth/AuthHandlers.cs` | Handler real |
| 4 | `Extensions/ResultExtensions.cs` | Cómo errores → HTTP |
| 5 | `Infrastructure/DependencyInjection.cs` | Qué se registra en DI |
| 6 | `Features/Checkout/CheckoutHandlers.cs` | Transacción compleja |
| 7 | `Endpoints/AdminEndpoints.cs` | Permisos admin |
| 8 | `Persistence/Sql/EcommerceDbContext.cs` | Modelo de BD |
| 9 | `Domain/Entities/Order.cs` | Entidad central |
| 10 | `Features/Admin/Lists/ListProductsAdminHandler.cs` | Paginación admin |

## Tests

```powershell
cd Ecommerce-api
dotnet test
```

## Regenerar seed

```powershell
node scriptsSql/tools/generate-seed.mjs
cd scriptsSql
.\run-all.ps1
```

## Problemas frecuentes

| Síntoma | Solución |
|---------|----------|
| Build: exe bloqueado | Cierra API en puerto 5088 |
| `/ready` → 503 | `scriptsSql/run-all.ps1` |
| Login 401 | Credenciales del seed; reinicia API |
| PDF 401 en Postman | Usa admin web o fetch autenticado |
| Frontend no conecta | API en 5088 + `NEXT_PUBLIC_API_URL` |
| CORS | Añade origen en `Cors:Origins` |

## Documentación relacionada

| Documento | Contenido |
|-----------|-----------|
| [README índice](./README.md) | Índice técnico |
| [01-arquitectura.md](./01-arquitectura.md) | Diagramas de capas |
| [03-api-endpoints.md](./03-api-endpoints.md) | Todas las rutas |
| [04-autenticacion-y-permisos.md](./04-autenticacion-y-permisos.md) | JWT y permisos |
| [05-dominio-y-base-de-datos.md](./05-dominio-y-base-de-datos.md) | Entidades y relaciones |
| [06-flujos-de-negocio.md](./06-flujos-de-negocio.md) | Checkout, stock, despacho |
| [Guía frontend (repo ecommerce-web)](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md) | Tutorial frontend de cero a código |
| [Referencia FSD (repo ecommerce-web)](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/10-referencia-fsd-completo.md) | Referencia técnica frontend |
| [scriptsSql/README.md](../scriptsSql/README.md) | Schema y seed |
| [postman/README.md](../postman/README.md) | Colección Postman |

## Ejercicios finales (para cerrar el tutorial)

| # | Ejercicio | Qué aprendes |
|---|-----------|--------------|
| 1 | Breakpoint en `LoginCommandHandler` + login Postman | Flujo MediatR completo |
| 2 | Breakpoint en `CreateOrderCommandHandler` + checkout | Transacciones EF |
| 3 | Quitar permiso `admin.products.view` al rol admin en BD → 403 en web | JWT claims |
| 4 | Añadir producto desde `/admin/products` y verlo en catálogo | CRUD end-to-end |
| 5 | Crear pedido → pagar → ready-to-dispatch → envío → PDF | Ciclo de negocio |
| 6 | `dotnet test` y leer un test de integración | WebApplicationFactory |
| 7 | Regenerar seed con `BULK_COUNT=100` | Scripts SQL |
| 8 | Traza una ruta del [catálogo Parte V](#parte-v--catálogo-completo-de-endpoints) en el IDE | Navegación del código |

## Tu siguiente paso

1. Completa la **Parte I** (pasos 0–9) con tus propias manos.
2. Lee la **Parte II** para entender el *porqué*.
3. Recorre la **Parte III** con el IDE abierto: abre cada archivo mientras lees.
4. Sigue la **Parte IV** con breakpoints en Postman.
5. Usa la **Parte V** como mapa cuando busques una ruta concreta.
6. Abre el frontend con la **[guía en ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md)** (Parte VI es solo resumen).
7. Haz los **ejercicios finales** de arriba.
8. Profundiza en [03-api-endpoints.md](./03-api-endpoints.md) y la [guía frontend en ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md).
