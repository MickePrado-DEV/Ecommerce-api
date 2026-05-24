# Comparativa: rutas Laravel (web + admin) vs API .NET

> **Estado actual:** la API .NET cubre la mayoría de flujos de tienda y admin descritos en la spec Laravel. Detalle de rutas: [`03-api-endpoints.md`](03-api-endpoints.md).

**Leyenda**

| Símbolo | Significado |
|---------|-------------|
| ✅ | Cubierto |
| ⚠️ | Parcial / distinto modelo o ruta |
| ❌ | No implementado |
| 🖥️ | Solo UI Laravel (no aplica API) |

---

## Resumen rápido

| Área | Estado |
|------|--------|
| Home + covers + latest products | ✅ |
| Navegación por slug (familia/categoría/subcategoría) | ✅ |
| Filtros productos (sin `featureIds[]`) | ⚠️ |
| Carrito CRUD + guest + merge | ✅ |
| Direcciones CRUD + default | ✅ |
| Checkout + pay + retry | ✅ |
| Admin covers + reorder | ✅ |
| Admin dashboard stats | ✅ |
| Admin envíos list + in-transit + delivered | ✅ |
| Admin opciones (por producto, no global) | ⚠️ |
| Variantes generate / attach Laravel | ❌ |
| Verificación email Jetstream | ❌ |
| Pagos Niubiz reales | ❌ |

---

## Tienda (web Laravel)

| Ruta Laravel | Equivalente API .NET | Estado |
|--------------|----------------------|--------|
| `GET /` welcome | `GET /catalog/home` | ✅ |
| `GET families/{family}` | `GET /catalog/families/{slug}` | ✅ |
| `GET categories/{category}` | `GET /catalog/categories/{slug}` | ✅ |
| `GET subCategories/{subCategory}` | `GET /catalog/subcategories/{slug}` | ✅ |
| `GET products/{product}` | `GET /catalog/products/{slug}` | ✅ |
| Filter Livewire | `GET /catalog/products?filters` | ⚠️ Sin `featureIds[]` |
| `GET cart` | `GET /cart` | ✅ |
| Add/update/remove cart | `POST/PATCH/DELETE /cart/items` | ✅ |
| Merge al login | `POST /cart/merge` | ✅ |
| AddressList / AddressForm | `/addresses` CRUD | ✅ |
| CartSummary.confirmOrder | `POST /checkout` | ✅ |
| PaymentCheckout | `POST /orders/{id}/pay` o `/checkout/{id}/pay` | ✅ |
| OrderHistory | `GET /orders` | ✅ |
| Middleware `verified` | — | ❌ |

---

## Admin (Laravel)

| Recurso | API .NET | Estado |
|---------|----------|--------|
| Dashboard | `GET /admin/dashboard/stats` | ✅ |
| Covers + sort | `/admin/covers` + `PATCH /reorder` | ✅ |
| families/categories/subCategories | `/admin/catalog/...` + aliases `/admin/families` | ⚠️ Sin list admin categorías |
| products + variants | `/admin/catalog/products`, `/variants` | ⚠️ Sin generate variants Laravel |
| options global Livewire | `/admin/products/{id}/options` | ⚠️ Modelo distinto |
| drivers | `/admin/drivers` CRUD | ✅ |
| orders + ticket | `/admin/orders` + `/ticket` | ✅ |
| shipments | list + create + in-transit + delivered | ✅ |
| stock movements list | — | ❌ |

---

## Rutas legacy vs aliases

La colección Postman y el front pueden usar:

- **Canónico:** `/admin/catalog/families`, `/admin/catalog/products`, …
- **Alias spec:** `/admin/families`, `/admin/products` (parcial)

---

## Postman y documentación

- Colección: `postman/Ecommerce-API.postman_collection.json`
- Regenerar endpoints en colección: `node postman/scripts/sync-collection-v2.js`
