# Autenticación y permisos

## Esquema JWT

1. **Login** valida email/password (BCrypt).
2. Se genera **access token** (JWT corto) con claims de usuario y permisos.
3. Se genera **refresh token** (aleatorio, solo hash en BD).
4. El cliente envía `Authorization: Bearer {accessToken}` en rutas protegidas.

### Claims en el access token

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

- Almacenado como **SHA-256 hash** en tabla `refresh_tokens`.
- **Refresh:** revoca tokens previos del usuario y emite par nuevo.
- **Logout:** marca `RevokedAt` en tokens activos del usuario.

## Servicios involucrados

| Componente | Archivo |
|------------|---------|
| `IAuthService` / `AuthService` | `Application/Services/AuthService.cs` |
| `IJwtTokenService` / `JwtTokenService` | `Infrastructure/Identity/JwtTokenService.cs` |
| `IUserRepository` | `Infrastructure/.../UserRepository.cs` |

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

> **Nota:** Entidades `Cover` y `ProductOption` existen en dominio; endpoints admin de covers/options pueden ampliarse en el futuro.
