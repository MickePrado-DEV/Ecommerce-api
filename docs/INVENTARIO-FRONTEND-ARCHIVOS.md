# Inventario completo de archivos — `ecommerce-web`

> Generado para copiar/pegar junto con [`10-frontend-nextjs-fsd-completo.md`](./10-frontend-nextjs-fsd-completo.md).

**Total aproximado:** 180 archivos · **API:** `http://localhost:5088/api/v1`

---

## Raíz del proyecto

| Archivo | Descripción |
|---------|-------------|
| `package.json` | Dependencias y scripts |
| `package-lock.json` | Lock (generado) |
| `next.config.ts` | Imágenes remotas, rewrites opcionales |
| `tsconfig.json` | Paths `@/*` |
| `eslint.config.mjs` | ESLint + FSD boundaries |
| `postcss.config.mjs` | Tailwind v4 |
| `tailwind.config.ts` | Extiende shadcn |
| `components.json` | Alias shadcn → `shared/ui` |
| `.env.local` | URLs API y app |
| `.env.example` | Plantilla sin secretos |
| `.gitignore` | node, .next, .env.local |
| `README.md` | Cómo arrancar frontend |

---

## `src/app/` — Solo routing (thin pages)

| Ruta Next.js | Archivo | Exporta desde |
|--------------|---------|---------------|
| `/` | `(store)/page.tsx` | `@/pages/home` |
| `/search` | `(store)/search/page.tsx` | `@/pages/search` |
| `/families/[slug]` | `(store)/families/[slug]/page.tsx` | `@/pages/family-catalog` |
| `/categories/[slug]` | `(store)/categories/[slug]/page.tsx` | `@/pages/category-catalog` |
| `/subcategories/[slug]` | `(store)/subcategories/[slug]/page.tsx` | `@/pages/subcategory-catalog` |
| `/products/[slug]` | `(store)/products/[slug]/page.tsx` | `@/pages/product-detail` |
| `/cart` | `(store)/cart/page.tsx` | `@/pages/cart` |
| `/wishlist` | `(store)/wishlist/page.tsx` | `@/pages/wishlist` |
| `/checkout/shipping` | `(store)/checkout/shipping/page.tsx` | `@/pages/checkout-shipping` |
| `/checkout/payment/[orderId]` | `(store)/checkout/payment/[orderId]/page.tsx` | `@/pages/checkout-payment` |
| `/checkout/success/[orderId]` | `(store)/checkout/success/[orderId]/page.tsx` | `@/pages/checkout-success` |
| `/orders` | `(store)/orders/page.tsx` | `@/pages/orders-list` |
| `/orders/[id]` | `(store)/orders/[id]/page.tsx` | `@/pages/order-detail` |
| `/account` | `(store)/account/page.tsx` | `@/pages/account` |
| `/account/addresses` | `(store)/account/addresses/page.tsx` | `@/pages/account-addresses` |
| `/account/addresses/new` | `(store)/account/addresses/new/page.tsx` | `@/pages/account-address-form` |
| `/account/addresses/[id]/edit` | `(store)/account/addresses/[id]/edit/page.tsx` | `@/pages/account-address-form` |
| `/account/password` | `(store)/account/password/page.tsx` | `@/pages/account-password` |
| `/login` | `(auth)/login/page.tsx` | `@/pages/login` |
| `/register` | `(auth)/register/page.tsx` | `@/pages/register-customer` |
| `/register/driver` | `(auth)/register/driver/page.tsx` | `@/pages/register-driver` |
| `/admin` | `(admin)/admin/page.tsx` | redirect → dashboard |
| `/admin/dashboard` | `(admin)/admin/dashboard/page.tsx` | `@/pages/admin-dashboard` |
| `/admin/covers` | `(admin)/admin/covers/page.tsx` | `@/pages/admin-covers` |
| `/admin/covers/new` | `(admin)/admin/covers/new/page.tsx` | `@/pages/admin-cover-form` |
| `/admin/covers/[id]/edit` | `(admin)/admin/covers/[id]/edit/page.tsx` | `@/pages/admin-cover-form` |
| `/admin/families` | `(admin)/admin/families/page.tsx` | `@/pages/admin-families` |
| `/admin/families/new` | `(admin)/admin/families/new/page.tsx` | `@/pages/admin-family-form` |
| `/admin/families/[id]/edit` | `(admin)/admin/families/[id]/edit/page.tsx` | `@/pages/admin-family-form` |
| `/admin/categories` | `(admin)/admin/categories/page.tsx` | `@/pages/admin-categories` |
| `/admin/categories/new` | `(admin)/admin/categories/new/page.tsx` | `@/pages/admin-category-form` |
| `/admin/categories/[id]/edit` | `(admin)/admin/categories/[id]/edit/page.tsx` | `@/pages/admin-category-form` |
| `/admin/subcategories` | `(admin)/admin/subcategories/page.tsx` | `@/pages/admin-subcategories` |
| `/admin/subcategories/new` | `(admin)/admin/subcategories/new/page.tsx` | `@/pages/admin-subcategory-form` |
| `/admin/subcategories/[id]/edit` | `(admin)/admin/subcategories/[id]/edit/page.tsx` | `@/pages/admin-subcategory-form` |
| `/admin/products` | `(admin)/admin/products/page.tsx` | `@/pages/admin-products` |
| `/admin/products/new` | `(admin)/admin/products/new/page.tsx` | `@/pages/admin-product-form` |
| `/admin/products/[id]/edit` | `(admin)/admin/products/[id]/edit/page.tsx` | `@/pages/admin-product-form` |
| `/admin/products/[id]/variants` | `(admin)/admin/products/[id]/variants/page.tsx` | `@/pages/admin-product-variants` |
| `/admin/products/[id]/options` | `(admin)/admin/products/[id]/options/page.tsx` | `@/pages/admin-product-options` |
| `/admin/inventory` | `(admin)/admin/inventory/page.tsx` | `@/pages/admin-inventory` |
| `/admin/orders` | `(admin)/admin/orders/page.tsx` | `@/pages/admin-orders` |
| `/admin/orders/[id]` | `(admin)/admin/orders/[id]/page.tsx` | `@/pages/admin-order-detail` |
| `/admin/shipments` | `(admin)/admin/shipments/page.tsx` | `@/pages/admin-shipments` |
| `/admin/shipments/new` | `(admin)/admin/shipments/new/page.tsx` | `@/pages/admin-shipment-form` |
| `/admin/drivers` | `(admin)/admin/drivers/page.tsx` | `@/pages/admin-drivers` |
| `/admin/drivers/new` | `(admin)/admin/drivers/new/page.tsx` | `@/pages/admin-driver-form` |
| `/admin/drivers/[id]/edit` | `(admin)/admin/drivers/[id]/edit/page.tsx` | `@/pages/admin-driver-form` |
| `/driver` | `(driver)/driver/page.tsx` | redirect → shipments |
| `/driver/shipments` | `(driver)/driver/shipments/page.tsx` | `@/pages/driver-shipments` |
| `/driver/shipments/[id]` | `(driver)/driver/shipments/[id]/page.tsx` | `@/pages/driver-shipment-detail` |
| Global | `layout.tsx` | Root + Providers |
| Global | `providers.tsx` | Re-export |
| Global | `globals.css` | Tema |
| Global | `not-found.tsx` | 404 |
| Global | `forbidden.tsx` | 403 |
| Global | `middleware.ts` | Protección rutas |

