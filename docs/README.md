# Ecommerce API — Documentación del backend

API REST de e-commerce en **.NET 10**. Proyecto personal de **MickePrado-DEV** — tienda, panel admin y app repartidor sobre una misma API.

## ¿Empiezas desde cero?

| Guía | Para quién |
|------|------------|
| **[Tutorial backend](./00-guia-para-principiantes.md)** | API .NET: seed → Postman → código carpeta por carpeta |
| **[Tutorial frontend](./11-guia-frontend-principiantes.md)** | Next.js + FSD: setup → login → admin → rutas |

## Índice

| Documento | Contenido |
|-----------|-----------|
| [Tutorial backend / principiantes](./00-guia-para-principiantes.md) | Práctica + arquitectura + inventario archivo por archivo |
| [Tutorial frontend / principiantes](./11-guia-frontend-principiantes.md) | Next.js FSD de cero a código |
| [Arquitectura](./01-arquitectura.md) | Capas, proyectos y dependencias |
| [Configuración y ejecución](./02-configuracion-y-ejecucion.md) | Perfiles, BD, scriptsSql, cómo arrancar |
| [Endpoints API](./03-api-endpoints.md) | Rutas, métodos, auth y ejemplos |
| [Autenticación y permisos](./04-autenticacion-y-permisos.md) | JWT, roles, claims, admin 403 |
| [Dominio y base de datos](./05-dominio-y-base-de-datos.md) | Entidades, relaciones, scripts SQL |
| [Flujos de negocio](./06-flujos-de-negocio.md) | Checkout, pago, despacho, stock |
| [Comparativa rutas Laravel](./07-comparativa-rutas-laravel.md) | Qué falta vs web/admin Laravel original |
| [Evolución CQRS y dominio](./08-evolucion-cqrs-y-dominio.md) | CQRS implementado, plan de migración |
| [Plan completitud web + mobile](./09-plan-complecion-backend-web-mobile.md) | Roadmap por fases |
| [Frontend Next.js FSD (referencia)](./10-frontend-nextjs-fsd-completo.md) | Pantallas, componentes, API cliente |
| [Inventario archivos frontend](./INVENTARIO-FRONTEND-ARCHIVOS.md) | Listado del proyecto `ecommerce-web` |

## Recursos relacionados

| Recurso | Ubicación |
|---------|-----------|
| Scripts SQL (schema + seed) | `../scriptsSql/` — [README](../scriptsSql/README.md) |
| Colección Postman | `../postman/` |
| Frontend Next.js | [github.com/MickePrado-DEV/ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web) |
| Scalar (OpenAPI) | `http://localhost:5088/scalar/v1` (solo desarrollo) |
| Tests integración | `../tests/Ecommerce.IntegrationTests/` |

## Resumen ejecutivo

**Estado:** backend API **completo** + frontend Next.js en [ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web).

- **Tienda:** catálogo, carrito guest, checkout, cupones, wishlist, reseñas.
- **Admin:** dashboard, catálogo, inventario paginado, pedidos, envíos, despacho, PDF, usuarios/roles.
- **Repartidor:** envíos asignados, in-transit, delivered.
- **Arquitectura:** CQRS (MediatR), FluentResults, Clean Architecture.
- **Datos:** seed masivo ~1000+ registros vía `scriptsSql/seed.sqlserver.sql`.

## Usuarios de prueba (seed)

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |
| Repartidor | `repartidor@ecommerce.local` | `Repartidor123!` |

| Dato demo | Valor |
|-----------|--------|
| Producto | slug `audifonos-pro-x` |
| Cupón | `WELCOME10` |

Poblar la BD: `cd scriptsSql; .\run-all.ps1`
