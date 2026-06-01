# Ecommerce API

**Repositorio:** [github.com/MickePrado-DEV/Ecommerce-api](https://github.com/MickePrado-DEV/Ecommerce-api)  
**Frontend:** [github.com/MickePrado-DEV/ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web)

API REST de e-commerce en **.NET 10** para alimentar una **tienda web/mobile**, un **panel de administración** y una **app de repartidor**. El backend está **completo** para el ciclo de compra (catálogo → carrito → checkout → pago simulado → despacho → entrega).

**URL base:** `http://localhost:5088/api/v1`

---

## Inicio rápido — correr el proyecto

### 1. Clonar e instalar

```powershell
git clone https://github.com/MickePrado-DEV/Ecommerce-api.git
cd Ecommerce-api
dotnet restore
dotnet build
```

### 2. Poblar la base de datos

```powershell
cd scriptsSql
.\run-all.ps1
```

### 3. Arrancar la API

```powershell
cd ..\src\Ecommerce.Api
dotnet run --launch-profile SqlServer
```

Comprueba: `GET http://localhost:5088/health` → `{ "status": "ok" }`

### 4. Arrancar el frontend (otro terminal)

```powershell
git clone https://github.com/MickePrado-DEV/ecommerce-web.git
cd ecommerce-web
copy .env.example .env.local
npm install
npm run dev
```

Abre [http://localhost:3000](http://localhost:3000) con la API ya en marcha.

Tutorial completo: [docs/00-guia-para-principiantes.md](docs/00-guia-para-principiantes.md)

---

## Qué incluye el sistema

Una sola API sirve a tres tipos de cliente:

| Cliente | Rol JWT | Capacidades principales |
|---------|---------|-------------------------|
| **Tienda** (web / mobile) | `customer` o invitado | Catálogo, carrito guest, wishlist, reseñas, direcciones, checkout con cupones, pedidos, tracking |
| **Admin** (panel) | `admin` + permisos | Dashboard, portadas, catálogo, opciones de producto, inventario, pedidos, envíos, conductores, PDF |
| **Repartidor** (mobile) | `driver` | Registro/login, envíos asignados, marcar en tránsito / entregado |

### Flujo de negocio cubierto

```
Catálogo → Carrito (guest o usuario) → Checkout → Pago mock → Admin prepara envío
    → Repartidor entrega → Cliente consulta tracking
```

| Área | Detalle |
|------|---------|
| **Catálogo** | Familias, categorías, productos, variantes, opciones (color/talla), filtros, búsqueda |
| **Carrito** | Header `X-Guest-Token` para invitados; `POST /cart/merge` al iniciar sesión |
| **Pagos** | Simulados (`POST /orders/{id}/pay` → referencia `MOCK-…`). Sin pasarela real |
| **Stock** | Reserva en checkout; confirmación al pagar; liberación al cancelar |
| **Promos** | Cupón en checkout (demo: `WELCOME10`) |
| **Seguridad** | JWT (access + refresh), roles en claims, permisos granulares en admin |

---

## Stack tecnológico

| Tecnología | Uso |
|------------|-----|
| .NET 10 | Runtime y SDK |
| Minimal APIs | Endpoints HTTP (`Endpoints/*.cs`) |
| MediatR | CQRS (commands / queries) |
| FluentValidation | Validación en pipeline |
| FluentResults | Errores de negocio → HTTP |
| EF Core | Persistencia (SQL Server o SQLite) |
| Serilog | Logs en consola y archivo |
| Scalar | Documentación OpenAPI interactiva |
| BCrypt | Hash de contraseñas |
| QuestPDF | Tickets PDF (admin) |

---

## Estructura del repositorio

```
ecommerce-api/
├── src/
│   ├── Ecommerce.Api/              # Entrada HTTP, endpoints, middleware
│   │   ├── Endpoints/              # Auth, Catalog, Cart, Checkout, Orders, Admin, Driver, Wishlist
│   │   ├── Extensions/             # JWT helpers, mapeo FluentResults → HTTP
│   │   └── Program.cs              # Pipeline, CORS, bootstrap BD
│   ├── Ecommerce.Application/      # CQRS: Features, DTOs, validadores, abstracciones
│   │   └── Features/               # Auth, Catalog, Cart, Checkout, Orders, Admin, Driver, Wishlist
│   ├── Ecommerce.Domain/           # Entidades, enums, reglas, errores de dominio
│   └── Ecommerce.Infrastructure/   # EF Core, repositorios, JWT, PDF, seed
├── tests/
│   ├── Ecommerce.IntegrationTests/ # Flujos HTTP contra la API
│   └── Ecommerce.UnitTests/
├── docs/                           # Documentación técnica (índice en docs/README.md)
├── postman/                        # Colección + entorno local
├── scriptsSql/                     # Schema + seed por motor (SQL Server, MySQL, PG…)
│   ├── schema.sqlserver.sql
│   ├── seed.sqlserver.sql
│   ├── run-all.ps1
│   └── tools/                      # generate-seed.mjs, build-scripts.mjs
├── Ecommerce.slnx                  # Solución
└── README.md                       # Este archivo
```

### Arquitectura en capas

```
┌─────────────────────────────────────────┐
│  Ecommerce.Api                          │  Minimal APIs, JWT, CORS
└──────────────────┬──────────────────────┘
                   │ ISender (MediatR)
┌──────────────────▼──────────────────────┐
│  Ecommerce.Application                  │  Handlers, DTOs, validación
└──────────────────┬──────────────────────┘
                   │ interfaces (I*Repository)
┌──────────────────▼──────────────────────┐
│  Ecommerce.Infrastructure               │  EF Core, repos, JWT, PDF
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│  Ecommerce.Domain                       │  Entidades y reglas puras
└─────────────────────────────────────────┘
```

Cada petición: **Endpoint → Command/Query → Handler → Repositorio → Result → JSON**.

Más detalle: [docs/01-arquitectura.md](docs/01-arquitectura.md)

---

## Requisitos previos

| Requisito | Notas |
|-----------|--------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | `dotnet --version` debe mostrar 10.x |
| **SQL Server LocalDB** | Perfil por defecto (`SqlServer`) |
| **O SQLite** | Sin instalar SQL Server (`Sqlite`) |
| Postman (opcional) | Para probar la API sin frontend |
| Git | Clonar el repositorio |

---

## Cómo ejecutar el proyecto

### 1. Clonar e ir a la carpeta del API

```powershell
git clone https://github.com/MickePrado-DEV/Ecommerce-api.git
cd Ecommerce-api
```

### 2. Restaurar dependencias

```powershell
dotnet restore
```

### 3. Poblar la base de datos (recomendado)

```powershell
cd scriptsSql
.\run-all.ps1
```

Carga ~1000+ registros por entidad (pedidos, envíos, usuarios, productos…) para probar listados paginados, despacho y admin.

### 4. Arrancar la API

**Opción A — SQL Server LocalDB (recomendado en Windows):**

```powershell
cd src/Ecommerce.Api
dotnet run --launch-profile SqlServer
```

**Opción B — SQLite (sin SQL Server):**

```powershell
cd src/Ecommerce.Api
dotnet run --launch-profile Sqlite
```

| Perfil | URL | Base de datos |
|--------|-----|---------------|
| `SqlServer` | http://localhost:5088 | LocalDB → base `ecommerce` |
| `Sqlite` | http://localhost:5089 | Archivo `ecommerce-dev.db` |

Al iniciar, la API automáticamente:

1. Crea las tablas si no existen (`EnsureCreated`).
2. Ajusta el esquema si detecta una BD antigua.
3. Inserta datos mínimos demo (`DbSeeder`) **solo si la BD está vacía**.

Para datos masivos, usa `scriptsSql/run-all.ps1` (ver arriba).

### 5. Comprobar que responde

```http
GET http://localhost:5088/health
```

```json
{ "status": "ok" }
```

```http
GET http://localhost:5088/ready
```

### 6. Documentación interactiva (desarrollo)

| Recurso | URL |
|---------|-----|
| Scalar (UI) | http://localhost:5088/scalar/v1 |
| OpenAPI JSON | http://localhost:5088/openapi/v1.json |

---

## Usuarios y datos de prueba

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |
| Repartidor | `repartidor@ecommerce.local` | `Repartidor123!` |

| Demo | Valor |
|------|--------|
| Seed masivo | ~2003 usuarios, 1001 productos, 1000 pedidos, 1000 envíos |
| Producto demo | slug `audifonos-pro-x`, variantes `APX-001-BLK`, `APX-001-WHT` |
| Cupón | `WELCOME10` (10 %, subtotal mínimo 50) |
| Regenerar seed | `node scriptsSql/tools/generate-seed.mjs` → `.\run-all.ps1` |

---

## Probar con Postman

1. Importar en Postman:
   - `postman/Ecommerce-API.postman_collection.json`
   - `postman/Ecommerce-Local.postman_environment.json`
2. Seleccionar el entorno **Ecommerce - Local**.
3. Ejecutar **00 - Setup** → `Ready (BD)` y **Login Cliente** o **Login Admin**.
4. Usar **Flujo Cliente**, **Flujo Admin** o **Flujo Completo E2E** (Collection Runner).

Guía: [postman/README.md](postman/README.md)

---

## Ejecutar tests

```powershell
cd ecommerce-api
dotnet test
```

Incluye tests de integración (health, auth, catálogo, checkout, wishlist, cupones) y tests unitarios.

---

## Configuración

Los archivos están en `src/Ecommerce.Api/`:

| Archivo | Contenido |
|---------|-----------|
| `appsettings.json` | Valores base |
| `appsettings.SqlServer.json` | Connection string LocalDB |
| `appsettings.Sqlite.json` | Connection string SQLite |
| `appsettings.Development.json` | Overrides locales |

| Clave | Descripción |
|-------|-------------|
| `Persistence:Provider` | `SqlServer` o `Sqlite` |
| `ConnectionStrings:Default` | Cadena de conexión |
| `Jwt:*` | Issuer, audience, secret, duración de tokens |
| `Cors:Origins` | Orígenes del frontend (ej. `http://localhost:3000`) |

Guía completa: [docs/02-configuracion-y-ejecucion.md](docs/02-configuracion-y-ejecucion.md)

---

## Endpoints principales

Prefijo: `/api/v1`

| Grupo | Rutas | Auth |
|-------|-------|------|
| Auth | `/auth/login`, `/register/customer`, `/register/driver`, `/me`, … | Mixto |
| Catálogo | `/catalog/home`, `/catalog/products`, `/catalog/products/{slug}`, … | Público |
| Carrito | `/cart`, `/cart/items`, `/cart/merge` | Guest o JWT |
| Direcciones | `/addresses` | JWT cliente |
| Checkout | `/checkout` | JWT cliente |
| Pedidos | `/orders`, `/orders/{id}/pay`, `/orders/{id}/tracking` | JWT cliente |
| Wishlist | `/wishlist` | JWT cliente |
| Admin | `/admin/dashboard`, `/admin/catalog/…`, `/admin/orders`, … | JWT admin + permiso |
| Driver | `/driver/me`, `/driver/shipments` | JWT repartidor |

Listado completo: [docs/03-api-endpoints.md](docs/03-api-endpoints.md)

---

## Documentación

| Si quieres… | Lee |
|-------------|-----|
| Tutorial backend (de cero a despacho) | [docs/00-guia-para-principiantes.md](docs/00-guia-para-principiantes.md) |
| Tutorial frontend (Next.js FSD) | [ecommerce-web/docs/00-guia-para-principiantes.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md) |
| Arquitectura y capas | [docs/01-arquitectura.md](docs/01-arquitectura.md) |
| Configurar y ejecutar | [docs/02-configuracion-y-ejecucion.md](docs/02-configuracion-y-ejecucion.md) |
| Todas las rutas | [docs/03-api-endpoints.md](docs/03-api-endpoints.md) |
| JWT, roles y permisos | [docs/04-autenticacion-y-permisos.md](docs/04-autenticacion-y-permisos.md) |
| Entidades y BD | [docs/05-dominio-y-base-de-datos.md](docs/05-dominio-y-base-de-datos.md) |
| Flujos (checkout, stock, envíos) | [docs/06-flujos-de-negocio.md](docs/06-flujos-de-negocio.md) |
| Roadmap y fases | [docs/09-plan-complecion-backend-web-mobile.md](docs/09-plan-complecion-backend-web-mobile.md) |
| Índice general | [docs/README.md](docs/README.md) |

---

## Estado del proyecto

| Completado | Pendiente / opcional |
|------------|----------------------|
| Tienda, admin, repartidor (API) | Pagos reales (Stripe, Niubiz, …) |
| CQRS, JWT, inventario, envíos, PDF | Push, email, upload blob de imágenes |
| Wishlist, reseñas, cupones, opciones | EF Migrations para despliegue productivo |
| Seed masivo SQL + Postman E2E | — |
| Frontend Next.js 16 + FSD | [ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web) |
| Admin con listados paginados server-side | — |

El backend API puede consumirse ya desde cualquier cliente HTTP.

### Frontend (Next.js 16 + FSD)

Repositorio: **[github.com/MickePrado-DEV/ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web)**

```powershell
git clone https://github.com/MickePrado-DEV/ecommerce-web.git
cd ecommerce-web
copy .env.example .env.local
npm install
npm run dev
```

Documentación:

- [Tutorial backend](docs/00-guia-para-principiantes.md)
- [Tutorial frontend](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md)
- [Referencia FSD frontend](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/10-referencia-fsd-completo.md)
- [Inventario archivos frontend](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/INVENTARIO-ARCHIVOS.md)
- [README del frontend](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/README.md)

---

## Licencia y autor

Proyecto personal / portafolio open source. **MickePrado-DEV** — Miguel Angel Prado Garcia