---

## `src/shared/`

| Archivo |
|---------|
| `shared/api/client.ts` |
| `shared/api/index.ts` |
| `shared/config/env.ts` |
| `shared/lib/utils.ts` |
| `shared/lib/format-money.ts` |
| `shared/lib/query-keys.ts` |
| `shared/lib/order-status.ts` |
| `shared/providers/app-providers.tsx` |
| `shared/ui/*` (shadcn: button, input, card, …) |
| `shared/ui/price-tag.tsx` |
| `shared/ui/empty-state.tsx` |
| `shared/ui/loading-grid.tsx` |
| `shared/ui/page-header.tsx` |

---

## `src/entities/`

| Slice | Archivos |
|-------|----------|
| `product` | `model/types.ts`, `api/product-api.ts`, `ui/product-card.tsx`, `ui/variant-selector.tsx`, `ui/stock-badge.tsx`, `index.ts` |
| `catalog` | `model/types.ts`, `api/catalog-api.ts`, `index.ts` |
| `cart` | `model/types.ts`, `model/cart-store.ts`, `api/cart-api.ts`, `index.ts` |
| `order` | `model/types.ts`, `api/order-api.ts`, `ui/order-status-badge.tsx`, `index.ts` |
| `user` | `model/types.ts`, `api/auth-api.ts`, `model/auth-store.ts`, `index.ts` |
| `address` | `model/types.ts`, `api/address-api.ts`, `index.ts` |
| `wishlist` | `model/types.ts`, `api/wishlist-api.ts`, `index.ts` |
| `review` | `model/types.ts`, `api/review-api.ts`, `index.ts` |
| `shipment` | `model/types.ts`, `api/shipment-api.ts`, `index.ts` |
| `driver` | `model/types.ts`, `api/driver-api.ts`, `index.ts` |
| `admin` | `model/types.ts`, `api/admin-api.ts`, `index.ts` |

---

## `src/features/`

