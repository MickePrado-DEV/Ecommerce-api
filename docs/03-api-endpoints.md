# Endpoints API

Base: `http://localhost:5088/api/v1`

Leyenda: 🔓 público · 🔐 JWT requerido · 👑 admin + permiso

---

## Sistema

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/health` | 🔓 | Estado de la API y conexión a BD |

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
| GET | `/families` | 🔓 | Árbol familias → categorías → subcategorías activas |
| GET | `/products?page=1&pageSize=20&q=` | 🔓 | Listado paginado de productos activos |
| GET | `/products/{slug}` | 🔓 | Detalle con variantes, precios e imágenes |

---

## Carrito — `/api/v1/cart`

| Método | Ruta | Auth | Headers | Descripción |
|--------|------|------|---------|-------------|
| GET | `/` | opcional | `X-Guest-Token` (invitado) o JWT | Ver carrito |
| POST | `/items` | opcional | idem | Agregar variante (`variantId`, `quantity`) |
| PUT | `/items/{itemId}` | opcional | idem | Actualizar cantidad |
| DELETE | `/items/{itemId}` | opcional | idem | Quitar línea |

**Invitado:** enviar header `X-Guest-Token` (GUID). Si no existe, la API crea uno y lo devuelve en la respuesta del carrito.

**Usuario:** JWT de cliente; el carrito se asocia a `userId`.

---

## Checkout — `/api/v1/checkout`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/` | 🔐 | Convierte carrito en pedido + reserva stock |

### Body

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

---

## Admin — `/api/v1/admin`

Todos requieren JWT de admin con el permiso indicado.

### General

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/dashboard` | `admin.dashboard.view` | Ping de panel admin |

### Catálogo — `/admin/catalog`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/families` | `admin.families.view` | Listar familias |
| POST | `/families` | `admin.families.manage` | Crear familia |
| PUT | `/families/{id}` | `admin.families.manage` | Actualizar |
| DELETE | `/families/{id}` | `admin.families.manage` | Eliminar |
| POST | `/categories` | `admin.categories.manage` | Crear categoría |
| PUT | `/categories/{id}` | `admin.categories.manage` | Actualizar |
| DELETE | `/categories/{id}` | `admin.categories.manage` | Eliminar |
| POST | `/subcategories` | `admin.subcategories.manage` | Crear subcategoría |
| PUT | `/subcategories/{id}` | `admin.subcategories.manage` | Actualizar |
| DELETE | `/subcategories/{id}` | `admin.subcategories.manage` | Eliminar |
| GET | `/products?page&pageSize` | `admin.products.view` | Listar productos |
| POST | `/products` | `admin.products.manage` | Crear producto |
| PUT | `/products/{id}` | `admin.products.manage` | Actualizar |
| DELETE | `/products/{id}` | `admin.products.manage` | Baja lógica (`IsActive=false`) |
| POST | `/variants` | `admin.products.manage` | Crear variante (+ stock inicial opcional) |
| PUT | `/variants/{id}` | `admin.products.manage` | Actualizar variante |
| DELETE | `/variants/{id}` | `admin.products.manage` | Baja lógica variante |

### Inventario — `/admin/inventory`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/` | `admin.stock.view` | Stock por variante |
| PUT | `/{variantId}` | `admin.stock.manage` | Fijar `quantityOnHand` |

### Pedidos — `/admin/orders`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/?page&pageSize&status` | `admin.orders.view` | Listado admin |
| GET | `/{orderId}` | `admin.orders.view` | Detalle |
| POST | `/{orderId}/ready` | `admin.orders.manage` | `Paid` → `ReadyToDispatch` |

### Envíos — `/admin/shipments` y `/admin/drivers`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| POST | `/shipments` | `admin.shipments.manage` | Crear envío + ticket |
| GET | `/shipments/{id}/ticket.pdf` | `admin.shipments.view` | Descargar PDF |
| GET | `/drivers` | `admin.drivers.view` | Listar repartidores |
| POST | `/drivers` | `admin.drivers.manage` | Crear repartidor |
| PUT | `/drivers/{id}` | `admin.drivers.manage` | Actualizar |

---

## Códigos HTTP habituales

| Código | Cuándo |
|--------|--------|
| 200 | OK |
| 204 | Logout / delete sin body |
| 400 | Validación FluentValidation o regla de negocio |
| 401 | Sin token o credenciales inválidas |
| 403 | Sin permiso admin |
| 404 | `NotFoundException` |
| 409 | `InsufficientStockException` |
| 500 | Error no controlado |
