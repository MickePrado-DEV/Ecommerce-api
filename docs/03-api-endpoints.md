# Endpoints API

Base: `http://localhost:5088/api/v1`

Leyenda: 🔓 público · 🔐 JWT requerido · 👑 admin + permiso

> Colección Postman actualizada: `postman/Ecommerce-API.postman_collection.json`

**Errores de negocio:** la API devuelve JSON con `errors` y códigos (`Validation`, `NotFound`, `Conflict`, `Unauthorized`) vía FluentResults. Ver [06-flujos-de-negocio.md](./06-flujos-de-negocio.md#5-manejo-de-errores).

---

## Sistema

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/health` | 🔓 | `{ status: "ok" }` |
| GET | `/ready` | 🔓 | BD accesible (`503` si no) |
| GET | `/openapi/v1.json` | 🔓 | Contrato OpenAPI (no producción) |
| GET | `/scalar/v1` | 🔓 | UI Scalar (no producción) |

---

## Auth — `/api/v1/auth`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/register` | 🔓 | Registro de usuario (BCrypt) |
| POST | `/login` | 🔓 | Login → `accessToken`, `refreshToken`, `user`, `permissions` |
| POST | `/refresh` | 🔓 | Renueva tokens con refresh válido |
| POST | `/logout` | 🔐 | Revoca refresh tokens del usuario |
| GET | `/me` | 🔐 | Perfil del usuario autenticado |

### Body login / register

```json
{ "email": "cliente@ecommerce.local", "password": "Cliente123!" }
```

---

## Catálogo (tienda) — `/api/v1/catalog`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/home?take=12` | 🔓 | Portadas + últimos productos |
| GET | `/covers` | 🔓 | Portadas activas |
| GET | `/products/latest?take=12` | 🔓 | Últimos N productos |
| GET | `/families` | 🔓 | Árbol familias → categorías → subcategorías |
| GET | `/families/{slug}` | 🔓 | Familia + categorías hijas |
| GET | `/categories/{slug}` | 🔓 | Categoría + subcategorías |
| GET | `/subcategories/{slug}` | 🔓 | Subcategoría |
| GET | `/products` | 🔓 | Listado paginado con filtros |
| GET | `/search?q=` | 🔓 | Alias de búsqueda en productos |
| GET | `/products/{slug}` | 🔓 | Detalle con variantes, precios e imágenes |

### Query `GET /products`

| Parámetro | Descripción |
|-----------|-------------|
| `page`, `pageSize` | Paginación (default 1, 20) |
| `familyId`, `categoryId`, `subCategoryId` | Filtro jerárquico |
| `q` | Búsqueda por nombre/descripción |
| `sort` | `price:asc`, `price:desc`, `recent` (o `1`/`2`/`3`) |

---

## Carrito — `/api/v1/cart`

| Método | Ruta | Auth | Headers | Descripción |
|--------|------|------|---------|-------------|
| GET | `/` | opcional | `X-Guest-Token` o JWT | Ver carrito |
| POST | `/items` | opcional | idem | Agregar variante (`variantId`, `quantity`) |
| PUT / PATCH | `/items/{itemId}` | opcional | idem | Actualizar cantidad |
| DELETE | `/items/{itemId}` | opcional | idem | Quitar línea |
| DELETE | `/` | opcional | idem | Vaciar carrito |
| POST | `/merge` | 🔐 | — | Fusionar carrito invitado (`guestToken`) |

**Invitado:** header `X-Guest-Token` (GUID). Si no existe, la API crea uno y lo devuelve en la respuesta.

**Usuario:** JWT de cliente; el carrito se asocia a `userId`.

---

## Direcciones — `/api/v1/addresses`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/` | 🔐 | Listado del usuario |
| GET | `/{id}` | 🔐 | Detalle |
| POST | `/` | 🔐 | Crear |
| PUT | `/{id}` | 🔐 | Actualizar |
| DELETE | `/{id}` | 🔐 | Eliminar |
| PATCH | `/{id}/default` | 🔐 | Marcar predeterminada |

### Body crear/actualizar

```json
{
  "label": "Casa",
  "street": "Av. Reforma 123",
  "city": "Ciudad de México",
  "state": "CDMX",
  "postalCode": "06600",
  "country": "MX",
  "phone": "5551234567",
  "isDefault": true
}
```

---

## Checkout — `/api/v1/checkout`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/` | 🔐 | Convierte carrito en pedido + reserva stock |
| POST | `/{orderId}/pay` | 🔐 | Pago mock (alias de orders pay) |

### Body checkout

Usar **`addressId`** (dirección guardada) **o** datos inline:

```json
{
  "addressId": "00000000-0000-0000-0000-000000000000",
  "shippingCost": 99.00
}
```

O sin `addressId`:

```json
{
  "fullName": "Cliente Demo",
  "street": "Av. Reforma 123",
  "city": "Ciudad de México",
  "state": "CDMX",
  "postalCode": "06600",
  "country": "MX",
  "phone": "5551234567",
  "shippingCost": 99.00
}
```

Respuesta: `orderId`, `orderNumber`, `total`, `status` (`PendingPayment`).

---

## Pedidos (cliente) — `/api/v1/orders`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/` | 🔐 | Historial de pedidos del usuario |
| GET | `/{orderId}` | 🔐 | Detalle de un pedido propio |
| POST | `/{orderId}/pay` | 🔐 | Pago mock (confirma stock, estado → `Paid`) |
| POST | `/{orderId}/retry-payment` | 🔐 | Reintento si `PaymentFailed` / `PendingPayment` |

---

## Admin — `/api/v1/admin`

Todos requieren JWT de admin con el permiso indicado.

### General

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/dashboard` | `admin.dashboard.view` | Stats (alias de `/dashboard/stats`) |
| GET | `/dashboard/stats` | `admin.dashboard.view` | Contadores pedidos/productos/usuarios |

### Portadas — `/admin/covers`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/` | `admin.covers.view` | Listar |
| GET | `/{id}` | `admin.covers.view` | Detalle |
| POST | `/` | `admin.covers.manage` | Crear |
| PUT | `/{id}` | `admin.covers.manage` | Actualizar |
| DELETE | `/{id}` | `admin.covers.manage` | Eliminar |
| PATCH | `/reorder` | `admin.covers.manage` | Body: `{ "ids": [guid, ...] }` |

### Catálogo — `/admin/catalog` (y aliases `/admin/families`, `/admin/products`)

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/catalog/families` | `admin.families.view` | Listar familias |
| POST | `/catalog/families` | `admin.families.manage` | Crear familia |
| PUT | `/catalog/families/{id}` | `admin.families.manage` | Actualizar |
| DELETE | `/catalog/families/{id}` | `admin.families.manage` | Eliminar |
| POST | `/catalog/categories` | `admin.categories.manage` | Crear categoría |
| PUT | `/catalog/categories/{id}` | `admin.categories.manage` | Actualizar |
| DELETE | `/catalog/categories/{id}` | `admin.categories.manage` | Eliminar |
| POST | `/catalog/subcategories` | `admin.subcategories.manage` | Crear subcategoría |
| PUT | `/catalog/subcategories/{id}` | `admin.subcategories.manage` | Actualizar |
| DELETE | `/catalog/subcategories/{id}` | `admin.subcategories.manage` | Eliminar |
| GET | `/catalog/products?page&pageSize` | `admin.products.view` | Listar productos |
| POST | `/catalog/products` | `admin.products.manage` | Crear producto |
| PUT | `/catalog/products/{id}` | `admin.products.manage` | Actualizar |
| DELETE | `/catalog/products/{id}` | `admin.products.manage` | Baja lógica |
| POST | `/catalog/variants` | `admin.products.manage` | Crear variante |
| PUT | `/catalog/variants/{id}` | `admin.products.manage` | Actualizar variante |
| PUT | `/variants/{id}` | `admin.products.manage` | Alias actualizar variante |
| DELETE | `/catalog/variants/{id}` | `admin.products.manage` | Baja lógica variante |

### Opciones por producto — `/admin/products/{productId}/options`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/options` | `admin.options.view` | Listar opciones + valores |
| POST | `/options` | `admin.options.manage` | Crear opción |
| PUT | `/options/{optionId}` | `admin.options.manage` | Actualizar opción |
| DELETE | `/options/{optionId}` | `admin.options.manage` | Eliminar opción |
| POST | `/options/{optionId}/values` | `admin.options.manage` | Crear valor |
| PUT | `/options/{optionId}/values/{valueId}` | `admin.options.manage` | Actualizar valor |
| DELETE | `/options/{optionId}/values/{valueId}` | `admin.options.manage` | Eliminar valor |

### Inventario — `/admin/inventory`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/` | `admin.stock.view` | Stock por variante |
| GET | `/{variantId}` | `admin.stock.view` | Detalle de una variante |
| PUT / PATCH | `/{variantId}` | `admin.stock.manage` | Fijar `quantityOnHand` |

### Pedidos — `/admin/orders`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/?page&pageSize&status` | `admin.orders.view` | Listado admin |
| GET | `/{orderId}` | `admin.orders.view` | Detalle |
| GET | `/{orderId}/ticket` | `admin.orders.view` | PDF ticket por pedido |
| POST | `/{orderId}/ready` | `admin.orders.manage` | `Paid` → `ReadyToDispatch` |
| PATCH | `/{orderId}/ready-to-dispatch` | `admin.orders.manage` | Mismo efecto que POST ready |

### Envíos y conductores

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/shipments?page&pageSize` | `admin.shipments.view` | Listar envíos |
| POST | `/shipments` | `admin.shipments.manage` | Crear envío + ticket |
| GET | `/shipments/{id}/ticket.pdf` | `admin.shipments.view` | PDF por envío |
| PATCH | `/shipments/{id}/in-transit` | `admin.shipments.manage` | En tránsito |
| PATCH | `/shipments/{id}/delivered` | `admin.shipments.manage` | Entregado |
| GET | `/drivers` | `admin.drivers.view` | Listar repartidores |
| POST | `/drivers` | `admin.drivers.manage` | Crear |
| PUT | `/drivers/{id}` | `admin.drivers.manage` | Actualizar |
| DELETE | `/drivers/{id}` | `admin.drivers.manage` | Eliminar |

---

## Códigos HTTP habituales

| Código | Cuándo |
|--------|--------|
| 200 | OK |
| 204 | Sin body (logout, delete, PATCH estado) |
| 400 | Validación FluentValidation o regla de negocio |
| 401 | Sin token o credenciales inválidas |
| 403 | Sin permiso admin |
| 404 | Recurso no encontrado |
| 409 | Stock insuficiente |
| 500 | Error no controlado |
| 503 | `/ready` cuando BD no responde |