| Slice | Archivos |
|-------|----------|
| `auth/login` | `ui/login-form.tsx`, `model/use-login.ts`, `index.ts` |
| `auth/register-customer` | `ui/register-form.tsx`, `index.ts` |
| `auth/register-driver` | `ui/register-driver-form.tsx`, `index.ts` |
| `auth/logout` | `ui/logout-button.tsx`, `index.ts` |
| `auth/update-profile` | `ui/profile-form.tsx`, `index.ts` |
| `auth/change-password` | `ui/change-password-form.tsx`, `index.ts` |
| `cart/add-to-cart` | `ui/add-to-cart-button.tsx`, `model/use-add-to-cart.ts`, `index.ts` |
| `cart/update-quantity` | `ui/quantity-stepper.tsx`, `index.ts` |
| `cart/remove-from-cart` | `ui/remove-item-button.tsx`, `index.ts` |
| `cart/merge-guest` | `model/use-merge-cart.ts`, `index.ts` |
| `wishlist/toggle-wishlist` | `ui/wishlist-button.tsx`, `index.ts` |
| `review/create-review` | `ui/review-form.tsx`, `index.ts` |
| `checkout/select-address` | `ui/address-picker.tsx`, `index.ts` |
| `checkout/apply-coupon` | `ui/coupon-field.tsx`, `index.ts` |
| `checkout/confirm-order` | `model/use-confirm-order.ts`, `index.ts` |
| `checkout/pay-order` | `ui/payment-mock-form.tsx`, `model/use-pay-order.ts`, `index.ts` |
| `order/cancel-order` | `ui/cancel-order-button.tsx`, `index.ts` |
| `order/retry-payment` | `ui/retry-payment-button.tsx`, `index.ts` |
| `driver/mark-in-transit` | `ui/mark-in-transit-button.tsx`, `index.ts` |
| `driver/mark-delivered` | `ui/mark-delivered-button.tsx`, `index.ts` |
| `admin/*` | Un feature por acción CRUD (save-family, delete-product, …) |

---

## `src/widgets/`

| Widget | Archivos |
|--------|----------|
| `store-layout` | `ui/store-layout.tsx`, `index.ts` |
| `store-header` | `ui/store-header.tsx`, `index.ts` |
| `store-footer` | `ui/store-footer.tsx`, `index.ts` |
| `catalog-drawer` | `ui/catalog-drawer.tsx`, `index.ts` |
| `cover-carousel` | `ui/cover-carousel.tsx`, `index.ts` |
| `family-grid` | `ui/family-grid.tsx`, `index.ts` |
| `product-grid` | `ui/product-grid.tsx`, `ui/product-filters.tsx`, `index.ts` |
| `product-reviews` | `ui/product-reviews-list.tsx`, `index.ts` |
| `cart-summary` | `ui/cart-summary.tsx`, `index.ts` |
| `checkout-order-summary` | `ui/checkout-order-summary.tsx`, `index.ts` |
| `order-tracking-card` | `ui/order-tracking-card.tsx`, `index.ts` |
| `admin-layout` | `ui/admin-layout.tsx`, `index.ts` |
| `admin-sidebar` | `ui/admin-sidebar.tsx`, `model/nav-config.ts`, `index.ts` |
| `admin-data-table` | `ui/admin-data-table.tsx`, `index.ts` |
| `admin-stats-cards` | `ui/admin-stats-cards.tsx`, `index.ts` |
| `driver-layout` | `ui/driver-layout.tsx`, `index.ts` |
| `driver-shipment-card` | `ui/driver-shipment-card.tsx`, `index.ts` |

---

## `src/pages/` (capa FSD)

Cada slice: `ui/*-page.tsx` + `index.ts` (re-export default).

| Slice | Pantalla |
|-------|----------|
| `home` | Home tienda |
| `search` | Resultados búsqueda |
| `family-catalog` | Listado por familia |
| `category-catalog` | Listado por categoría |
| `subcategory-catalog` | Listado por subcategoría |
| `product-detail` | Ficha producto |
| `cart` | Carrito |
| `wishlist` | Favoritos |
| `checkout-shipping` | Envío + dirección + cupón |
| `checkout-payment` | Pago mock |
| `checkout-success` | Confirmación |
| `orders-list` | Mis pedidos |
| `order-detail` | Detalle + tracking |
| `account` | Perfil |
| `account-addresses` | Lista direcciones |
| `account-address-form` | Crear/editar dirección |
| `account-password` | Cambiar contraseña |
| `login` | Login |
| `register-customer` | Registro cliente |
| `register-driver` | Registro repartidor |
| `admin-dashboard` | Dashboard |
| `admin-covers` … `admin-drivers` | CRUD admin (ver rutas arriba) |
| `driver-shipments` | Lista envíos repartidor |
| `driver-shipment-detail` | Detalle envío (desde listado API) |

---

## Mapeo pantalla → endpoints API

Ver tabla completa en [`10-frontend-nextjs-fsd-completo.md`](./10-frontend-nextjs-fsd-completo.md#mapeo-pantalla--api).
