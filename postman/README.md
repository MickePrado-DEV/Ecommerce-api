# Postman - Ecommerce API

Documentación del backend: [`../docs/`](../docs/README.md)

## Qué importar en Postman

Solo estos **2 archivos JSON** (no importes la carpeta `scripts/`):

| Archivo | Descripción |
|---------|-------------|
| `Ecommerce-API.postman_collection.json` | Colección con todos los endpoints |
| `Ecommerce-Local.postman_environment.json` | Variables de entorno local |

Los scripts de login, sesión y variables ya van **dentro** del JSON de la colección (pestañas **Tests** y **Pre-request** en Postman). No hace falta copiar nada manualmente.

### Carpeta `scripts/` (solo para el repo, no para Postman)

| Archivo | Para qué sirve |
|---------|----------------|
| `save-auth-session.js` | Referencia del código de sesión (ya está embebido en la colección) |
| `enrich-collection-descriptions.js` | Herramienta Node para regenerar descripciones en el JSON (`node postman/scripts/enrich-collection-descriptions.js`) |

Puedes ignorar `scripts/` al usar Postman.

## Importar en Postman

1. Abre Postman → **Import**
2. Arrastra **solo** los dos `.json` de arriba (o selecciónalos con Upload)
3. Selecciona el entorno **Ecommerce - Local** (esquina superior derecha)

## Antes de probar

1. Ejecuta la API con perfil **SqlServer** (`http://localhost:5088`)
2. Verifica `GET {{baseUrl}}/health` → `database: connected`

## Usuarios de prueba (seed)

| Rol | Email | Password |
|-----|-------|----------|
| Admin | `admin@ecommerce.local` | `Admin123!` |
| Cliente | `cliente@ecommerce.local` | `Cliente123!` |

Producto demo: slug `audifonos-pro-x` (variable `productSlug`)

## Autenticación automática (sesión)

1. Ejecuta **Login Admin** o **Login Cliente** (carpeta `00 - Setup` o al inicio de los flujos).
2. El script de **Tests** guarda en colección y entorno:
   - `accessToken`, `refreshToken`
   - `userEmail`, `userId`, `sessionRole`, `isLoggedIn`
3. El resto de peticiones heredan **Bearer Token** de la colección (`{{accessToken}}`).
4. Un script **Pre-request** de colección añade `Authorization: Bearer …` si hay token (excepto en peticiones con `noauth`).
5. **Logout** limpia la sesión. **Refresh Token** renueva los tokens.

> Tras un login, el último rol activo queda en `accessToken`. Para probar admin, haz **Login Admin**; para cliente, **Login Cliente**.

## Uso rápido

1. **00 - Setup** → `Login Cliente` o `Login Admin`
2. **Flujo Cliente** → Run folder (compra completa; el paso 1 vuelve a guardar sesión)
3. **Flujo Admin** → Run folder (despacho + PDF; requiere pedido pagado)

Las variables (`variantId`, `orderId`, etc.) se guardan solas con los scripts **Tests**.

## Bodies y descripciones

- Cada **POST/PUT** trae el JSON de ejemplo en la pestaña **Body** (credenciales seed, `{{variantId}}`, dirección de envío, catálogo admin, etc.).
- En la pestaña **Description** (panel derecho de cada request) está documentado cada campo: qué tocar y qué se rellena solo.
- En la mayoría de casos puedes **Send** directo; solo cambia valores si quieres otro escenario (email en Register, `description` del producto, dirección de checkout, nombres/slug en admin).

## Cambiar URL

En el entorno, edita `baseUrl` si la API corre en otro puerto.
