# Evolución: CQRS, dominio, FluentResults y pipeline

Documento de arquitectura del backend Ecommerce. **Estado:** CQRS con MediatR + FluentResults aplicado en **todos los módulos** (Auth, Catalog, Cart, Checkout, Orders, Addresses, Admin). La carpeta `Application/Services/` ya no contiene servicios de aplicación; la lógica vive en `Features/*/`.

---

## 1. Lo que ya está bien (no tirar)

| Aspecto | Estado actual |
|---------|----------------|
| Separación en capas | `Domain` → `Application` → `Infrastructure` → `Api` |
| Dominio sin dependencias | Entidades y enums en `Ecommerce.Domain` |
| Contratos en Application | `I*Repository`, `IJwtTokenService`, handlers MediatR |
| Validación de entrada | FluentValidation + `ValidationBehavior` (pipeline MediatR) |
| Transacciones críticas | `IUnitOfWork` en checkout y pago |
| Tests de integración | Flujo compra con WebApplicationFactory |

Eso **ya es Clean Architecture** aplicada con CQRS. La lógica está en `Features/` por módulo; los endpoints solo envían commands/queries y traducen `Result` a HTTP.

---

## 2. Mapa: comentario → situación actual → objetivo

### 2.1 CQRS bajo Clean Architecture

**Antes:** un `*Service` por área con GET y POST mezclados.

**Hoy (implementado):** separación explícita:

| Lado | Responsabilidad | Ejemplos |
|------|-----------------|----------|
| **Queries** (lectura) | Solo leer, sin efectos secundarios, DTOs/proyecciones | `GetProductBySlugQuery`, `ListOrdersQuery` |
| **Commands** (escritura) | Crear/actualizar/borrar, transacciones, reglas de negocio | `CreateOrderCommand`, `SaveAddressCommand` |

