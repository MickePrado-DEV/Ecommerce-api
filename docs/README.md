# Ecommerce API — Documentación del backend

API REST de e-commerce en **.NET 10** con arquitectura en capas. Esta carpeta describe qué hace cada parte del sistema para facilitar revisiones, onboarding y pruebas.

## ¿Empiezas en .NET?

Lee primero la **[Guía para principiantes](./00-guia-para-principiantes.md)** — explica `Program.cs`, inyección de dependencias, Minimal APIs, middleware, `Endpoints/` y el resto del código para quien empieza en .NET.

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

## Recursos relacionados

| Recurso | Ubicación |
|---------|-----------|
| Colección Postman | `../postman/` |
| Scalar (OpenAPI) | `http://localhost:5088/scalar/v1` (no producción) |
| Tests integración | `../tests/Ecommerce.IntegrationTests/` |

## Resumen ejecutivo

- **Tienda (público):** catálogo jerárquico (familias → categorías → subcategorías → productos → variantes).
- **Carrito:** soporta usuario autenticado o invitado (`X-Guest-Token`).
- **Checkout:** crea pedido, reserva stock en transacción.
- **Pago:** mock que confirma stock y marca pedido como pagado.
- **Admin:** CRUD catálogo, inventario, pedidos, envíos y ticket PDF (QuestPDF).
- **Seguridad:** JWT con permisos granulares en claims; políticas por endpoint admin.

## Usuarios de prueba (seed)

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |

Producto demo: slug `audifonos-pro-x`, SKU `APX-001`, stock inicial 50.
