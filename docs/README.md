# Ecommerce API — Documentación del backend

API REST de e-commerce en **.NET 10** con arquitectura en capas. Esta carpeta describe qué hace cada parte del sistema para facilitar revisiones, onboarding y pruebas.

## ¿Empiezas en .NET?

Lee primero la **[Guía para principiantes](./00-guia-para-principiantes.md)** — explica `Program.cs`, Minimal APIs, **CQRS con MediatR**, **FluentResults**, inyección de dependencias, `Features/`, validación en pipeline, middleware y flujos completos (login, compra, admin). Pensada para quien empieza en .NET.

## Índice

| Documento | Contenido |
|-----------|-----------|
| [Guía para principiantes](./00-guia-para-principiantes.md) | Explicación del código para quien empieza en .NET |
| [Arquitectura](./01-arquitectura.md) | Capas, proyectos y dependencias |
| [Configuración y ejecución](./02-configuracion-y-ejecucion.md) | Perfiles, BD, variables, cómo arrancar |
| [Endpoints API](./03-api-endpoints.md) | Rutas, métodos, auth y ejemplos |
| [Autenticación y permisos](./04-autenticacion-y-permisos.md) | JWT, roles, claims, admin 403 |
| [Dominio y base de datos](./05-dominio-y-base-de-datos.md) | Entidades, relaciones, inventario |
| [Flujos de negocio](./06-flujos-de-negocio.md) | Checkout, pago, despacho, stock |
| [Comparativa rutas Laravel](./07-comparativa-rutas-laravel.md) | Qué falta vs web/admin Laravel original |
| [Evolución CQRS y dominio](./08-evolucion-cqrs-y-dominio.md) | CQRS implementado, plan de migración y mejoras futuras |
| [Plan completitud web + mobile](./09-plan-complecion-backend-web-mobile.md) | Roadmap por fases: tienda, admin, app repartidor |

## Recursos relacionados

| Recurso | Ubicación |
|---------|-----------|
| Colección Postman | `../postman/` |
| Scalar (OpenAPI) | `http://localhost:5088/scalar/v1` (no producción) |
| Tests integración | `../tests/Ecommerce.IntegrationTests/` |

## Resumen ejecutivo

- **Tienda (público):** home (portadas + novedades), catálogo por slug, búsqueda y filtros.
- **Carrito:** invitado (`X-Guest-Token`), merge al login, vaciar y PATCH cantidad.
- **Direcciones:** CRUD del cliente; checkout con `addressId` o dirección inline.
- **Checkout:** crea pedido, reserva stock; pago mock y reintento de pago.
- **Admin:** dashboard stats, covers, catálogo, opciones por producto, inventario, pedidos, envíos (in-transit/delivered), PDF.
- **Arquitectura:** CQRS (MediatR), FluentResults, handlers en `Application/Features/`.
- **Seguridad:** JWT con permisos granulares en claims; políticas por endpoint admin.
- **Pruebas:** colección Postman en `postman/` alineada con `03-api-endpoints.md`.

## Usuarios de prueba (seed)

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |
| Repartidor | `repartidor@ecommerce.local` | `Repartidor123!` |

Producto demo: slug `audifonos-pro-x`, SKU `APX-001`, stock inicial 50.
