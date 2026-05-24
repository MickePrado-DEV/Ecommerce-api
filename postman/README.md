# Postman - Ecommerce API

Documentación del backend: [`../docs/`](../docs/README.md) · Endpoints: [`../docs/03-api-endpoints.md`](../docs/03-api-endpoints.md)

## Qué importar en Postman

Solo estos **2 archivos JSON** (no importes la carpeta `scripts/`):

| Archivo | Descripción |
|---------|-------------|
| `Ecommerce-API.postman_collection.json` | Colección con todos los endpoints |
| `Ecommerce-Local.postman_environment.json` | Variables de entorno local |

Los scripts de login, sesión y variables van **dentro** del JSON de la colección (pestañas **Tests** y **Pre-request**).

### Carpeta `scripts/` (solo para el repo)

| Archivo | Para qué sirve |
|---------|----------------|
| `save-auth-session.js` | Referencia del código de sesión (embebido en la colección) |
| `enrich-collection-descriptions.js` | Regenerar descripciones en el JSON |
| `sync-collection-v2.js` | Añadir endpoints nuevos a la colección (`node postman/scripts/sync-collection-v2.js`) |

## Estructura de la colección

| Carpeta | Contenido |
|---------|-----------|
| **00 - Setup** | Health, Ready (BD), Login Admin/Cliente |
| **01 - Auth** | Register, refresh, me, logout |
| **02 - Catálogo** | Home, covers, slugs, búsqueda, filtros |
| **03 - Carrito** | Guest (GET/POST/PATCH/DELETE), merge con JWT |
| **03b - Direcciones** | CRUD + default (cliente autenticado) |
| **04 - Checkout y Pedidos** | Checkout inline o `addressId`, pay, retry |
| **05 - Admin General** | Dashboard + stats |
| **06 - Admin Catálogo** | CRUD familias, categorías, productos, variantes |
| **07 - Admin Inventario** | Listar y ajustar stock |
| **08 - Admin Pedidos y Envíos** | Pedidos, PDF, envíos in-transit/delivered, conductores |
| **09 - Admin Portadas** | CRUD covers + reorder |
| **10 - Admin Opciones** | Opciones y valores por producto |
| **Flujo Cliente** | Compra completa en orden |
| **Flujo Admin** | Despacho + PDF |

## Importar en Postman

1. Postman → **Import** → los dos `.json`
2. Entorno **Ecommerce - Local** (esquina superior derecha)

## Antes de probar

1. API en `http://localhost:5088` (perfil SqlServer)
2. `GET {{baseUrl}}/health` → `{ "status": "ok" }`
3. `GET {{baseUrl}}/ready` → `{ "status": "ready" }` (comprueba BD)
4. Si hay error 500: reinicia la API (puede recrear esquema si la BD LocalDB estaba desactualizada)

## Usuarios de prueba (seed)

| Rol | Email | Password |
|-----|-------|----------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |

| Variable | Valor seed |
|----------|------------|
| `productSlug` | `audifonos-pro-x` |
| `familySlug` | `electronica` |
| `categorySlug` | `audio` |
| `subcategorySlug` | `audifonos` |

## Autenticación automática

1. **Login Admin** o **Login Cliente** (carpeta 00 - Setup)
2. Tests guardan `accessToken`, `refreshToken`, `userId`, etc.
3. El resto de peticiones usa **Bearer {{accessToken}}** (colección + pre-request)
4. Carrito invitado: header `X-Guest-Token: {{guestToken}}` (se guarda al agregar ítem)
5. **Merge carrito:** tras login cliente, `POST /cart/merge` con `guestToken` previo

## Flujos recomendados

### Cliente (compra con dirección guardada)

1. `00 - Setup` → Login Cliente
2. `02 - Catálogo` → Detalle producto (guarda `variantId`)
3. `03 - Carrito` → Agregar ítem
4. `03b - Direcciones` → Crear dirección (guarda `addressId`)
5. `04 - Checkout` → **Checkout (con addressId)** → Pago mock

### Admin (despacho)

1. Login Admin
2. Pedido pagado del flujo cliente → `Marcar listo para despacho`
3. `Crear envío` → `Descargar ticket PDF` o `Ticket PDF por pedido`

### Invitado + merge

1. Sin login: Guest - Agregar item (guarda `guestToken`)
2. Login Cliente
3. Usuario - Merge carrito guest

## Variables automáticas

| Variable | Se guarda en |
|----------|----------------|
| `accessToken`, `refreshToken` | Login / Register / Refresh |
| `variantId` | Detalle producto / agregar carrito |
| `cartItemId` | Carrito guest |
| `guestToken` | Carrito invitado |
| `addressId` | Direcciones |
| `orderId` | Checkout |
| `familyId`, `categoryId`, `productId` | Admin catálogo |
| `shipmentId`, `driverId` | Admin envíos |
| `coverId`, `optionId` | Admin covers / opciones |

## Cambiar URL

En el entorno, edita `baseUrl` si la API corre en otro puerto.