**Implementación típica en .NET:** [MediatR](https://github.com/jbogard/MediatR) con `IRequest<T>` / `IRequestHandler<,>` y carpetas `Features/{Módulo}/Commands` y `Features/{Módulo}/Queries`.

**Estructura propuesta en `Application`:**

```
Ecommerce.Application/
├── Common/
│   ├── Behaviors/          # Pipeline (validación, logging)
│   ├── Results/            # Mapeo FluentResults → HTTP
│   └── Interfaces/
├── Features/
│   ├── Catalog/
│   │   ├── Queries/
│   │   └── Commands/       # si hay escritura pública admin
│   ├── Addresses/
│   │   ├── Queries/
│   │   └── Commands/
│   └── Orders/
│       ├── Queries/
│       └── Commands/
└── Abstractions/           # contratos que Infrastructure implementa
```

Los **endpoints** quedan finos: solo envían el command/query al mediator y traducen el `Result` a `Results.Ok` / `Results.NotFound` / etc.

**Migración:** módulo a módulo. Piloto recomendado: **Direcciones** (CRUD acotado, sin transacciones complejas). Luego **Catálogo (solo queries)**. Después **Checkout/Orders (commands)**.

---

### 2.2 Validaciones de dominio (además de FluentValidation)

**Hoy:**

- **Entrada:** `Validators/*` (longitud, campos requeridos).
- **Negocio:** reglas dispersas en servicios (`if (order.Status != …) throw InvalidOperationException`, stock en `InventoryRepository`).

**Problema que señalan:** validar el JSON no garantiza invariantes al persistir (unicidad de slug/email, stock, transiciones de estado, límites alineados con BD).

**Objetivo:** reglas que viven en el **dominio** o en **domain services** y se ejecutan **siempre** antes de guardar, no solo en el endpoint.

| Tipo | Dónde | Ejemplo |
|------|-------|---------|
| Invariantes de entidad | Métodos en la entidad o value objects | `Address.Create(...)`, `Order.MarkPaid()` |
| Reglas que necesitan BD | `IAddressUniquenessChecker`, `IStockPolicy` en Domain/Application, impl en Infrastructure | “Slug de producto único” |
| Validación de transición | Enum + método en agregado | `Order` solo pasa a `Paid` desde `PendingPayment` |

**Ejemplo conceptual (dirección):**

```csharp
// Domain/Addresses/Address.cs (evolución)
public static Result<Address> Create(Guid userId, string label, string street, ...)
{
    if (label.Length > 100) return Result.Fail("Label demasiado largo");
    // ...
    return Result.Ok(new Address { ... });
}
```

El **command handler** llama a `Address.Create`, luego al repositorio; si falla, devuelve `Result` sin excepción.

**No duplicar:** FluentValidation sigue para forma del request; dominio valida **significado** y **persistencia**.

---

### 2.3 “Decoradores” / pipeline automático

**Hoy:** `ValidationFilter<T>` solo en endpoints que llaman `.WithValidation<T>()`.

**Objetivo:** un **pipeline** que ejecute en orden, para cada command/query:

1. Validación FluentValidation del request (si aplica).
2. (Opcional) validación de dominio en el handler o en un behavior.
3. Handler.
4. (Futuro) auditoría / logging.

Con MediatR esto son **Pipeline Behaviors**:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Ejecuta todos IValidator<TRequest> antes del handler
}
```

Ventaja: **no** repetir `.WithValidation` en cada endpoint; el handler siempre recibe datos ya validados en entrada.

Los endpoints pueden quedar así:

```csharp
addresses.MapPost("/", async (SaveAddressCommand cmd, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(cmd, ct);
    return result.ToHttpResult(); // extensión que mapea Result → IResult
});
```

---

### 2.4 FluentResults en lugar de excepciones “normales”

**Hoy:** `NotFoundException`, `InvalidOperationException`, `InsufficientStockException` → `ExceptionMiddleware`.

**Objetivo:**

| Situación | Enfoque |
|-----------|---------|
| Esperado (no encontrado, stock, regla de negocio) | `Result` / `Result<T>` con errores tipados |
| Bug / fallo infra (BD caída, null inesperado) | Excepción + 500 + log |

**Ventajas:** flujo explícito en handlers (`if (result.IsFailed) return result`), OpenAPI más predecible, menos `try/catch` en servicios.

**Paquete:** [FluentResults](https://github.com/altmann/FluentResults).

**Convención sugerida:**

```csharp
// Errores de dominio reutilizables
public static class AddressErrors
{
    public static Error NotFound(Guid id) => new($"Address {id} not found");
    public static Error LabelTooLong => new("Label exceeds 100 characters");
}
```

**HTTP:** extensión `ResultExtensions.ToHttpResult()` en Api:

| Result | HTTP |
|--------|------|
| Success | 200/201 |
| NotFound | 404 |
| Conflict (stock) | 409 |
| Validation | 400 con lista de errores |
| Unauthorized | 401 |

`ExceptionMiddleware` se mantiene solo para lo **no** controlado.

---

### 2.5 Proyecciones EF Core en lecturas (en lugar de mapear entidad completa)

**Hoy:** repositorio carga `Product` con `Include(Variants)`, `Include(Images)` y `CatalogMapping.ToDetail()` en memoria.

**Objetivo en queries:**

```csharp
await db.Products.AsNoTracking()
    .Where(p => p.Slug == slug && p.IsActive)
    .Select(p => new ProductDetailDto(
        p.Id,
        p.Name,
        p.Slug,
        p.Description,
        p.BasePrice,
        p.Variants.Where(v => v.IsActive).Select(v => new ProductVariantDto(...)).ToList(),
        p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList()
    ))
    .FirstOrDefaultAsync(ct);
```

**Beneficios:** menos datos transferidos, sin tracking, SQL más enfocado. Encaja con **Query handlers** y repositorios de solo lectura (`IProductReadRepository`).

**Regla:** Commands pueden seguir usando entidades tracked; Queries usan `AsNoTracking()` + `Select`.

---

### 2.6 Interceptores EF para tabla de logs (futuro)

Cuando exista tabla `audit_logs` (o similar):

```csharp
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
    {
        // Antes de SaveChanges: capturar entidades Added/Modified/Deleted
        // Insertar filas en audit_logs en la misma transacción
    }
}
```

Registrar en `AddDbContext`:

```csharp
options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
```

**No implementar aún** hasta definir esquema (qué campos, quién, PII). Dejar previsto en Infrastructure.

---

## 3. Cómo encaja con los cuatro proyectos actuales

```
Domain          Entidades ricas, value objects, errores de dominio (opcional)
                Interfaces de políticas (IEmailUniquenessChecker)

Application     Commands, Queries, Handlers, Behaviors, FluentValidation
                NO referencia EF

Infrastructure  Command repos (tracked), Query repos (proyecciones)
                Interceptores, JWT, PDF

Api             Endpoints → ISender.Send
                Result → HTTP, ExceptionMiddleware para fallos graves
