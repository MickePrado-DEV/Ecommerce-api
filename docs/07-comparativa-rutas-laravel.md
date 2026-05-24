# Comparativa: rutas Laravel (web + admin) vs API .NET

> **Actualizado:** se amplió la API para acercarla a la spec Laravel (home/covers, slugs, direcciones, carrito merge, admin covers/shipments, etc.). Ver `03-api-endpoints.md`.

Análisis de cobertura entre el proyecto Laravel original y la API actual (`/api/v1`).

**Leyenda**

| Símbolo | Significado |
|---------|-------------|
| ✅ | Cubierto (equivalente o mejor en API) |
| ⚠️ | Parcial / distinto flujo (el front debe adaptarse) |
| ❌ | No existe en la API |
| 🖥️ | Solo UI web (no aplica como endpoint API) |

---

## Tienda (web Laravel)

| Ruta Laravel | Equivalente API .NET | Estado | Notas |
|--------------|----------------------|--------|-------|
| `GET /` welcome | — | 🖥️ | Página de inicio; el front React/Next la reemplaza. |
| `GET families/{family}` | `GET /catalog/families` | ⚠️ | API devuelve **árbol completo**, no un `show` por familia/id. |
| `GET categories/{category}` | (incluido en familias) | ⚠️ | No hay `GET /catalog/categories/{id\|slug}`. |
| `GET subCategories/{subCategory}` | (incluido en familias) | ⚠️ | No hay `GET /catalog/subcategories/{id\|slug}`. |
| `GET products/{product}` | `GET /catalog/products/{slug}` | ✅ | Por **slug**, no por id numérico. |
| — | `GET /catalog/products?page&q` | ➕ | Extra: listado paginado con búsqueda. |
| `GET cart` | `GET /api/v1/cart/` | ✅ | + invitado con `X-Guest-Token`. |
| — | `POST/PUT/DELETE /cart/items` | ➕ | API REST completa del carrito. |

### Zona autenticada (cliente)

| Ruta Laravel | Equivalente API .NET | Estado | Notas |
|--------------|----------------------|--------|-------|
| `GET shipping` | — | ⚠️ | En API la dirección va en **un solo POST** `/checkout` (no pantalla previa separada). |
| `GET checkout/payment/{order}` | `POST /checkout` + `POST /orders/{id}/pay` | ⚠️ | Laravel: pantallas Livewire. API: crear pedido → pagar mock en 2 POST. |
| `GET checkout/success/{order}` | `GET /orders/{orderId}` | ⚠️ | No hay ruta “success”; el front usa detalle del pedido tras pago. |
| `GET orders` | `GET /api/v1/orders` | ✅ | Historial. |
| `GET orders/{order}` | `GET /api/v1/orders/{orderId}` | ✅ | Detalle. |
| — | `POST /auth/register`, `/refresh`, `/logout`, `/me` | ➕ | Auth JWT (no Sanctum). |
| Middleware `verified` | — | ❌ | No hay verificación de email en API. |

---

## Admin (Laravel)

| Recurso Laravel | API .NET | Estado | Detalle |
|-----------------|----------|--------|---------|
| `GET /` dashboard | `GET /admin/dashboard` | ✅ | API devuelve JSON, no `view()`. |
| `GET /options` | — | ❌ | Entidad `ProductOption` / `OptionValue` en BD, **sin endpoints**. |
| `Route::resource families` | `/admin/catalog/families` | ⚠️ | Ver tabla abajo. |
| `Route::resource categories` | `/admin/catalog/categories` | ⚠️ | Sin list/show admin. |
| `Route::resource subCategories` | `/admin/catalog/subcategories` | ⚠️ | Sin list/show admin. |
| `Route::resource products` | `/admin/catalog/products` + variants | ⚠️ | Ver tabla abajo. |
| `Route::resource covers` | — | ❌ | Entidad `Cover` + permisos; **sin CRUD API**. |
| `Route::resource drivers` | `/admin/drivers` | ⚠️ | Sin `DELETE` ni `GET /{id}`. |
| `GET orders` | `GET /admin/orders` | ✅ | Con filtros `page`, `pageSize`, `status`. |
| `GET orders/{order}` | `GET /admin/orders/{orderId}` | ✅ | |
| `GET orders/{order}/ticket` | `GET /admin/shipments/{shipmentId}/ticket.pdf` | ⚠️ | Ticket por **envío**, no por pedido directo. |
| `PATCH orders/.../ready-to-dispatch` | `POST /admin/orders/{id}/ready` | ✅ | Método distinto (POST vs PATCH). |
| `GET shipments` (vista) | — | ❌ | No hay **listar envíos**. |
| `GET shipments/create` | — | 🖥️ | Formulario; en API solo `POST` crear. |
| `POST shipments` | `POST /admin/shipments` | ✅ | |
| `PATCH shipments/.../in-transit` | — | ❌ | Enum `InTransit` existe; **sin endpoint**. |
| `PATCH shipments/.../delivered` | — | ❌ | Enum `Delivered` existe; **sin endpoint**. |
| `GET products/{product}/variants/{variant}` | CRUD `/admin/catalog/variants` | ⚠️ | Variantes globales por id; no ruta anidada “por producto”. |

