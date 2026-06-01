# Guía para principiantes — Frontend (Next.js + FSD)

Tutorial completo del frontend **[ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web)**: de cero a entender cada carpeta y cada archivo.

**API requerida:** [Ecommerce-api](https://github.com/MickePrado-DEV/Ecommerce-api) en `http://localhost:5088/api/v1`

**Guía del backend:** [00-guia-para-principiantes.md](https://github.com/MickePrado-DEV/Ecommerce-api/blob/master/docs/00-guia-para-principiantes.md)

---

## Mapa del tutorial

| Parte | Contenido |
|-------|-----------|
| [I — Práctica](#parte-i-práctica-de-cero-a-que-funcione) | Clonar, configurar, arrancar, probar roles |
| [II — Decisiones](#parte-ii-por-qué-está-así-el-frontend) | Next.js, FSD, repo separado, React Query |
| [III — Inventario `src/`](#parte-iii-inventario-carpeta-por-carpeta) | Cada capa y archivos clave |
| [IV — Código a código](#parte-iv-recorrido-código-a-código) | Login, carrito, admin, PDF |
| [V — Mapa de rutas](#parte-v-mapa-de-rutas-app) | Todas las URLs de la app |
| [VI — Referencia](#parte-vi-referencia-y-ejercicios) | Glosario, ejercicios, problemas |

---

# Parte I — Práctica: de cero a que funcione

## Paso 0: Qué es este frontend

Aplicación **Next.js 16** que consume la API .NET. Tres experiencias en una sola web:

| Área | Rutas | Rol |
|------|-------|-----|
| **Tienda** | `/`, `/catalog/...`, `/cart`, `/checkout` | Cliente o invitado |
| **Admin** | `/admin/...` | Admin con permisos JWT |
| **Repartidor** | `/driver/...` | Repartidor |

```
Navegador (:3000)
    │ fetch + JWT / guest token
    ▼
API (:5088/api/v1)
    ▼
SQL Server
```

## Paso 1: Requisitos

| Herramienta | Versión |
|-------------|---------|
| Node.js | 20+ |
| API en marcha | Ver guía backend |
| Git | Reciente |

## Paso 2: Clonar y configurar

```powershell
git clone https://github.com/MickePrado-DEV/ecommerce-web.git
cd ecommerce-web
copy .env.example .env.local
npm install
```

`.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5088/api/v1
NEXT_PUBLIC_APP_URL=http://localhost:3000
```

## Paso 3: Arrancar (con API ya corriendo)

```powershell
npm run dev
```

Abre [http://localhost:3000](http://localhost:3000)

Si Windows da `EPERM` en `.next`:

```powershell
npm run dev:clean
```

**Checkpoint:** home carga productos sin errores de red en consola del navegador.

## Paso 4: Probar los tres roles

| Rol | Login | Qué probar |
|-----|-------|------------|
| Cliente | `cliente@ecommerce.local` / `Cliente123!` | Catálogo → carrito → checkout |
| Admin | `admin@ecommerce.local` / `Admin123!` | `/admin/products`, inventario, envíos |
| Repartidor | `repartidor@ecommerce.local` / `Repartidor123!` | `/driver` envíos asignados |

Cupón demo: `WELCOME10` · Producto: slug `audifonos-pro-x`

## Paso 5: Comprobar tipos (opcional)

```powershell
npx tsc --noEmit
```

---

# Parte II — Por qué está así el frontend

## Repo separado del API

| Motivo | Explicación |
|--------|-------------|
| **Stack distinto** | TypeScript/React vs C#/.NET — builds y despliegues independientes |
| **Un API, N clientes** | La misma API puede servir web, mobile futuro, Postman |
| **Historial Git claro** | Issues, PRs y releases por proyecto |
| **Portafolio** | Dos repos demuestran full-stack real |

El frontend **nunca** toca la base de datos. Solo HTTP.

## Por qué Next.js 16 (App Router)

| Ventaja | Uso en este proyecto |
|---------|----------------------|
| **App Router** | Carpetas `app/` = rutas; layouts anidados por zona (store, admin, driver) |
| **Server + Client Components** | Páginas ligeras; interactividad en `'use client'` |
| **`proxy.ts`** | Protege rutas antes de renderizar (equivalente a middleware) |
| **Optimización** | Imágenes, fuentes, bundling automático |

## Por qué Feature-Sliced Design (FSD)

FSD organiza el código por **capas de responsabilidad**, no por tipo de archivo suelto:

```
app/       → routing (Next.js)
views/     → pantallas completas
widgets/   → bloques UI compuestos
features/  → acciones del usuario (login, ajustar stock)
entities/  → dominio + llamadas API
shared/    → utilidades transversales
```

**Regla de importación:** una capa solo importa de capas **inferiores**.

```
shared ← entities ← features ← widgets ← views ← app
```

Beneficio: sabes dónde poner código nuevo y evitas imports circulares.

## Por qué React Query (TanStack Query)

Los listados admin usan `useQuery` para:

- Cachear respuestas paginadas
- `keepPreviousData` al cambiar página (sin parpadeo)
- Reintentos y estados loading/error consistentes

El hook `useAdminTableQuery` centraliza page, sort, search en la **URL** (`?page=1&search=...`).

## Por qué cookie + sessionStorage para auth

| Dato | Dónde | Por qué |
|------|-------|---------|
| `accessToken` | cookie `accessToken` + `sessionStorage` | Cookie la lee `proxy.ts`; sessionStorage la usa `client.ts` en fetch |
| `refreshToken` | sessionStorage | Renovar JWT sin relogin |
| `guestToken` | localStorage | Carrito invitado persistente |

---

# Parte III — Inventario carpeta por carpeta

Árbol simplificado de `src/`:

```
src/
├── app/              → Rutas Next.js (thin re-exports)
├── views/            → Páginas FSD (UI completa)
├── widgets/          → Bloques compuestos
├── features/         → Acciones de usuario
├── entities/         → API + tipos de dominio
├── domain/           → Config de tablas admin (columnas, acciones)
├── shared/           → Client HTTP, UI base, utilidades
└── proxy.ts          → Guard de rutas protegidas
```

---

## 3.1 `shared/` — base transversal

| Archivo / carpeta | Qué hace |
|-------------------|----------|
| `shared/config/env.ts` | Lee `NEXT_PUBLIC_API_URL` y `NEXT_PUBLIC_APP_URL` |
| `shared/api/client.ts` | `api()`, `downloadAuthenticatedFile()`, JWT, refresh, guest header |
| `shared/api/parse-api-error.ts` | Mensajes de error del API |
| `shared/api/refresh-session.ts` | Refresh token + limpiar sesión |
| `shared/lib/query-keys.ts` | Claves React Query centralizadas |
| `shared/lib/admin-table-url.ts` | Parse/serialize `?page&search&sortKey` en URL |
| `shared/lib/paged-list.ts` | Helpers respuestas paginadas |
| `shared/types/admin-table-config.ts` | Tipo `AdminTableConfig<T>` (columnas, acciones, paginación) |
| `shared/ui/` | Button, Input, Card, PageHeader… (design system mínimo) |

### `client.ts` — corazón HTTP

```typescript
export async function api<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json', ... };
  const token = getAccessToken();
  if (token) headers.Authorization = `Bearer ${token}`;
  if (options.guest) {
    const guest = getGuestToken();
    if (guest) headers['X-Guest-Token'] = guest;
  }
  const res = await fetch(`${env.apiUrl}${path}`, { ...options, headers });
  // 401 → intenta refresh → reintenta una vez
  return parseResponse<T>(res);
}
```

Opciones:

| Flag | Efecto |
|------|--------|
| `auth: true` (default en rutas protegidas) | Añade Bearer |
| `guest: true` | Añade `X-Guest-Token` |
| `_retriedAfterRefresh` | Evita bucle infinito en refresh |

---

## 3.2 `entities/` — dominio + API

Cada entidad agrupa **tipos** y **funciones fetch**:

| Carpeta | Archivo API | Endpoints que llama |
|---------|-------------|---------------------|
| `entities/user/` | `api/auth-api.ts` | `/auth/login`, `/auth/me`, register… |
| `entities/user/` | `model/auth-store.ts` | Zustand: user, permissions, tokens |
| `entities/catalog/` | `api/catalog-api.ts` | `/catalog/*` |
| `entities/cart/` | `api/cart-api.ts` | `/cart/*`, merge |
| `entities/order/` | `api/order-api.ts` | `/orders/*`, checkout |
| `entities/admin/` | `api/admin-api.ts` | `/admin/*` (todo el panel) |
| `entities/admin/` | `model/types.ts` | DTOs admin (ProductAdminDto, etc.) |
| `entities/admin/` | `lib/rbac-labels.ts` | Etiquetas legibles de permisos |

`admin-api.ts` es el archivo más grande: un método por ruta admin (products, inventory, shipments, dispatch, users, roles…).

---

## 3.3 `domain/` — configuración de tablas admin

No confundir con `entities/`: aquí viven **configs declarativas** de columnas y acciones, sin JSX pesado.

| Archivo | Pantalla |
|---------|----------|
| `domain/products/product.table.ts` | `/admin/products` |
| `domain/inventory/inventory.table.ts` | `/admin/inventory` |
| `domain/orders/order.table.ts` | `/admin/orders` |
| `domain/shipments/shipment.table.ts` | `/admin/shipments` |
| `domain/drivers/driver.table.ts` | `/admin/drivers` |
| `domain/users/user.table.ts` | `/admin/users` |
| `domain/dispatch/dispatch-queue.table.ts` | `/admin/dispatch/queue` |

Ejemplo (`product.table.ts`):

```typescript
export const PRODUCT_TABLE_CONFIG: AdminTableConfig<ProductAdminDto> = {
  rowKeyField: 'id',
  defaultPageSize: 20,
  columns: [
    { key: 'name', label: 'Nombre', sortable: true },
    { key: 'slug', label: 'Slug', sortable: true },
    // ...
  ],
  rowActions: [
    { label: 'Editar', href: (row) => `/admin/products/${row.id}/edit`, icon: Pencil },
    { label: 'Variantes', href: (row) => `/admin/products/${row.id}/variants`, icon: Layers },
    { label: 'Opciones', href: (row) => `/admin/products/${row.id}/options`, icon: SlidersHorizontal },
  ],
};
```

---

## 3.4 `features/` — acciones del usuario

| Carpeta | Qué hace |
|---------|----------|
| `features/auth/login/` | `LoginForm` — POST login, guarda sesión, merge carrito |
| `features/auth/register/` | Registro cliente |
| `features/admin/table/` | `useAdminTableQuery` — paginación en URL + React Query |
| `features/catalog/add-to-cart/` | Añadir producto al carrito |
| `features/checkout/` | Flujo checkout y pago |
| `features/driver/mark-in-transit/` | Botón repartidor |
| `features/driver/mark-delivered/` | Marcar entregado |

---

## 3.5 `widgets/` — bloques UI compuestos

| Widget | Uso |
|--------|-----|
| `widgets/admin/data-table/` | `AdminDataTable`, paginación sticky, filtros |
| `widgets/admin/layout/` | Shell admin: sidebar + header |
| `widgets/admin/sidebar/` | Navegación + permisos (`nav-config.ts`) |
| `widgets/admin/route-guard/` | Oculta rutas sin permiso |
| `widgets/catalog/product-listing/` | Grid catálogo con filtros |
| `widgets/catalog/filters-panel/` | Panel lateral filtros |
| `widgets/store-header/` | Header tienda |
| `widgets/cart-drawer/` | Drawer carrito |
| `widgets/driver-shipment-card/` | Tarjeta envío repartidor |

### `nav-config.ts` — menú admin y RBAC

Cada link tiene un `permission` que coincide con el API:

```typescript
{ href: '/admin/products', label: 'Productos', permission: 'admin.products.view', icon: Package }
```

`canAccessAdminPanel(permissions)` → true si algún permiso empieza con `admin.`.

---

## 3.6 `views/` — pantallas completas

Convención: `views/{area}/{nombre}/ui/{nombre}-page.tsx`

| Carpeta | Pantalla |
|---------|----------|
| `views/catalog/` | Home, familia, categoría, producto |
| `views/cart/` | Carrito |
| `views/checkout/` | Checkout multi-paso |
| `views/account/` | Cuenta, direcciones, pedidos |
| `views/admin/products/` | Listado productos admin |
| `views/admin/inventory/` | Inventario con ajuste inline |
| `views/admin/shipments/` | Envíos + descarga PDF |
| `views/admin/dispatch/` | Cola y lotes despacho |
| `views/admin/roles/` | Roles y permisos |
| `views/driver/` | Panel repartidor |

Patrón típico admin:

```typescript
export function AdminProductsPage() {
  const table = useAdminTableQuery({
    queryKey: queryKeys.adminProductsTable,
    fetchPage: adminApi.listProductsPaged,
    tableConfig: PRODUCT_TABLE_CONFIG,
  });
  return (
    <>
      <PageHeader title="Productos" action={...} />
      <AdminDataTable {...table} tableConfig={PRODUCT_TABLE_CONFIG} />
    </>
  );
}
```

---

## 3.7 `app/` — routing Next.js

Solo **re-exporta** views. Ejemplo:

```typescript
// app/(admin)/admin/products/page.tsx
export { AdminProductsPage as default } from '@/views/admin/products/ui/admin-products-page';
```

Grupos de rutas (route groups):

| Grupo | Layout | Zona |
|-------|--------|------|
| `(store)` | Header tienda, footer | Público + cliente |
| `(admin)` | Sidebar admin | Panel administración |
| `(driver)` | Layout repartidor | App repartidor |
| `(auth)` | Sin chrome | Login, register |

---

## 3.8 `proxy.ts` — protección de rutas

```typescript
const needsAuth =
  path.startsWith('/checkout') ||
  path.startsWith('/account') ||
  path.startsWith('/admin') ||
  path.startsWith('/driver');

if (needsAuth && !token) {
  return NextResponse.redirect(`/login?redirect=${path}`);
}
```

Lee cookie `accessToken` (la setea el auth store al login).

---

# Parte IV — Recorrido código a código

## 4.1 Login completo

```
Usuario envía formulario
  → LoginForm (features/auth/login)
  → authApi.login({ email, password })
  → client.ts POST /auth/login
  → API LoginCommandHandler
  → setSession(user, permissions, accessToken, refreshToken)
  → cookie accessToken + sessionStorage
  → cartApi.merge(guestToken) si había carrito invitado
  → redirect: admin → /admin/dashboard, driver → /driver, cliente → /
```

**Auth store** (`entities/user/model/auth-store.ts`):

- Guarda user, permissions, tokens
- Sincroniza cookie para `proxy.ts`
- `clearSession()` en logout o refresh fallido

## 4.2 Carrito invitado → usuario

```
1. Primera visita: client genera guestToken en localStorage
2. GET/POST /cart con header X-Guest-Token
3. Login exitoso → cartApi.merge(guestToken)
4. API fusiona ítems al carrito del userId
```

## 4.3 Checkout tienda

```
/catalog/products/{slug} → add-to-cart feature
/cart → revisar ítems
/checkout/shipping → elegir dirección
/checkout/payment/{orderId} → POST checkout + POST pay
/checkout/success/{orderId} → confirmación
```

Cada paso usa `entities/order/api` y `entities/cart/api`.

## 4.4 Listado admin paginado (flujo completo)

```
URL: /admin/products?page=2&search=audifonos&sortKey=name&sortDir=asc

1. useAdminTableQuery parsea searchParams
2. queryKey(['admin','products', params]) → React Query
3. fetchPage → adminApi.listProductsPaged(params)
4. GET /admin/catalog/products?page=2&search=...
5. API ListProductsAdminHandler → PagedResult JSON
6. AdminDataTable renderiza columnas de PRODUCT_TABLE_CONFIG
7. onPageChange → router.replace con nuevos query params (sin scroll)
```

## 4.5 Inventario — ajuste inline

```
inventory-adjust-cell.tsx
  → input cantidad local
  → botón ✓ → adminApi.setInventory(variantId, quantity)
  → PUT /admin/inventory/{variantId}
  → invalidate query → tabla se refresca
```

## 4.6 PDF de envío

```typescript
// NO uses <a href="...ticket.pdf">
await downloadAuthenticatedFile(
  `/admin/shipments/${id}/ticket.pdf`,
  `ticket-${id}.pdf`
);
```

`downloadAuthenticatedFile` hace fetch con Bearer, convierte a blob y dispara descarga.

## 4.7 Permisos admin en UI

```
Login → permissions[] en JWT
  → auth-store guarda permissions
  → AdminRouteGuard filtra sidebar (nav-config)
  → Si entras a ruta sin permiso → /forbidden
```

Los permisos string coinciden 1:1 con `AdminPermissions` del API (`admin.products.view`, etc.).

---

# Parte V — Mapa de rutas `app/`

## Tienda `(store)`

| Ruta | View |
|------|------|
| `/` | Home catálogo |
| `/catalog` | Índice catálogo |
| `/catalog/[familySlug]` | Familia |
| `/catalog/.../[categorySlug]` | Categoría |
| `/catalog/.../[subCategorySlug]` | Subcategoría + productos |
| `/cart` | Carrito |
| `/checkout/shipping` | Dirección envío |
| `/checkout/payment/[orderId]` | Pago |
| `/checkout/success/[orderId]` | Éxito |
| `/account` | Mi cuenta |
| `/account/addresses` | Direcciones |
| `/orders/[id]` | Detalle pedido cliente |
| `/wishlist` | Lista deseos |

## Auth `(auth)`

| Ruta | View |
|------|------|
| `/login` | LoginForm |
| `/register` | Registro cliente |
| `/register/driver` | Registro repartidor |

## Admin `(admin)`

| Ruta | View |
|------|------|
| `/admin/dashboard` | Stats |
| `/admin/covers` | Portadas CRUD |
| `/admin/families` | Familias |
| `/admin/categories` | Categorías |
| `/admin/subcategories` | Subcategorías |
| `/admin/products` | Productos paginados |
| `/admin/products/[id]/edit` | Editar producto |
| `/admin/products/[id]/variants` | Variantes |
| `/admin/products/[id]/options` | Opciones |
| `/admin/inventory` | Stock |
| `/admin/orders` | Pedidos |
| `/admin/orders/[id]` | Detalle + acciones |
| `/admin/shipments` | Envíos |
| `/admin/drivers` | Conductores |
| `/admin/dispatch/queue` | Cola despacho |
| `/admin/dispatch/batches` | Lotes |
| `/admin/dispatch/routes` | Rutas entrega |
| `/admin/users` | Usuarios |
| `/admin/roles` | Roles |
| `/admin/roles/[id]/permissions` | Permisos por rol |
| `/admin/options` | Opciones globales |

## Repartidor `(driver)`

| Ruta | View |
|------|------|
| `/driver` | Listado envíos |
| `/driver/shipments/[id]` | Detalle envío |
| `/driver/change-password` | Cambio obligatorio |

---

# Parte VI — Referencia y ejercicios

## Scripts npm

| Comando | Descripción |
|---------|-------------|
| `npm run dev` | Desarrollo |
| `npm run dev:clean` | Borra `.next` y arranca |
| `npm run build` | Build producción |
| `npm run generate:barrels` | Regenera `index.ts` FSD |

## Orden de lectura recomendado

| # | Archivo |
|---|---------|
| 1 | `shared/api/client.ts` |
| 2 | `entities/user/api/auth-api.ts` |
| 3 | `features/auth/login/ui/login-form.tsx` |
| 4 | `proxy.ts` |
| 5 | `features/admin/table/model/use-admin-table-query.ts` |
| 6 | `widgets/admin/data-table/ui/admin-data-table.tsx` |
| 7 | `views/admin/products/ui/admin-products-page.tsx` |
| 8 | `entities/admin/api/admin-api.ts` |
| 9 | `domain/products/product.table.ts` |
| 10 | `widgets/admin/sidebar/model/nav-config.ts` |

## Ejercicios finales

| # | Ejercicio | Aprendes |
|---|-----------|----------|
| 1 | Login cliente + añadir al carrito sin login previo | Guest token |
| 2 | Breakpoint en `LoginForm` onSubmit | Flujo auth |
| 3 | Cambiar columna en `product.table.ts` | Config declarativa |
| 4 | Paginar productos admin y copiar URL con query params | URL state |
| 5 | Descargar PDF envío desde admin | Auth en binarios |
| 6 | Quitar permiso en API y ver link oculto en sidebar | RBAC UI |
| 7 | Trazar ruta `/checkout/payment/[orderId]` archivo por archivo | App Router + FSD |

## Problemas frecuentes

| Síntoma | Solución |
|---------|----------|
| Network error | API en :5088, `NEXT_PUBLIC_API_URL` correcto |
| Redirect infinito login | Cookie `accessToken` + sessionStorage sincronizados |
| Admin vacío / 403 | Usuario admin con permisos en BD (seed) |
| PDF no descarga | Usar `downloadAuthenticatedFile`, no enlace directo |
| EPERM `.next` | `npm run dev:clean` |
| Tabla admin sin datos | Seed SQL ejecutado en API |

## Documentación relacionada

| Documento | Enlace |
|-----------|--------|
| Guía backend | [00-guia-para-principiantes.md](https://github.com/MickePrado-DEV/Ecommerce-api/blob/master/docs/00-guia-para-principiantes.md) |
| Guía FSD detallada | [10-frontend-nextjs-fsd-completo.md](https://github.com/MickePrado-DEV/Ecommerce-api/blob/master/docs/10-frontend-nextjs-fsd-completo.md) |
| Inventario archivos | [INVENTARIO-FRONTEND-ARCHIVOS.md](https://github.com/MickePrado-DEV/Ecommerce-api/blob/master/docs/INVENTARIO-FRONTEND-ARCHIVOS.md) |
| Endpoints API | [03-api-endpoints.md](https://github.com/MickePrado-DEV/Ecommerce-api/blob/master/docs/03-api-endpoints.md) |

---

Proyecto personal / portafolio. **MickePrado-DEV** — Miguel Angel Prado Garcia.
