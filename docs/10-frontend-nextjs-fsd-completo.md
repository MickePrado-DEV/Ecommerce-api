# Frontend paso a paso — Next.js 15 + TypeScript (completo)

> **API .NET:** `http://localhost:5088/api/v1` · Scalar: `http://localhost:5088/scalar/v1`  
> **Inventario de archivos:** [`INVENTARIO-FRONTEND-ARCHIVOS.md`](./INVENTARIO-FRONTEND-ARCHIVOS.md)  
> **Endpoints backend:** [`03-api-endpoints.md`](./03-api-endpoints.md)  
> **Backend:** [`../README.md`](../README.md)

Guía **copy-paste** para el cliente web del ecommerce: **tienda**, **admin** y **app repartidor**, con **Feature-Sliced Design (FSD)**.

---

## Tabla de contenidos

1. [Prerrequisitos y crear proyecto](#1-prerrequisitos-y-crear-proyecto)
2. [Estructura FSD y árbol de carpetas](#2-estructura-fsd)
3. [Configuración (copiar tal cual)](#3-configuración-copiar-tal-cual)
4. [Tipos TypeScript (DTOs del API)](#4-tipos-typescript)
5. [Cliente API y módulos por dominio](#5-cliente-api)
6. [Autenticación y middleware](#6-autenticación)
7. [Tienda — pantallas y componentes](#7-tienda)
8. [Cuenta, pedidos y wishlist](#8-cuenta-pedidos-wishlist)
9. [Panel admin — todas las pantallas](#9-panel-admin)
10. [App repartidor](#10-app-repartidor)
11. [Widgets y design system](#11-widgets)
12. [Estado, React Query y checklist](#12-estado-y-checklist)

---

## 1. Prerrequisitos y crear proyecto

| Herramienta | Versión |
|-------------|---------|
| Node.js | 20 LTS o 22 |
| API .NET | Corriendo en **:5088** |
| Git | 2.x |

```powershell
# API (otra terminal)
cd C:\Udemy\.net\Ecommerce\ecommerce-api\src\Ecommerce.Api
dotnet run --launch-profile SqlServer

# Frontend
cd C:\Udemy\.net\Ecommerce
npx create-next-app@latest ecommerce-web --typescript --tailwind --eslint --app --src-dir --import-alias "@/*"
cd ecommerce-web
```

```powershell
npm install @tanstack/react-query @tanstack/react-table
npm install react-hook-form @hookform/resolvers zod
npm install zustand sonner lucide-react swiper next-themes
npm install @dnd-kit/core @dnd-kit/sortable

npx shadcn@latest init
# Style: New York · Base: Zinc · CSS variables: Yes

npx shadcn@latest add button input label card badge dialog dropdown-menu sheet tabs table select textarea sonner skeleton separator avatar checkbox form alert alert-dialog scroll-area breadcrumb pagination
```

---

## 2. Estructura FSD

```
shared → entities → features → widgets → pages → app (Next.js solo enruta)
```

Reglas:

- Solo importar **hacia abajo**.
- Cada slice exporta **Public API** en `index.ts`.
- `src/app/**/page.tsx` = **thin page** (1 línea re-export).

Árbol completo: [`INVENTARIO-FRONTEND-ARCHIVOS.md`](./INVENTARIO-FRONTEND-ARCHIVOS.md).

---

## 3. Configuración (copiar tal cual)

### 3.1 `.env.local`

```env
NEXT_PUBLIC_API_URL=http://localhost:5088/api/v1
NEXT_PUBLIC_APP_URL=http://localhost:3000
```

### 3.2 `.env.example`

```env
NEXT_PUBLIC_API_URL=http://localhost:5088/api/v1
NEXT_PUBLIC_APP_URL=http://localhost:3000
```

### 3.3 `next.config.ts`

```typescript
import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: 'placehold.co', pathname: '/**' },
      { protocol: 'http', hostname: 'localhost', port: '5088', pathname: '/**' },
    ],
  },
};

export default nextConfig;
```

### 3.4 `components.json`

```json
{
  "$schema": "https://ui.shadcn.com/schema.json",
  "style": "new-york",
  "rsc": true,
  "tsx": true,
  "tailwind": {
    "config": "tailwind.config.ts",
    "css": "src/app/globals.css",
    "baseColor": "zinc",
    "cssVariables": true
  },
  "aliases": {
    "components": "@/shared/ui",
    "utils": "@/shared/lib/utils",
    "ui": "@/shared/ui",
    "lib": "@/shared/lib",
    "hooks": "@/shared/hooks"
  }
}
```

### 3.5 `tsconfig.json` (paths)

```json
{
  "compilerOptions": {
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

### 3.6 `src/shared/config/env.ts`

```typescript
export const env = {
  apiUrl: process.env.NEXT_PUBLIC_API_URL!,
  appUrl: process.env.NEXT_PUBLIC_APP_URL!,
} as const;
```

### 3.7 `src/shared/lib/utils.ts`

```typescript
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
```

### 3.8 `src/shared/lib/format-money.ts`

```typescript
export const formatMoney = (amount: number, currency = 'MXN') =>
  new Intl.NumberFormat('es-MX', { style: 'currency', currency }).format(amount);
```

### 3.9 `src/shared/lib/query-keys.ts`

```typescript
export const queryKeys = {
  catalogHome: ['catalog', 'home'] as const,
  families: ['catalog', 'families'] as const,
  family: (slug: string) => ['catalog', 'family', slug] as const,
  category: (slug: string) => ['catalog', 'category', slug] as const,
  subcategory: (slug: string) => ['catalog', 'subcategory', slug] as const,
  products: (qs: string) => ['catalog', 'products', qs] as const,
  product: (slug: string) => ['catalog', 'product', slug] as const,
  productReviews: (slug: string) => ['catalog', 'reviews', slug] as const,
  cart: ['cart'] as const,
  wishlist: ['wishlist'] as const,
  addresses: ['addresses'] as const,
  orders: (page: number, status?: string) => ['orders', page, status] as const,
  order: (id: string) => ['order', id] as const,
  orderTracking: (id: string) => ['order', id, 'tracking'] as const,
  me: ['auth', 'me'] as const,
  adminDashboard: ['admin', 'dashboard'] as const,
  adminCovers: ['admin', 'covers'] as const,
  adminFamilies: ['admin', 'families'] as const,
  adminProducts: (page: number) => ['admin', 'products', page] as const,
  adminOrders: (page: number, status?: string) => ['admin', 'orders', page, status] as const,
  adminShipments: (page: number) => ['admin', 'shipments', page] as const,
  adminDrivers: ['admin', 'drivers'] as const,
  adminInventory: ['admin', 'inventory'] as const,
  driverShipments: (page: number) => ['driver', 'shipments', page] as const,
  driverMe: ['driver', 'me'] as const,
};
```

### 3.10 `src/app/globals.css`

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    --brand: 270 70% 55%;
    --background: 240 10% 3.9%;
    --foreground: 0 0% 98%;
    --card: 240 10% 6%;
    --card-foreground: 0 0% 98%;
    --primary: 270 70% 55%;
    --primary-foreground: 0 0% 100%;
    --muted: 240 5% 26%;
    --muted-foreground: 240 5% 64.9%;
    --border: 240 6% 20%;
    --radius: 0.5rem;
  }
  * { @apply border-border; }
  body { @apply bg-background text-foreground antialiased; }
}
```

### 3.11 `src/shared/providers/app-providers.tsx`

```tsx
'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from 'next-themes';
import { Toaster } from '@/shared/ui/sonner';
import { useState } from 'react';

export function AppProviders({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: { staleTime: 60_000, retry: 1, refetchOnWindowFocus: false },
        },
      }),
  );

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider attribute="class" defaultTheme="dark" enableSystem={false}>
        {children}
        <Toaster richColors position="top-right" />
      </ThemeProvider>
    </QueryClientProvider>
  );
}
```

### 3.12 `src/app/providers.tsx`

```tsx
export { AppProviders as Providers } from '@/shared/providers/app-providers';
```

### 3.13 `src/app/layout.tsx`

```tsx
import type { Metadata } from 'next';
import './globals.css';
import { Providers } from './providers';

export const metadata: Metadata = {
  title: { default: 'Ecommerce', template: '%s | Ecommerce' },
  description: 'Tienda en línea',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es" suppressHydrationWarning>
      <body className="min-h-screen">
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
```

---

## 4. Tipos TypeScript

### 4.1 `src/entities/user/model/types.ts`

```typescript
export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  driverId?: string | null;
  phone?: string | null;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
  permissions: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterCustomerRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
}

export interface RegisterDriverRequest extends RegisterCustomerRequest {
  licenseNumber: string;
  vehiclePlate: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phone?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
```

### 4.2 `src/entities/catalog/model/types.ts`

```typescript
export interface SubcategoryDto {
  id: string;
  name: string;
  slug: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  slug: string;
  subcategories: SubcategoryDto[];
}

export interface FamilyDto {
  id: string;
  name: string;
  slug: string;
  categories: CategoryDto[];
}

export interface CoverDto {
  id: string;
  title: string;
  imageUrl: string;
  linkUrl?: string | null;
  sortOrder: number;
}

export interface CatalogHomeDto {
  covers: CoverDto[];
  latestProducts: ProductListItemDto[];
}

export interface ProductListItemDto {
  id: string;
  name: string;
  slug: string;
  price: number;
  primaryImage?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
}

export type PagedProducts = PagedResult<ProductListItemDto>;
```

### 4.3 `src/entities/product/model/types.ts`

```typescript
export interface CatalogOptionValueDto {
  id: string;
  value: string;
  sortOrder: number;
}

export interface CatalogOptionDto {
  id: string;
  name: string;
  sortOrder: number;
  values: CatalogOptionValueDto[];
}

export interface ProductVariantDto {
  id: string;
  sku: string;
  price: number;
  available: number;
  optionValueIds: string[];
}

export interface ProductDetailDto {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  basePrice: number;
  options: CatalogOptionDto[];
  variants: ProductVariantDto[];
  images: string[];
}

export interface ResolvedVariantDto {
  variantId: string;
  sku: string;
  price: number;
  available: number;
  optionValueIds: string[];
}
```

### 4.4 `src/entities/cart/model/types.ts`

```typescript
export interface CartItemDto {
  id: string;
  variantId: string;
  productName: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface CartDto {
  id: string;
  guestToken?: string | null;
  items: CartItemDto[];
  subtotal: number;
}

export interface AddCartItemRequest {
  variantId: string;
  quantity: number;
}
```

### 4.5 `src/entities/order/model/types.ts`

```typescript
export interface OrderSummaryDto {
  id: string;
  orderNumber: string;
  status: string;
  total: number;
  createdAt: string;
}

export interface OrderItemDto {
  productName: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderAddressDto {
  fullName: string;
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone: string;
}

export interface PaymentInfoDto {
  status: string;
  amount: number;
  paidAt?: string | null;
}

export interface OrderShipmentInfoDto {
  shipmentId: string;
  status: string;
  trackingNumber?: string | null;
  driverName?: string | null;
  shippedAt?: string | null;
}

export interface OrderDetailDto {
  id: string;
  orderNumber: string;
  status: string;
  subtotal: number;
  shippingCost: number;
  total: number;
  createdAt: string;
  items: OrderItemDto[];
  address?: OrderAddressDto | null;
  payment?: PaymentInfoDto | null;
  shipment?: OrderShipmentInfoDto | null;
}

export interface CheckoutRequest {
  addressId?: string;
  fullName?: string;
  street?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  phone?: string;
  shippingCost: number;
  couponCode?: string;
}

export interface CheckoutResultDto {
  orderId: string;
  orderNumber: string;
  subtotal: number;
  discountAmount: number;
  couponCode?: string | null;
  total: number;
  status: string;
}

export interface PaymentResultDto {
  orderId: string;
  status: string;
  reference?: string | null;
}

export interface PagedOrdersDto {
  items: OrderSummaryDto[];
  total: number;
  page: number;
  pageSize: number;
}
```

### 4.6 Otros tipos (crear archivos equivalentes)

**`src/entities/address/model/types.ts`**

```typescript
export interface AddressDto {
  id: string;
  label: string;
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone?: string | null;
  isDefault: boolean;
}

export interface SaveAddressRequest {
  label: string;
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone?: string;
  isDefault?: boolean;
}
```

**`src/entities/wishlist/model/types.ts`**

```typescript
export interface WishlistItemDto {
  productId: string;
  name: string;
  slug: string;
  price: number;
  primaryImage?: string | null;
  addedAt: string;
}
```

**`src/entities/review/model/types.ts`**

```typescript
export interface ProductReviewDto {
  id: string;
  authorName: string;
  rating: number;
  title?: string | null;
  comment: string;
  createdAt: string;
}

export interface ProductReviewSummaryDto {
  averageRating: number;
  totalCount: number;
}

export interface ProductReviewsPageDto {
  summary: ProductReviewSummaryDto;
  items: ProductReviewDto[];
}

export interface CreateProductReviewRequest {
  rating: number;
  title?: string;
  comment: string;
}
```

**`src/entities/driver/model/types.ts`**

```typescript
export interface DriverProfileDto {
  driverId: string;
  name: string;
  phone: string;
  licenseNumber: string;
  vehiclePlate: string;
}

export interface DriverShipmentDto {
  id: string;
  orderId: string;
  orderNumber: string;
  status: string;
  trackingNumber?: string | null;
  customerName: string;
  addressLine: string;
  city: string;
  phone: string;
  createdAt: string;
}
```

> Ajusta `DriverShipmentDto` según la respuesta real de `GET /driver/shipments` (mapear en el handler del API si hace falta campos extra).

---

## 5. Cliente API

### 5.1 `src/shared/api/client.ts`

```typescript
import { env } from '@/shared/config/env';

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public body?: unknown,
  ) {
    super(message);
  }
}

function getAccessToken(): string | null {
  if (typeof window === 'undefined') return null;
  return sessionStorage.getItem('accessToken');
}

function getGuestToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('guestToken');
}

export type ApiOptions = RequestInit & {
  guest?: boolean;
  auth?: boolean;
};

export async function api<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };

  if (options.auth !== false) {
    const token = getAccessToken();
    if (token) headers.Authorization = `Bearer ${token}`;
  }

  if (options.guest) {
    const guest = getGuestToken();
    if (guest) headers['X-Guest-Token'] = guest;
  }

  const res = await fetch(`${env.apiUrl}${path}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    const msg =
      (body as { errors?: { message: string }[] })?.errors?.[0]?.message ??
      (body as { title?: string })?.title ??
      res.statusText;
    throw new ApiError(res.status, msg, body);
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

/** Guarda guestToken si el carrito lo devuelve */
export function persistGuestTokenFromCart(cart: { guestToken?: string | null }) {
  if (cart.guestToken && typeof window !== 'undefined') {
    localStorage.setItem('guestToken', cart.guestToken);
  }
}
```

### 5.2 `src/entities/user/api/auth-api.ts`

```typescript
import { api } from '@/shared/api/client';
import type {
  ChangePasswordRequest,
  LoginRequest,
  LoginResponse,
  RegisterCustomerRequest,
  RegisterDriverRequest,
  UpdateProfileRequest,
  UserDto,
} from '../model/types';

export const authApi = {
  login: (body: LoginRequest) =>
    api<LoginResponse>('/auth/login', { method: 'POST', body: JSON.stringify(body), auth: false }),

  registerCustomer: (body: RegisterCustomerRequest) =>
    api<LoginResponse>('/auth/register/customer', { method: 'POST', body: JSON.stringify(body), auth: false }),

  registerDriver: (body: RegisterDriverRequest) =>
    api<LoginResponse>('/auth/register/driver', { method: 'POST', body: JSON.stringify(body), auth: false }),

  refresh: (refreshToken: string) =>
    api<LoginResponse>('/auth/refresh', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
      auth: false,
    }),

  logout: () => api<void>('/auth/logout', { method: 'POST' }),

  me: () => api<UserDto>('/auth/me'),

  updateProfile: (body: UpdateProfileRequest) =>
    api<UserDto>('/auth/me', { method: 'PATCH', body: JSON.stringify(body) }),

  changePassword: (body: ChangePasswordRequest) =>
    api<void>('/auth/change-password', { method: 'POST', body: JSON.stringify(body) }),
};
```

### 5.3 `src/entities/catalog/api/catalog-api.ts`

```typescript
import { api } from '@/shared/api/client';
import type {
  CatalogHomeDto,
  CoverDto,
  FamilyDto,
  PagedProducts,
  ProductDetailDto,
} from '@/entities/catalog/model/types';
import type { ResolvedVariantDto } from '@/entities/product/model/types';

export const catalogApi = {
  getHome: (take = 12) => api<CatalogHomeDto>(`/catalog/home?take=${take}`, { auth: false }),

  getCovers: () => api<CoverDto[]>('/catalog/covers', { auth: false }),

  getFamilies: () => api<FamilyDto[]>('/catalog/families', { auth: false }),

  getFamily: (slug: string) => api<FamilyDto>(`/catalog/families/${slug}`, { auth: false }),

  getCategory: (slug: string) => api<CategoryDto>(`/catalog/categories/${slug}`, { auth: false }),

  getSubcategory: (slug: string) => api<SubcategoryDto>(`/catalog/subcategories/${slug}`, { auth: false }),

  getProducts: (params: URLSearchParams) =>
    api<PagedProducts>(`/catalog/products?${params}`, { auth: false }),

  search: (q: string, page = 1, pageSize = 20) =>
    api<PagedProducts>(`/catalog/search?q=${encodeURIComponent(q)}&page=${page}&pageSize=${pageSize}`, {
      auth: false,
    }),

  getProduct: (slug: string) => api<ProductDetailDto>(`/catalog/products/${slug}`, { auth: false }),

  resolveVariant: (slug: string, optionValueIds: string[]) =>
    api<ResolvedVariantDto>(`/catalog/products/${slug}/resolve-variant`, {
      method: 'POST',
      body: JSON.stringify({ optionValueIds }),
      auth: false,
    }),
};

// Re-export tipos usados arriba
import type { CategoryDto, SubcategoryDto } from '@/entities/catalog/model/types';
```

### 5.4 `src/entities/cart/api/cart-api.ts`

```typescript
import { api, persistGuestTokenFromCart } from '@/shared/api/client';
import type { AddCartItemRequest, CartDto } from '../model/types';

export const cartApi = {
  get: async () => {
    const cart = await api<CartDto>('/cart', { guest: true });
    persistGuestTokenFromCart(cart);
    return cart;
  },

  addItem: async (body: AddCartItemRequest) => {
    const cart = await api<CartDto>('/cart/items', {
      method: 'POST',
      body: JSON.stringify(body),
      guest: true,
    });
    persistGuestTokenFromCart(cart);
    return cart;
  },

  updateItem: (itemId: string, quantity: number) =>
    api<CartDto>(`/cart/items/${itemId}`, {
      method: 'PATCH',
      body: JSON.stringify({ quantity }),
      guest: true,
    }),

  removeItem: (itemId: string) =>
    api<CartDto>(`/cart/items/${itemId}`, { method: 'DELETE', guest: true }),

  clear: () => api<CartDto>('/cart', { method: 'DELETE', guest: true }),

  merge: (guestToken: string) =>
    api<CartDto>('/cart/merge', {
      method: 'POST',
      body: JSON.stringify({ guestToken }),
    }),
};
```

### 5.5 `src/entities/order/api/order-api.ts`

```typescript
import { api } from '@/shared/api/client';
import type {
  CheckoutRequest,
  CheckoutResultDto,
  OrderDetailDto,
  OrderTrackingDto,
  PagedOrdersDto,
  PaymentResultDto,
} from '../model/types';

export const orderApi = {
  checkout: (body: CheckoutRequest) =>
    api<CheckoutResultDto>('/checkout', { method: 'POST', body: JSON.stringify(body) }),

  list: (page = 1, pageSize = 20, status?: string) => {
    const q = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (status) q.set('status', status);
    return api<PagedOrdersDto>(`/orders?${q}`);
  },

  get: (id: string) => api<OrderDetailDto>(`/orders/${id}`),

  tracking: (id: string) => api<OrderTrackingDto>(`/orders/${id}/tracking`),

  pay: (id: string) => api<PaymentResultDto>(`/orders/${id}/pay`, { method: 'POST' }),

  cancel: (id: string) => api<void>(`/orders/${id}/cancel`, { method: 'POST' }),

  retryPayment: (id: string) => api<void>(`/orders/${id}/retry-payment`, { method: 'POST' }),
};
```

### 5.6 APIs restantes (mismo patrón)

| Archivo | Métodos → API |
|---------|----------------|
| `entities/address/api/address-api.ts` | GET/POST/PUT/DELETE/PATCH `/addresses` |
| `entities/wishlist/api/wishlist-api.ts` | GET/POST/DELETE `/wishlist` |
| `entities/review/api/review-api.ts` | GET/POST `/catalog/products/{slug}/reviews` |
| `entities/driver/api/driver-api.ts` | GET `/driver/me`, GET `/driver/shipments`, PATCH in-transit/delivered |
| `entities/admin/api/admin-api.ts` | Todos los `/admin/*` (ver sección 9) |

**`wishlist-api.ts`:**

```typescript
import { api } from '@/shared/api/client';
import type { WishlistItemDto } from '../model/types';

export const wishlistApi = {
  list: () => api<WishlistItemDto[]>('/wishlist'),
  add: (productId: string) => api<void>(`/wishlist/${productId}`, { method: 'POST' }),
  remove: (productId: string) => api<void>(`/wishlist/${productId}`, { method: 'DELETE' }),
};
```

**`review-api.ts`:**

```typescript
import { api } from '@/shared/api/client';
import type { CreateProductReviewRequest, ProductReviewDto, ProductReviewsPageDto } from '../model/types';

export const reviewApi = {
  list: (slug: string) => api<ProductReviewsPageDto>(`/catalog/products/${slug}/reviews`, { auth: false }),
  create: (slug: string, body: CreateProductReviewRequest) =>
    api<ProductReviewDto>(`/catalog/products/${slug}/reviews`, { method: 'POST', body: JSON.stringify(body) }),
};
```

**`driver-api.ts`:**

```typescript
import { api } from '@/shared/api/client';
import type { DriverProfileDto, DriverShipmentDto } from '../model/types';
import type { PagedResult } from '@/entities/catalog/model/types';

export const driverApi = {
  me: () => api<DriverProfileDto>('/driver/me'),
  shipments: (page = 1, pageSize = 20) =>
    api<PagedResult<DriverShipmentDto>>(`/driver/shipments?page=${page}&pageSize=${pageSize}`),
  inTransit: (id: string) => api<void>(`/driver/shipments/${id}/in-transit`, { method: 'PATCH' }),
  delivered: (id: string) => api<void>(`/driver/shipments/${id}/delivered`, { method: 'PATCH' }),
};
```

Cada entity: `index.ts` → `export * from './api/...'; export * from './model/types';`

---

## 6. Autenticación

### 6.1 `src/entities/user/model/auth-store.ts`

```typescript
'use client';

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UserDto } from './types';

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: UserDto | null;
  permissions: string[];
  setSession: (access: string, refresh: string, user: UserDto, permissions: string[]) => void;
  clear: () => void;
  hasPermission: (p: string) => boolean;
  hasRole: (r: string) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      permissions: [],
      setSession: (access, refresh, user, permissions) => {
        if (typeof window !== 'undefined') sessionStorage.setItem('accessToken', access);
        set({ accessToken: access, refreshToken: refresh, user, permissions });
      },
      clear: () => {
        if (typeof window !== 'undefined') sessionStorage.removeItem('accessToken');
        set({ accessToken: null, refreshToken: null, user: null, permissions: [] });
      },
      hasPermission: (p) => get().permissions.includes(p),
      hasRole: (r) => get().user?.roles.includes(r) ?? false,
    }),
    { name: 'auth-store', partialize: (s) => ({ refreshToken: s.refreshToken, user: s.user, permissions: s.permissions }) },
  ),
);
```

### 6.2 `src/middleware.ts`

```typescript
import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const token = request.cookies.get('accessToken')?.value;
  const path = request.nextUrl.pathname;

  const needsAuth =
    path.startsWith('/checkout') ||
    path.startsWith('/orders') ||
    path.startsWith('/account') ||
    path.startsWith('/wishlist') ||
    path.startsWith('/admin') ||
    path.startsWith('/driver');

  if (needsAuth && !token) {
    const login = new URL('/login', request.url);
    login.searchParams.set('redirect', path);
    return NextResponse.redirect(login);
  }

  if (path.startsWith('/admin') && token) {
    // Opcional: decodificar JWT en edge o confiar en 403 del API
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/checkout/:path*', '/orders/:path*', '/account/:path*', '/wishlist', '/admin/:path*', '/driver/:path*'],
};
```

> Tras login, guarda también cookie `accessToken` (además de sessionStorage) para que el middleware funcione:

```typescript
document.cookie = `accessToken=${data.accessToken}; path=/; max-age=${60 * 15}; SameSite=Lax`;
```

### 6.3 `src/features/auth/login/ui/login-form.tsx`

```tsx
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useRouter, useSearchParams } from 'next/navigation';
import { authApi } from '@/entities/user/api/auth-api';
import { useAuthStore } from '@/entities/user/model/auth-store';
import { cartApi } from '@/entities/cart/api/cart-api';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { toast } from 'sonner';

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
});

export function LoginForm() {
  const router = useRouter();
  const params = useSearchParams();
  const setSession = useAuthStore((s) => s.setSession);
  const form = useForm({ resolver: zodResolver(schema) });

  const onSubmit = form.handleSubmit(async (data) => {
    try {
      const res = await authApi.login(data);
      setSession(res.accessToken, res.refreshToken, res.user, res.permissions);
      document.cookie = `accessToken=${res.accessToken}; path=/; max-age=900; SameSite=Lax`;
      const guest = localStorage.getItem('guestToken');
      if (guest) await cartApi.merge(guest);
      toast.success('Bienvenido');
      router.push(params.get('redirect') ?? '/');
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Error de login');
    }
  });

  return (
    <Card className="mx-auto max-w-md border-white/10 bg-white/5">
      <CardHeader>
        <CardTitle>Iniciar sesión</CardTitle>
      </CardHeader>
      <CardContent>
        <form onSubmit={onSubmit} className="space-y-4">
          <div>
            <Label>Email</Label>
            <Input type="email" {...form.register('email')} />
          </div>
          <div>
            <Label>Contraseña</Label>
            <Input type="password" {...form.register('password')} />
          </div>
          <Button type="submit" className="w-full">Entrar</Button>
        </form>
      </CardContent>
    </Card>
  );
}
```

---

## 7. Tienda

### 7.1 Layout tienda

**`src/app/(store)/layout.tsx`**

```tsx
import { StoreLayout } from '@/widgets/store-layout';

export default function Layout({ children }: { children: React.ReactNode }) {
  return <StoreLayout>{children}</StoreLayout>;
}
```

**`src/widgets/store-layout/ui/store-layout.tsx`**

```tsx
import { StoreHeader } from '@/widgets/store-header';
import { StoreFooter } from '@/widgets/store-footer';

export function StoreLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <StoreHeader />
      <main className="container mx-auto px-4 py-8">{children}</main>
      <StoreFooter />
    </>
  );
}
```

### 7.2 Thin pages tienda (copiar todas)

```tsx
// src/app/(store)/page.tsx
export { HomePage as default } from '@/pages/home';

// src/app/(store)/search/page.tsx
export { SearchPage as default } from '@/pages/search';

// src/app/(store)/families/[slug]/page.tsx
export { FamilyCatalogPage as default } from '@/pages/family-catalog';

// src/app/(store)/categories/[slug]/page.tsx
export { CategoryCatalogPage as default } from '@/pages/category-catalog';

// src/app/(store)/subcategories/[slug]/page.tsx
export { SubcategoryCatalogPage as default } from '@/pages/subcategory-catalog';

// src/app/(store)/products/[slug]/page.tsx
export { ProductDetailPage as default } from '@/pages/product-detail';

// src/app/(store)/cart/page.tsx
export { CartPage as default } from '@/pages/cart';

// src/app/(store)/wishlist/page.tsx
export { WishlistPage as default } from '@/pages/wishlist';

// src/app/(store)/checkout/shipping/page.tsx
export { CheckoutShippingPage as default } from '@/pages/checkout-shipping';

// src/app/(store)/checkout/payment/[orderId]/page.tsx
export { CheckoutPaymentPage as default } from '@/pages/checkout-payment';

// src/app/(store)/checkout/success/[orderId]/page.tsx
export { CheckoutSuccessPage as default } from '@/pages/checkout-success';

// src/app/(store)/orders/page.tsx
export { OrdersListPage as default } from '@/pages/orders-list';

// src/app/(store)/orders/[id]/page.tsx
export { OrderDetailPage as default } from '@/pages/order-detail';

// src/app/(store)/account/page.tsx
export { AccountPage as default } from '@/pages/account';

// src/app/(store)/account/addresses/page.tsx
export { AccountAddressesPage as default } from '@/pages/account-addresses';

// src/app/(store)/account/addresses/new/page.tsx
export { AccountAddressFormPage as default } from '@/pages/account-address-form';

// src/app/(store)/account/addresses/[id]/edit/page.tsx
export { AccountAddressFormPage as default } from '@/pages/account-address-form';

// src/app/(store)/account/password/page.tsx
export { AccountPasswordPage as default } from '@/pages/account-password';
```

### 7.3 `src/pages/home/ui/home-page.tsx`

```tsx
import { CoverCarousel } from '@/widgets/cover-carousel';
import { ProductGrid } from '@/widgets/product-grid';
import { catalogApi } from '@/entities/catalog/api/catalog-api';

export async function HomePage() {
  const home = await catalogApi.getHome(12);
  return (
    <div className="space-y-12">
      <CoverCarousel covers={home.covers} />
      <section>
        <h2 className="mb-6 text-2xl font-semibold">Novedades</h2>
        <ProductGrid initialItems={home.latestProducts} />
      </section>
    </div>
  );
}
```

**`src/pages/home/index.ts`:** `export { HomePage } from './ui/home-page';`

### 7.4 `src/pages/product-detail/ui/product-detail-page.tsx`

```tsx
'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { catalogApi } from '@/entities/catalog/api/catalog-api';
import { queryKeys } from '@/shared/lib/query-keys';
import { VariantSelector } from '@/entities/product/ui/variant-selector';
import { AddToCartButton } from '@/features/cart/add-to-cart';
import { WishlistButton } from '@/features/wishlist/toggle-wishlist';
import { ProductReviews } from '@/widgets/product-reviews';
import { formatMoney } from '@/shared/lib/format-money';
import Image from 'next/image';
import { Skeleton } from '@/shared/ui/skeleton';

export function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const { data: product, isLoading } = useQuery({
    queryKey: queryKeys.product(slug),
    queryFn: () => catalogApi.getProduct(slug),
  });

  if (isLoading || !product) return <Skeleton className="h-96 w-full" />;

  return (
    <div className="grid gap-10 lg:grid-cols-2">
      <div className="relative aspect-square overflow-hidden rounded-lg border border-white/10">
        {product.images[0] && (
          <Image src={product.images[0]} alt={product.name} fill className="object-cover" />
        )}
      </div>
      <div className="space-y-6">
        <div className="flex items-start justify-between gap-4">
          <h1 className="text-3xl font-bold">{product.name}</h1>
          <WishlistButton productId={product.id} />
        </div>
        <p className="text-muted-foreground">{product.description}</p>
        <p className="text-2xl font-semibold text-primary">{formatMoney(product.basePrice)}</p>
        <VariantSelector product={product} slug={slug} />
        <AddToCartButton />
        <ProductReviews slug={slug} />
      </div>
    </div>
  );
}
```

### 7.5 `src/entities/product/ui/variant-selector.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { catalogApi } from '@/entities/catalog/api/catalog-api';
import type { ProductDetailDto } from '../model/types';
import { Button } from '@/shared/ui/button';
import { cn } from '@/shared/lib/utils';

type Props = { product: ProductDetailDto; slug: string; onResolved?: (variantId: string) => void };

export function VariantSelector({ product, slug, onResolved }: Props) {
  const [selected, setSelected] = useState<Record<string, string>>({});

  const optionValueIds = Object.values(selected);
  const { data: resolved } = useQuery({
    queryKey: ['resolve', slug, optionValueIds.join(',')],
    queryFn: () => catalogApi.resolveVariant(slug, optionValueIds),
    enabled: optionValueIds.length === product.options.length && product.options.length > 0,
  });

  if (resolved?.variantId) onResolved?.(resolved.variantId);

  return (
    <div className="space-y-4">
      {product.options.map((opt) => (
        <div key={opt.id}>
          <p className="mb-2 text-sm font-medium">{opt.name}</p>
          <div className="flex flex-wrap gap-2">
            {opt.values.map((v) => (
              <Button
                key={v.id}
                type="button"
                size="sm"
                variant={selected[opt.id] === v.id ? 'default' : 'outline'}
                className={cn(selected[opt.id] === v.id && 'ring-2 ring-primary')}
                onClick={() => setSelected((s) => ({ ...s, [opt.id]: v.id }))}
              >
                {v.value}
              </Button>
            ))}
          </div>
        </div>
      ))}
      {resolved && (
        <p className="text-sm text-muted-foreground">
          SKU {resolved.sku} · Stock: {resolved.available}
        </p>
      )}
    </div>
  );
}
```

### 7.6 `src/features/cart/add-to-cart/ui/add-to-cart-button.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { cartApi } from '@/entities/cart/api/cart-api';
import { queryKeys } from '@/shared/lib/query-keys';
import { Button } from '@/shared/ui/button';
import { toast } from 'sonner';

export function AddToCartButton({ variantId, disabled }: { variantId?: string; disabled?: boolean }) {
  const qc = useQueryClient();
  const [qty, setQty] = useState(1);
  const mutation = useMutation({
    mutationFn: () => cartApi.addItem({ variantId: variantId!, quantity: qty }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.cart });
      toast.success('Agregado al carrito');
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <div className="flex gap-2">
      <input
        type="number"
        min={1}
        value={qty}
        onChange={(e) => setQty(Number(e.target.value))}
        className="w-16 rounded border border-white/10 bg-transparent px-2"
      />
      <Button disabled={!variantId || disabled} onClick={() => mutation.mutate()}>
        Agregar al carrito
      </Button>
    </div>
  );
}
```

---

## 8. Cuenta, pedidos y wishlist

### 8.1 `src/pages/checkout-shipping/ui/checkout-shipping-page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { addressApi } from '@/entities/address/api/address-api';
import { orderApi } from '@/entities/order/api/order-api';
import { CheckoutOrderSummary } from '@/widgets/checkout-order-summary';
import { CouponField } from '@/features/checkout/apply-coupon';
import { Button } from '@/shared/ui/button';
import { toast } from 'sonner';

export function CheckoutShippingPage() {
  const router = useRouter();
  const [addressId, setAddressId] = useState<string>();
  const [couponCode, setCouponCode] = useState<string>();
  const { data: addresses } = useQuery({ queryKey: ['addresses'], queryFn: addressApi.list });

  const confirm = async () => {
    try {
      const order = await orderApi.checkout({
        addressId,
        shippingCost: 99,
        couponCode,
      });
      router.push(`/checkout/payment/${order.orderId}`);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Error en checkout');
    }
  };

  return (
    <div className="grid gap-8 lg:grid-cols-3">
      <div className="lg:col-span-2 space-y-6">
        <h1 className="text-2xl font-bold">Envío</h1>
        {addresses?.map((a) => (
          <label key={a.id} className="flex cursor-pointer gap-3 rounded border border-white/10 p-4">
            <input type="radio" name="addr" onChange={() => setAddressId(a.id)} />
            <div>
              <p className="font-medium">{a.label}</p>
              <p className="text-sm text-muted-foreground">{a.street}, {a.city}</p>
            </div>
          </label>
        ))}
        <CouponField value={couponCode} onChange={setCouponCode} />
      </div>
      <CheckoutOrderSummary />
      <Button className="lg:col-span-3" onClick={confirm} disabled={!addressId}>
        Confirmar pedido
      </Button>
    </div>
  );
}
```

### 8.2 `src/pages/checkout-payment/ui/checkout-payment-page.tsx`

```tsx
'use client';

import { useParams, useRouter } from 'next/navigation';
import { orderApi } from '@/entities/order/api/order-api';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { toast } from 'sonner';

export function CheckoutPaymentPage() {
  const { orderId } = useParams<{ orderId: string }>();
  const router = useRouter();

  const pay = async () => {
    try {
      await orderApi.pay(orderId);
      toast.success('Pago simulado correcto');
      router.push(`/checkout/success/${orderId}`);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Error al pagar');
    }
  };

  return (
    <Card className="mx-auto max-w-lg border-white/10 bg-white/5">
      <CardHeader>
        <CardTitle>Pago (simulado)</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-muted-foreground">
          En producción aquí iría Stripe/Niubiz. El API usa pago mock.
        </p>
        <input placeholder="Número de tarjeta" className="w-full rounded border border-white/10 bg-transparent px-3 py-2" />
        <Button className="w-full" onClick={pay}>Pagar ahora</Button>
      </CardContent>
    </Card>
  );
}
```

### 8.3 Pantallas restantes tienda (resumen implementación)

| Page slice | Componentes clave | API |
|------------|-------------------|-----|
| `cart` | `CartPage`, líneas, `QuantityStepper`, link checkout | `cartApi` |
| `wishlist` | grid `ProductCard`, quitar favorito | `wishlistApi` |
| `orders-list` | tabla + filtro status + paginación | `orderApi.list` |
| `order-detail` | ítems, `OrderTrackingCard`, cancel/retry/pay | `orderApi.get`, `tracking` |
| `account` | `ProfileForm` | `authApi.updateProfile` |
| `account-addresses` | lista + links new/edit | `addressApi` |
| `account-password` | `ChangePasswordForm` | `authApi.changePassword` |
| `search` | `ProductGrid` + `?q=` | `catalogApi.search` |
| `family-catalog` etc. | `ProductGrid` + filtros URL | `catalogApi.getProducts` |

---

## 9. Panel admin

### 9.1 Layout admin

**`src/app/(admin)/admin/layout.tsx`**

```tsx
import { AdminLayout } from '@/widgets/admin-layout';

export default function Layout({ children }: { children: React.ReactNode }) {
  return <AdminLayout>{children}</AdminLayout>;
}
```

### 9.2 `src/widgets/admin-sidebar/model/nav-config.ts`

```typescript
export const adminNav = [
  {
    title: 'General',
    items: [
      { label: 'Dashboard', href: '/admin/dashboard', permission: 'admin.dashboard.view' },
    ],
  },
  {
    title: 'Tienda',
    items: [
      { label: 'Portadas', href: '/admin/covers', permission: 'admin.covers.view' },
      { label: 'Familias', href: '/admin/families', permission: 'admin.families.view' },
      { label: 'Categorías', href: '/admin/categories', permission: 'admin.categories.view' },
      { label: 'Subcategorías', href: '/admin/subcategories', permission: 'admin.subcategories.view' },
      { label: 'Productos', href: '/admin/products', permission: 'admin.products.view' },
      { label: 'Inventario', href: '/admin/inventory', permission: 'admin.stock.view' },
    ],
  },
  {
    title: 'Operaciones',
    items: [
      { label: 'Pedidos', href: '/admin/orders', permission: 'admin.orders.view' },
      { label: 'Envíos', href: '/admin/shipments', permission: 'admin.shipments.view' },
      { label: 'Repartidores', href: '/admin/drivers', permission: 'admin.drivers.view' },
    ],
  },
];
```

### 9.3 Thin pages admin (lista para copiar)

```tsx
// dashboard
export { AdminDashboardPage as default } from '@/pages/admin-dashboard';
// covers
export { AdminCoversPage as default } from '@/pages/admin-covers';
export { AdminCoverFormPage as default } from '@/pages/admin-cover-form';
// families, categories, subcategories, products, variants, options
export { AdminFamiliesPage as default } from '@/pages/admin-families';
export { AdminFamilyFormPage as default } from '@/pages/admin-family-form';
// ... (ver INVENTARIO por cada ruta)
export { AdminOrdersPage as default } from '@/pages/admin-orders';
export { AdminOrderDetailPage as default } from '@/pages/admin-order-detail';
export { AdminShipmentsPage as default } from '@/pages/admin-shipments';
export { AdminDriversPage as default } from '@/pages/admin-drivers';
```

### 9.4 `src/entities/admin/api/admin-api.ts` (extracto)

```typescript
import { api } from '@/shared/api/client';

export const adminApi = {
  dashboard: () => api<DashboardStats>('/admin/dashboard'),
  // Covers
  listCovers: () => api<CoverAdmin[]>('/admin/covers'),
  saveCover: (body: SaveCoverRequest, id?: string) =>
    id
      ? api(`/admin/covers/${id}`, { method: 'PUT', body: JSON.stringify(body) })
      : api('/admin/covers', { method: 'POST', body: JSON.stringify(body) }),
  // Catalog
  listFamilies: () => api('/admin/catalog/families'),
  saveFamily: (body: unknown, id?: string) =>
    id ? api(`/admin/catalog/families/${id}`, { method: 'PUT', body: JSON.stringify(body) }) : api('/admin/catalog/families', { method: 'POST', body: JSON.stringify(body) }),
  // Products, variants, options, inventory, orders, shipments, drivers
  listOrders: (page: number, status?: string) => {
    const q = new URLSearchParams({ page: String(page), pageSize: '20' });
    if (status) q.set('status', status);
    return api(`/admin/orders?${q}`);
  },
  orderReady: (id: string) => api(`/admin/orders/${id}/ready`, { method: 'POST' }),
  orderTicketUrl: (id: string) => `${process.env.NEXT_PUBLIC_API_URL}/admin/orders/${id}/ticket`,
  listShipments: (page: number) => api(`/admin/shipments?page=${page}&pageSize=20`),
  shipmentInTransit: (id: string) => api(`/admin/shipments/${id}/in-transit`, { method: 'PATCH' }),
  shipmentDelivered: (id: string) => api(`/admin/shipments/${id}/delivered`, { method: 'PATCH' }),
};
```

### 9.5 Plantilla lista admin `src/pages/admin-families/ui/admin-families-page.tsx`

```tsx
'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminApi } from '@/entities/admin/api/admin-api';
import { AdminDataTable } from '@/widgets/admin-data-table';
import { PageHeader } from '@/shared/ui/page-header';
import { Button } from '@/shared/ui/button';
import Link from 'next/link';
import { toast } from 'sonner';

export function AdminFamiliesPage() {
  const qc = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ['admin', 'families'], queryFn: adminApi.listFamilies });
  const remove = useMutation({
    mutationFn: (id: string) => adminApi.deleteFamily(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin', 'families'] }); toast.success('Eliminado'); },
  });

  return (
    <div>
      <PageHeader title="Familias" action={<Button asChild><Link href="/admin/families/new">Nueva</Link></Button>} />
      <AdminDataTable
        loading={isLoading}
        columns={[
          { key: 'name', label: 'Nombre' },
          { key: 'slug', label: 'Slug' },
        ]}
        rows={data ?? []}
        onDelete={(row) => remove.mutate(row.id)}
        editHref={(row) => `/admin/families/${row.id}/edit`}
      />
    </div>
  );
}
```

Repite el mismo patrón para: **covers, categories, subcategories, products, drivers, shipments, inventory, orders** cambiando `adminApi.*` y columnas.

### 9.6 Pantallas admin — tabla completa

| Ruta | Page | Formulario | API principal |
|------|------|------------|---------------|
| `/admin/dashboard` | `admin-dashboard` | — | GET `/admin/dashboard` |
| `/admin/covers` | `admin-covers` | `admin-cover-form` | `/admin/covers` |
| `/admin/families` | `admin-families` | `admin-family-form` | `/admin/catalog/families` |
| `/admin/categories` | `admin-categories` | `admin-category-form` | `/admin/catalog/categories` |
| `/admin/subcategories` | `admin-subcategories` | `admin-subcategory-form` | `/admin/catalog/subcategories` |
| `/admin/products` | `admin-products` | `admin-product-form` | `/admin/catalog/products` |
| `/admin/products/[id]/variants` | `admin-product-variants` | inline | `/admin/catalog/variants` |
| `/admin/products/[id]/options` | `admin-product-options` | modales valores | `/admin/products/{id}/options` |
| `/admin/inventory` | `admin-inventory` | PATCH stock | `/admin/inventory` |
| `/admin/orders` | `admin-orders` | — | GET `/admin/orders` |
| `/admin/orders/[id]` | `admin-order-detail` | acciones ready, PDF | GET ticket, POST ready |
| `/admin/shipments` | `admin-shipments` | `admin-shipment-form` | POST `/admin/shipments` |
| `/admin/drivers` | `admin-drivers` | `admin-driver-form` | `/admin/drivers` |

---

## 10. App repartidor

### 10.1 Layout

**`src/app/(driver)/driver/layout.tsx`**

```tsx
import { DriverLayout } from '@/widgets/driver-layout';

export default function Layout({ children }: { children: React.ReactNode }) {
  return <DriverLayout>{children}</DriverLayout>;
}
```

### 10.2 Thin pages

```tsx
// src/app/(driver)/driver/page.tsx
import { redirect } from 'next/navigation';
export default function Page() { redirect('/driver/shipments'); }

// src/app/(driver)/driver/shipments/page.tsx
export { DriverShipmentsPage as default } from '@/pages/driver-shipments';

// src/app/(driver)/driver/shipments/[id]/page.tsx
export { DriverShipmentDetailPage as default } from '@/pages/driver-shipment-detail';
```

### 10.3 `src/pages/driver-shipments/ui/driver-shipments-page.tsx`

```tsx
'use client';

import { useQuery } from '@tanstack/react-query';
import { driverApi } from '@/entities/driver/api/driver-api';
import { DriverShipmentCard } from '@/widgets/driver-shipment-card';
import { queryKeys } from '@/shared/lib/query-keys';

export function DriverShipmentsPage() {
  const { data, isLoading } = useQuery({
    queryKey: queryKeys.driverShipments(1),
    queryFn: () => driverApi.shipments(1, 20),
  });

  if (isLoading) return <p>Cargando envíos…</p>;

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Mis envíos</h1>
      {data?.items.map((s) => (
        <DriverShipmentCard key={s.id} shipment={s} />
      ))}
    </div>
  );
}
```

**`DriverShipmentCard`:** botones `MarkInTransit` / `MarkDelivered` → `driverApi.inTransit` / `delivered`.

### 10.4 Auth repartidor

- Login mismo `/login`; si `user.roles` incluye `driver` → mostrar link "Ir a panel repartidor".
- Registro: `/register/driver` → `authApi.registerDriver`.

---

## 11. Widgets

| Widget | Archivo | Usado en |
|--------|---------|----------|
| `store-header` | `widgets/store-header/ui/store-header.tsx` | Todas las páginas tienda |
| `store-footer` | `widgets/store-footer/ui/store-footer.tsx` | Layout tienda |
| `catalog-drawer` | `widgets/catalog-drawer/ui/catalog-drawer.tsx` | Header móvil |
| `cover-carousel` | `widgets/cover-carousel/ui/cover-carousel.tsx` | Home |
| `product-grid` | `widgets/product-grid/ui/product-grid.tsx` | Catálogos, búsqueda |
| `product-reviews` | `widgets/product-reviews/ui/product-reviews-list.tsx` | Detalle producto |
| `cart-summary` | `widgets/cart-summary/ui/cart-summary.tsx` | Carrito |
| `checkout-order-summary` | `widgets/checkout-order-summary/ui/checkout-order-summary.tsx` | Checkout |
| `order-tracking-card` | `widgets/order-tracking-card/ui/order-tracking-card.tsx` | Detalle pedido |
| `admin-layout` | `widgets/admin-layout/ui/admin-layout.tsx` | Admin |
| `admin-sidebar` | `widgets/admin-sidebar/ui/admin-sidebar.tsx` | Admin |
| `admin-data-table` | `widgets/admin-data-table/ui/admin-data-table.tsx` | Listados admin |
| `admin-stats-cards` | `widgets/admin-stats-cards/ui/admin-stats-cards.tsx` | Dashboard |
| `driver-layout` | `widgets/driver-layout/ui/driver-layout.tsx` | Driver |
| `driver-shipment-card` | `widgets/driver-shipment-card/ui/driver-shipment-card.tsx` | Envíos driver |

### Tokens UI (Tailwind)

| Uso | Clases |
|-----|--------|
| Fondo tienda | `bg-background` (zinc oscuro) |
| Fondo admin | `bg-gray-950` |
| Acento | `bg-primary`, `text-primary` (purple) |
| Cards | `border border-white/10 bg-white/5` |

---

## 12. Estado y checklist

### Auth + carrito invitado

1. Primera visita → `GET /cart` sin token → API devuelve `guestToken` → `localStorage`.
2. Login → `POST /cart/merge` con `guestToken`.
3. JWT en `sessionStorage` + cookie para middleware.

### Mapeo pantalla → API

| Pantalla | Endpoints |
|----------|-----------|
| Home | `GET /catalog/home` |
| Producto | `GET /catalog/products/{slug}`, `POST resolve-variant`, `GET/POST reviews` |
| Carrito | `GET/POST/PATCH/DELETE /cart/*` |
| Checkout | `POST /checkout`, `POST /orders/{id}/pay` |
| Pedidos | `GET /orders`, `GET /orders/{id}`, `GET tracking`, `POST cancel` |
| Wishlist | `GET/POST/DELETE /wishlist` |
| Admin * | `/admin/*` según permiso |
| Driver | `GET /driver/me`, `GET /driver/shipments`, `PATCH ...` |

### Checklist entrega frontend

**Tienda**

- [ ] Home, catálogo jerárquico, búsqueda, detalle con opciones
- [ ] Carrito guest + merge
- [ ] Checkout + cupón + pago mock + éxito
- [ ] Pedidos, tracking, wishlist, reseñas
- [ ] Cuenta: perfil, direcciones, contraseña

**Admin**

- [ ] Sidebar por permisos
- [ ] CRUD portadas, catálogo, productos, opciones, inventario
- [ ] Pedidos, envíos, conductores, PDF ticket

**Repartidor**

- [ ] Lista envíos + cambiar estado

**Técnico**

- [ ] `npm run build` sin errores
- [ ] FSD: imports solo hacia abajo
- [ ] API apunta a `:5088`

---

## Auth thin pages

```tsx
// src/app/(auth)/login/page.tsx
export { LoginPage as default } from '@/pages/login';
// src/pages/login/ui/login-page.tsx
import { LoginForm } from '@/features/auth/login';
export function LoginPage() {
  return <div className="flex min-h-screen items-center justify-center p-4"><LoginForm /></div>;
}

// src/app/(auth)/register/page.tsx
export { RegisterCustomerPage as default } from '@/pages/register-customer';

// src/app/(auth)/register/driver/page.tsx
export { RegisterDriverPage as default } from '@/pages/register-driver';
```

---

## Errores globales

**`src/app/not-found.tsx`**

```tsx
import Link from 'next/link';
import { Button } from '@/shared/ui/button';

export default function NotFound() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4">
      <h1 className="text-4xl font-bold">404</h1>
      <p>Página no encontrada</p>
      <Button asChild><Link href="/">Volver al inicio</Link></Button>
    </div>
  );
}
```

**`src/app/forbidden.tsx`** — igual con mensaje 403 y link a `/`.

---

**Siguiente paso:** crear carpeta `ecommerce-web` al lado de `ecommerce-api` y seguir este documento archivo por archivo usando [`INVENTARIO-FRONTEND-ARCHIVOS.md`](./INVENTARIO-FRONTEND-ARCHIVOS.md) como checklist.