```

Los `*Service` fueron **sustituidos** por handlers; los endpoints usan `ISender.Send` exclusivamente.

---

## 4. Plan de migración por fases (recomendado)

| Fase | Entregable | Riesgo |
|------|------------|--------|
| **0** | Paquetes: MediatR, FluentResults. `ValidationBehavior`, `ResultExtensions`. Doc + 1 test | Bajo |
| **1** | Módulo **Addresses**: Commands/Queries + proyección en listado + dominio básico | Bajo |
| **2** | **Catalog** solo queries (home, slug, products con `Select`) | Medio |
| **3** | **Cart** commands; guest merge como command | Medio |
| **4** | **Checkout/Orders** commands + `Result` para stock (sin excepción 409) | Alto |
| **5** | Admin por subdominios (covers, catalog CRUD) | Alto |
| **6** | Deprecar `Services/*` y `I*Service` | **Completado** |
| **7** | Tabla audit + interceptor | Cuando haya requisito |

Cada fase: tests de integración existentes deben seguir en verde.

---

## 5. Qué NO haría de golpe

- Reescribir los ~15 servicios en un solo PR.
- CQRS “puro” con event sourcing (overkill para este ecommerce).
- Duplicar validación idéntica en FluentValidation y dominio (solo reglas de negocio en dominio).
- Eliminar `ExceptionMiddleware` (sigue siendo necesario).

---

## 6. Piloto Addresses — completado

- [x] `SaveAddressCommand` + `SaveAddressCommandValidator`
- [x] `AddressRules` / `AddressErrors` en Domain
- [x] Handlers con `Result` / `Result<T>`
- [x] `IAddressReadRepository` + `IAddressWriteRepository` (proyecciones en lectura)
- [x] Endpoints `/addresses` → `ISender` + `ToHttpResult()`
- [x] URLs Postman sin cambios

## 6b. Catalog queries — completado

- [x] `ICatalogReadRepository` con proyecciones EF (`Select` → DTO)
- [x] Queries: home, covers, families, slugs, products, search
- [x] Endpoints `/catalog` → MediatR

## 6c. Cart, Checkout y Orders — completado

- [x] Commands: carrito (add/update/remove/clear/merge), `CreateOrder`, `PayOrder`
- [x] Queries: listado y detalle de pedidos del usuario
- [x] `IOrderReadRepository` con proyección en listado
- [x] Stock insuficiente → `Result` 409 (sin excepción en flujo normal)
- [x] Endpoints `/cart`, `/checkout`, `/orders` → MediatR

## 6d. Auth y Admin — completado

- [x] `LoginCommand`, `RegisterCommand`, `RefreshTokenCommand`, `LogoutCommand`, `GetMeQuery`
- [x] Admin: dashboard, covers, catálogo CRUD, inventario, pedidos, envíos, conductores, opciones de producto
- [x] Endpoints `/auth` y `/admin` → `ISender` + `ToHttpResult()`
- [x] Eliminados `Services/*` e `I*Service` de aplicación

## 6e. Pendiente (futuro)

- [ ] Interceptor audit activo (tabla `audit_logs`)
- [ ] Job de liberación de reservas de stock expiradas
- [ ] Más reglas de dominio en entidades (`Order.MarkPaid()`, etc.)

---

## 7. Resumen para quien revisó el código

| Recomendación | ¿Aplica al proyecto? | Prioridad |
|---------------|----------------------|-----------|
| CQRS + Clean Architecture | Sí, como evolución de `Services` | Alta |
| Validación dominio | Sí, sobre todo órdenes, stock, catálogo admin | Alta |
| Pipeline / behaviors | Sí, reemplaza validación solo en filtro HTTP | Media |
| FluentResults | Sí, para casos de negocio esperados | Media |
| Proyecciones EF en reads | Sí, en catálogo y listados admin | Media |
| Interceptor audit | Sí, cuando exista tabla de logs | Baja (futuro) |

La base de capas **ya es correcta**; el siguiente salto es **organizar por intención (leer vs escribir)** y **hacer explícitos los fallos de negocio**, no ampliar un mismo servicio con más métodos.

---

## Referencias en el repo

- Arquitectura actual: [`01-arquitectura.md`](01-arquitectura.md)
- Endpoints: [`03-api-endpoints.md`](03-api-endpoints.md)
- Flujos: [`06-flujos-de-negocio.md`](06-flujos-de-negocio.md)
