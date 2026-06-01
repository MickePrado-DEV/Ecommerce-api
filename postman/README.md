# Postman - Ecommerce API

Documentación del backend: [`../docs/`](../docs/README.md) · Endpoints: [`../docs/03-api-endpoints.md`](../docs/03-api-endpoints.md)

## Qué importar en Postman

Solo estos **2 archivos JSON** (no importes la carpeta `scripts/`):

| Archivo | Descripción |
|---------|-------------|
| `Ecommerce-API.postman_collection.json` | Colección con todos los endpoints |
| `Ecommerce-Local.postman_environment.json` | Variables de entorno local |

Regenerar colección tras cambios en la API:

```bash
node postman/scripts/sync-collection-v2.js
```

## Setup inicial

1. Ejecutar seed SQL: `scriptsSql/run-all.ps1` (usa `schema.sqlserver.sql` + `seed.sqlserver.sql`)
2. Arrancar API en `http://localhost:5088` (perfil SqlServer)
3. Importar los 2 JSON en Postman
4. Entorno **Ecommerce - Local** activo

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
| **06 - Admin Catálogo** | CRUD + listado paginado productos |
| **07 - Admin Inventario** | Listado paginado + ajuste stock |
| **08 - Admin Pedidos y Envíos** | Pedidos/envíos paginados, conductores options, PDF |
| **09 - Admin Portadas** | CRUD covers + reorder |
| **10 - Admin Opciones globales** | Opciones Talla/Color + asignaciones + variantes |
| **11 - Admin Usuarios y Roles** | Usuarios, roles, permisos |
| **12 - Admin Despacho** | Settings, cola, lotes, rutas |
| **Flujo Cliente** | Compra completa en orden |
| **Flujo Admin** | Despacho + PDF |
| **Flujo Completo E2E** | Runner end-to-end (compra → pago → envío → PDF) |

## Usuarios de prueba (seed SQL)

| Rol | Email | Password |
|-----|-------|----------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |
| Repartidor | `repartidor@ecommerce.local` | `Repartidor123!` |

## Variables seed (entorno)

| Variable | Valor |
|----------|-------|
| `productSlug` | `audifonos-pro-x` |
| `addressId` | `66666666-6666-6666-6666-666666666601` |
| `productId` | `44444444-4444-4444-4444-444444444401` |
| `driverId` | `55555555-5555-5555-5555-555555555501` |
| `couponCode` | `WELCOME10` |

## Flujo E2E recomendado (Collection Runner)

1. `scriptsSql/run-all.ps1`
2. Postman → carpeta **Flujo Completo E2E** → **Run collection**
3. Verifica: pedido pagado, envío creado, PDF descargado

Pasos manuales equivalentes: **Flujo Cliente** → **Flujo Admin** (login admin entre ambos).

## Autenticación automática

1. **Login Admin** o **Login Cliente** (carpeta 00 - Setup)
2. Tests guardan `accessToken`, `refreshToken`, `userId`, etc.
3. El resto de peticiones usa **Bearer {{accessToken}}**
4. Carrito invitado: header `X-Guest-Token: {{guestToken}}`

## Notas

- `GET /admin/drivers` devuelve respuesta **paginada**; para selects usar `/admin/drivers/options`
- Listados admin soportan `page`, `pageSize`, `search`, `sortBy`, `sortDirection`
- Si `GET /ready` falla: ejecutar `run-all.ps1` o reiniciar API