### Detalle admin — familias / categorías / subcategorías

| Acción REST (Laravel) | API actual |
|----------------------|------------|
| index (list) | Familias: ✅ `GET /families`. Categorías/subcategorías: ❌ no list admin |
| show | ❌ ninguna |
| create/store | ✅ POST |
| edit/update | ✅ PUT |
| destroy | ✅ DELETE |

### Detalle admin — productos y variantes

| Acción | API actual |
|--------|------------|
| products index | ✅ `GET /admin/catalog/products` |
| products show | ❌ (solo listado; detalle tienda es por slug público) |
| products CRUD | ✅ POST/PUT/DELETE |
| variants por producto | ✅ POST/PUT/DELETE `/variants` (con `productId` en body) |

---

## Resumen: qué falta implementar (prioridad sugerida)

### Alta (paridad funcional tienda + admin operativo)

1. **Covers CRUD** — `admin.covers.view` / `admin.covers.manage` ya definidos.  
2. **Product options CRUD** — `admin.options.view` / `admin.options.manage`; entidades listas.  
3. **Listar envíos (admin)** — `GET /admin/shipments`.  
4. **Cambiar estado de envío** — `PATCH` o `POST` in-transit / delivered.  
5. **Catálogo público por slug/id** — opcional si el front no usa solo el árbol:
   - `GET /catalog/families/{slug}`
   - `GET /catalog/categories/{slug}`
   - `GET /catalog/subcategories/{slug}`

### Media (comodidad admin / front)

6. **Admin GET list** categorías y subcategorías (y opcionalmente show por id).  
7. **Admin GET producto por id** — `GET /admin/catalog/products/{id}`.  
8. **Ticket PDF por pedido** — `GET /admin/orders/{orderId}/ticket.pdf` (hoy solo por `shipmentId`).  
9. **DELETE conductor** — si el admin Laravel lo usaba.  
10. **Listar variantes de un producto** — `GET /admin/catalog/products/{productId}/variants`.

### Baja / solo web

11. **Email verified** — flujo de registro/verificación.  
12. **GET shipping** — pantalla intermedia; el front puede usar formulario local + `POST /checkout`.  
13. **Welcome / vistas Blade** — no aplican a API.

---

## Lo que la API ya tiene y Laravel no (extras útiles)

- `GET /health` — monitoreo.  
- `POST /auth/refresh` — renovar JWT.  
- Carrito invitado (`X-Guest-Token`).  
- OpenAPI / Scalar (`/scalar/v1`).  
- Transacciones checkout + reserva de stock.  
- Permisos JWT granulares en claims (equivalente a `permission:` de Laravel).  
- Inventario admin dedicado (`/admin/inventory`).

---

## Mapeo mental Laravel → React + esta API

| Pantalla Laravel | Cómo armarla con la API actual |
|------------------|--------------------------------|
| Home / welcome | Front estático + `GET /catalog/families` o `/products` |
| Familia / categoría / subcategoría | Navegar con datos del árbol o añadir endpoints `show` |
| Ficha producto | `GET /catalog/products/{slug}` |
| Carrito | `GET/POST/PUT/DELETE /cart` |
| Envío | Formulario → `POST /checkout` (incluye dirección + `shippingCost`) |
| Pago | `POST /orders/{id}/pay` |
| Éxito | `GET /orders/{id}` con status `Paid` |
| Admin CRUD | Endpoints bajo `/admin/...` con Bearer token admin |

---

## Siguiente paso recomendado

Decidir si el frontend necesita **árbol completo** (`/families`) o **shows individuales** por slug. Para admin, el mayor hueco es **covers**, **options** y **gestión de estados de envío**.

Cuando priorices, se pueden implementar por bloques (ver checklist en issues o tareas).
