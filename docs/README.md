# Ecommerce API — Documentación del backend

API REST de e-commerce en **.NET 10**. Proyecto personal de **MickePrado-DEV** — tienda, panel admin y app repartidor sobre una misma API.

## ¿Empiezas desde cero?

| Guía | Dónde |
|------|-------|
| **[Tutorial backend](./00-guia-para-principiantes.md)** | Este repo — API .NET |
| **[Tutorial frontend](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md)** | Repo [ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web) |

## Índice

| Documento | Contenido |
|-----------|-----------|
| [Tutorial backend / principiantes](./00-guia-para-principiantes.md) | Práctica + arquitectura + inventario API |
| [Arquitectura](./01-arquitectura.md) | Capas, proyectos y dependencias |
| [Configuración y ejecución](./02-configuracion-y-ejecucion.md) | Perfiles, BD, scriptsSql |
| [Endpoints API](./03-api-endpoints.md) | Rutas, métodos, auth |
| [Autenticación y permisos](./04-autenticacion-y-permisos.md) | JWT, roles, permisos admin |
| [Dominio y base de datos](./05-dominio-y-base-de-datos.md) | Entidades, scripts SQL |
| [Flujos de negocio](./06-flujos-de-negocio.md) | Checkout, despacho, stock |

## Recursos relacionados

| Recurso | Ubicación |
|---------|-----------|
| **Guía frontend** | [ecommerce-web/docs/00-guia-para-principiantes.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/00-guia-para-principiantes.md) |
| **Docs frontend (índice)** | [ecommerce-web/docs/README.md](https://github.com/MickePrado-DEV/ecommerce-web/blob/master/docs/README.md) |
| Scripts SQL | `../scriptsSql/` |
| Postman | `../postman/` |
| Frontend | [github.com/MickePrado-DEV/ecommerce-web](https://github.com/MickePrado-DEV/ecommerce-web) |

## Usuarios de prueba (seed)

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |
| Repartidor | `repartidor@ecommerce.local` | `Repartidor123!` |

Poblar BD: `cd scriptsSql; .\run-all.ps1`
