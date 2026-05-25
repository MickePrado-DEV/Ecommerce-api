# Autenticación y permisos

## Esquema JWT

1. **Login** (`LoginCommand`) valida email/password con BCrypt.
2. Se genera **access token** (JWT corto) con claims de usuario y permisos.
3. Se genera **refresh token** (aleatorio; solo el hash se guarda en BD).
4. El cliente envía `Authorization: Bearer {accessToken}` en rutas protegidas.

## Commands y queries (CQRS)

| Operación | Tipo MediatR | Handler |
|-----------|--------------|---------|
| Register | `RegisterCommand` | `RegisterCommandHandler` |
| Login | `LoginCommand` | `LoginCommandHandler` |
| Refresh | `RefreshTokenCommand` | `RefreshTokenCommandHandler` |
| Logout | `LogoutCommand` | `LogoutCommandHandler` |
| Perfil | `GetMeQuery` | `GetMeQueryHandler` |

Código: `Application/Features/Auth/AuthHandlers.cs`  
Validación: `Application/Features/Auth/Validators/AuthCommandValidators.cs`  
Errores: `Domain/Auth/AuthErrors.cs`

## Claims en el access token

| Claim | Contenido |
|-------|-----------|
| `nameid` | `User.Id` |
| `email` | Email |
| `given_name` / `surname` | Nombre y apellido |
| `permission` | Un claim por cada permiso del rol (repetido) |

## Roles y permisos (seed)

| Rol | Código | Permisos |
|-----|--------|----------|
| Administrador | `admin` | Todos los de `AdminPermissions.All` |
| Cliente | `customer` | Ninguno admin |

El admin recibe **21 permisos** granulares (`admin.products.manage`, `admin.orders.view`, etc.).

## Políticas de autorización

En `Program.cs`, cada permiso se registra como política ASP.NET Core:

```csharp
o.AddPolicy(perm, p => p.RequireClaim("permission", perm));
```

Los endpoints admin usan `.RequireAuthorization(AdminPermissions.ProductsManage)` etc.

Un cliente con JWT válido pero **sin** el claim recibe **403 Forbidden**.

## Refresh token

- Almacenado como **SHA-256 hash** en tabla `refresh_tokens` (`AuthTokenHasher` en Application).
- **Refresh:** revoca tokens previos del usuario y emite par nuevo.
- **Logout:** revoca tokens activos del usuario (`RevokedAt`).

## Respuestas HTTP (FluentResults)

| Caso | Código metadata | HTTP |
|------|-----------------|------|
| Credenciales inválidas (login/refresh) | `Unauthorized` | 401 |
| Email ya registrado | `Conflict` | 409 |
| Usuario no encontrado (me) | `NotFound` | 404 |
| Validación de command | `Validation` | 400 |

Mapeo en `Api/Extensions/ResultExtensions.cs`.

## Componentes técnicos

| Componente | Ubicación |
|------------|-----------|
| Handlers auth | `Application/Features/Auth/AuthHandlers.cs` |
| `IJwtTokenService` / `JwtTokenService` | `Infrastructure/Identity/JwtTokenService.cs` |
| `IUserRepository` | `Infrastructure/Persistence/Sql/Repositories/UserRepository.cs` |
| Endpoints | `Api/Endpoints/AuthEndpoints.cs` |

## Lista completa de permisos admin

Definidos en `Application/Authorization/AdminPermissions.cs`:

- `admin.dashboard.view`
- `admin.covers.view` / `admin.covers.manage`
- `admin.families.view` / `admin.families.manage`
- `admin.categories.view` / `admin.categories.manage`
- `admin.subcategories.view` / `admin.subcategories.manage`
- `admin.products.view` / `admin.products.manage`
- `admin.options.view` / `admin.options.manage`
- `admin.stock.view` / `admin.stock.manage`
- `admin.drivers.view` / `admin.drivers.manage`
- `admin.orders.view` / `admin.orders.manage`
- `admin.shipments.view` / `admin.shipments.manage`

> **Covers y opciones:** `admin.covers.*` → `/admin/covers`. `admin.options.*` → `/admin/products/{productId}/options` (opciones por producto, no globales tipo Laravel).
