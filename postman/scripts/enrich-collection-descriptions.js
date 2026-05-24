/**
 * Añade description y options JSON a requests de la colección Postman.
 * Ejecutar: node postman/scripts/enrich-collection-descriptions.js
 */
const fs = require('fs');
const path = require('path');

const collectionPath = path.join(__dirname, '..', 'Ecommerce-API.postman_collection.json');
const collection = JSON.parse(fs.readFileSync(collectionPath, 'utf8'));

const descriptions = {
  'Health': 'Comprueba que la API responde y la BD está conectada. Sin body.',
  'Login Admin': '**Body (listo para enviar)**\n- `email`, `password`: credenciales seed admin.\n\nTras 200, Tests guardan sesión (`accessToken`). Solo cambia email/password si pruebas otro usuario.',
  'Login Cliente': '**Body (listo para enviar)**\n- `email`, `password`: credenciales seed cliente.\n\nTras 200, Tests guardan sesión. Ejecuta antes de carrito/checkout.',
  'Register': '**Body ejemplo**\n- `email`: único (cambia si ya existe)\n- `password`: mín. 8 caracteres\n- `firstName`, `lastName`\n\nGuarda tokens en sesión al registrar.',
  'Refresh Token': '**Body**\n- `refreshToken`: se rellena solo con `{{refreshToken}}` tras login.\n\nRenueva `accessToken` sin volver a loguear.',
  'Me (usa sesión guardada)': 'Devuelve el usuario del JWT actual. Requiere login previo.',
  'Logout': 'Invalida refresh token y limpia variables de sesión en Tests.',
  'Listar familias': 'Catálogo público. Sin body.',
  'Listar productos': 'Query: `page`, `pageSize`, `q` (búsqueda opcional).',
  'Detalle producto por slug': 'Usa `{{productSlug}}`. Tests guardan `variantId` del primer variant.',
  'Guest - Obtener carrito': 'Header `X-Guest-Token`: opcional; si vacío crea carrito invitado. Tests guardan `guestToken`.',
  'Guest - Agregar item': '**Body**\n- `variantId`: `{{variantId}}` (ejecuta antes Detalle producto)\n- `quantity`: cantidad\n\nHeader `X-Guest-Token` opcional.',
  'Guest - Actualizar cantidad': '**Body**: `{ "quantity": 2 }`. URL usa `{{cartItemId}}`.',
  'Guest - Eliminar item': 'DELETE con `{{cartItemId}}` y header guest.',
  'Usuario - Agregar item (con JWT cliente)': '**Body**: `variantId`, `quantity`. Requiere **Login Cliente**.',
  'Usuario - Ver carrito': 'Carrito del usuario autenticado (sin guest token).',
  'Checkout': '**Body – dirección de envío**\n- `fullName`, `street`, `city`, `state`, `postalCode`, `country`, `phone`\n- `shippingCost`: decimal\n\nSolo cambia textos/dirección si quieres otro escenario. Tests guardan `orderId`.',
  'Pago mock': 'Simula pago del pedido `{{orderId}}`. Sin body. Requiere checkout previo.',
  'Mis pedidos': 'Lista pedidos del cliente autenticado.',
  'Detalle pedido': 'GET con `{{orderId}}`.',
  'Dashboard (requiere permiso)': 'Requiere **Login Admin** y permiso dashboard.',
  'Dashboard sin permiso (tras Login Cliente → 403)': 'Ejecuta **Login Cliente** antes. Debe responder 403.',
  'Crear familia': '**Body**\n- `name`, `slug`, `sortOrder`, `isActive`\n\nSolo cambia nombre/slug para otro registro.',
  'Actualizar familia': '**Body**: mismos campos que crear. URL `{{familyId}}`.',
  'Crear categoría': '**Body**\n- `familyId`: `{{familyId}}`\n- `name`, `slug`, `sortOrder`, `isActive`',
  'Crear subcategoría': '**Body**\n- `categoryId`: `{{categoryId}}`\n- `name`, `slug`, `sortOrder`, `isActive`',
  'Listar productos (admin)': 'Query `page`, `pageSize`.',
  'Crear producto': '**Body**\n- `subcategoryId`, `name`, `slug`\n- `description`: texto del producto (cámbialo aquí)\n- `basePrice`, `isActive`',
  'Crear variante': '**Body**\n- `productId`, `sku`, `price`, `isActive`, `initialStock`',
  'Eliminar variante': 'DELETE `{{variantId}}`.',
  'Eliminar producto': 'DELETE `{{productId}}`.',
  'Eliminar subcategoría': 'DELETE `{{subcategoryId}}`.',
  'Eliminar categoría': 'DELETE `{{categoryId}}`.',
  'Eliminar familia': 'DELETE `{{familyId}}`.',
  'Listar inventario': 'Stock de todas las variantes.',
  'Ajustar stock variante': '**Body**: `{ "quantityOnHand": 75 }`. URL `{{variantId}}`.',
  'Listar pedidos': 'Query `page`, `pageSize`, `status` (ej. Paid). Tests guardan `orderId`.',
  'Detalle pedido (admin)': 'GET admin con `{{orderId}}`.',
  'Marcar listo para despacho': 'POST sin body. Pedido debe estar Paid.',
  'Listar conductores': 'Tests guardan `driverId` del primero.',
  'Crear conductor': '**Body**: `name`, `phone`, `isActive`.',
  'Crear envío': '**Body**\n- `orderId`, `driverId`\n- `trackingNumber` opcional',
  'Descargar ticket PDF': 'Descarga PDF del envío `{{shipmentId}}`.',
  '1. Login Cliente': 'Igual que Login Cliente en Setup. Guarda sesión.',
  '2. Detalle producto (guarda variantId)': 'Público. Rellena `variantId` en Tests.',
  '3. Agregar al carrito': '**Body**: variantId + quantity. Usa sesión cliente.',
  '4. Checkout': '**Body**: dirección de envío (ejemplo listo). Cambia solo datos de envío si quieres.',
  '5. Pago mock': 'Paga `{{orderId}}` del paso anterior.',
  '6. Mis pedidos': 'Lista pedidos del cliente.',
  '1. Login Admin': 'Guarda sesión admin.',
  '2. Listar pedidos pagados': 'Filtra status=Paid. Guarda orderId.',
  '3. Marcar listo para despacho': 'Sin body.',
  '4. Listar conductores': 'Guarda driverId.',
  '5. Crear envío': '**Body**: orderId, driverId, trackingNumber.',
  '6. Ticket PDF': 'Descarga ticket del envío creado.'
};

const descriptionsByPath = {
  '02 - Catálogo (público)/Listar familias': 'Catálogo público. Sin body.',
  '02 - Catálogo (público)/Listar productos': descriptions['Listar productos'],
  '06 - Admin - Catálogo CRUD/Listar familias': 'Admin: lista familias. Tests guardan `familyId`.',
  '06 - Admin - Catálogo CRUD/Listar productos (admin)': descriptions['Listar productos (admin)']
};

function walk(items, folderPath = '') {
  for (const entry of items) {
    const currentPath = folderPath ? `${folderPath}/${entry.name}` : entry.name;
    if (entry.item) {
      walk(entry.item, currentPath);
      continue;
    }
    if (!entry.request) continue;

    const key = descriptionsByPath[currentPath] ? currentPath : entry.name;
    const desc = descriptionsByPath[currentPath] || descriptions[entry.name];
    if (desc) entry.request.description = desc;

    const body = entry.request.body;
    if (body?.mode === 'raw') {
      body.options = { raw: { language: 'json' } };
    }
  }
}

walk(collection.item);

collection.info.description +=
  '\n\n## Bodies de ejemplo\nCada POST/PUT trae JSON listo con valores seed o variables (`{{variantId}}`, etc.). Abre la pestaña **Body** y solo modifica los campos que quieras probar (nombre, slug, **description** del producto, dirección, etc.). La pestaña **Description** de cada request documenta cada parámetro.';

fs.writeFileSync(collectionPath, JSON.stringify(collection, null, 2) + '\n', 'utf8');
console.log('Colección actualizada:', collectionPath);
