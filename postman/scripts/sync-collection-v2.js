/**
 * Añade/actualiza endpoints en Ecommerce-API.postman_collection.json
 * Ejecutar: node postman/scripts/sync-collection-v2.js
 */
const fs = require('fs');
const path = require('path');

const collectionPath = path.join(__dirname, '..', 'Ecommerce-API.postman_collection.json');
const col = JSON.parse(fs.readFileSync(collectionPath, 'utf8'));

const extraVars = [
  { key: 'addressId', value: '' },
  { key: 'coverId', value: '' },
  { key: 'optionId', value: '' },
  { key: 'familySlug', value: 'electronica' },
  { key: 'categorySlug', value: 'audio' },
  { key: 'subcategorySlug', value: 'audifonos' }
];
for (const v of extraVars) {
  if (!col.variable.find((x) => x.key === v.key)) col.variable.push(v);
}

function noauth() {
  return { type: 'noauth' };
}

function jsonBody(raw) {
  return { mode: 'raw', raw, options: { raw: { language: 'json' } } };
}

function headersJson() {
  return [{ key: 'Content-Type', value: 'application/json' }];
}

function guestHeader() {
  return [{ key: 'X-Guest-Token', value: '{{guestToken}}' }];
}

function makeRequest(name, method, url, opts = {}) {
  const r = {
    name,
    request: {
      method,
      header: opts.headers || [],
      url,
      description: opts.description || ''
    }
  };
  if (opts.auth === 'noauth') r.request.auth = noauth();
  if (opts.body) {
    r.request.header = [...r.request.header, ...headersJson()];
    r.request.body = jsonBody(opts.body);
  }
  if (opts.event) r.event = opts.event;
  return r;
}

function findFolder(name) {
  return col.item.find((i) => i.name === name);
}

function ensureFolder(name, description) {
  let f = findFolder(name);
  if (!f) {
    f = { name, item: [] };
    if (description) f.description = description;
    col.item.push(f);
  }
  return f;
}

function upsertByName(folder, requests) {
  for (const req of requests) {
    const idx = folder.item.findIndex((i) => i.name === req.name);
    if (idx >= 0) folder.item[idx] = req;
    else folder.item.push(req);
  }
}

const saveOrderIdTest = {
  listen: 'test',
  script: {
    type: 'text/javascript',
    exec: [
      'if (pm.response.code === 200) {',
      '  const j = pm.response.json();',
      "  pm.collectionVariables.set('orderId', j.orderId);",
      "  try { pm.environment.set('orderId', j.orderId); } catch(e) {}",
      '}'
    ]
  }
};

const saveAddressIdTest = {
  listen: 'test',
  script: {
    type: 'text/javascript',
    exec: [
      'if (pm.response.code === 200) {',
      '  const j = pm.response.json();',
      "  pm.collectionVariables.set('addressId', j.id);",
      "  try { pm.environment.set('addressId', j.id); } catch(e) {}",
      '}'
    ]
  }
};

const saveAddressIdListTest = {
  listen: 'test',
  script: {
    type: 'text/javascript',
    exec: [
      'if (pm.response.code === 200) {',
      '  const list = pm.response.json();',
      '  if (Array.isArray(list) && list.length) {',
      "    pm.collectionVariables.set('addressId', list[0].id);",
      "    try { pm.environment.set('addressId', list[0].id); } catch(e) {}",
      '  }',
      '}'
    ]
  }
};

// --- 00 Setup ---
const setup = findFolder('00 - Setup');
upsertByName(setup, [
  makeRequest('Ready (BD)', 'GET', '{{baseUrl}}/ready', {
    auth: 'noauth',
    description: 'Comprueba BD. 200 = ready, 503 = no disponible.'
  })
]);
const healthIdx = setup.item.findIndex((i) => i.name === 'Health');
if (healthIdx >= 0) {
  setup.item[healthIdx].request.description =
    '{ status: "ok" }. Para BD usa **Ready (BD)**.';
}

// --- 02 Catalog ---
const catalog = findFolder('02 - Catálogo (público)');
upsertByName(catalog, [
  makeRequest('Home (covers + latest)', 'GET', '{{baseUrl}}/api/v1/catalog/home?take=12', {
    auth: 'noauth',
    description: 'Portadas + últimos productos.'
  }),
  makeRequest('Listar portadas', 'GET', '{{baseUrl}}/api/v1/catalog/covers', {
    auth: 'noauth',
    description: 'Portadas activas del home.'
  }),
  makeRequest('Últimos productos', 'GET', '{{baseUrl}}/api/v1/catalog/products/latest?take=12', {
    auth: 'noauth',
    description: 'Últimos N productos.'
  }),
  makeRequest('Familia por slug', 'GET', '{{baseUrl}}/api/v1/catalog/families/{{familySlug}}', {
    auth: 'noauth',
    description: 'Slug seed: electronica'
  }),
  makeRequest('Categoría por slug', 'GET', '{{baseUrl}}/api/v1/catalog/categories/{{categorySlug}}', {
    auth: 'noauth',
    description: 'Slug seed: audio'
  }),
  makeRequest('Subcategoría por slug', 'GET', '{{baseUrl}}/api/v1/catalog/subcategories/{{subcategorySlug}}', {
    auth: 'noauth',
    description: 'Slug seed: audifonos'
  }),
  makeRequest('Buscar productos', 'GET', '{{baseUrl}}/api/v1/catalog/search?q=audifono&page=1&pageSize=20', {
    auth: 'noauth',
    description: 'Búsqueda por texto.'
  }),
  makeRequest('Listar productos (filtros)', 'GET', '{{baseUrl}}/api/v1/catalog/products?page=1&pageSize=20&sort=price:desc', {
    auth: 'noauth',
    description: 'Query: familyId, categoryId, subCategoryId, q, sort (price:asc|desc|recent)'
  })
]);

// --- 03 Cart ---
const cart = findFolder('03 - Carrito');
upsertByName(cart, [
  makeRequest('Guest - PATCH cantidad', 'PATCH', '{{baseUrl}}/api/v1/cart/items/{{cartItemId}}', {
    headers: guestHeader(),
    body: '{\n  "quantity": 2\n}',
    description: 'Actualiza cantidad (invitado).'
  }),
  makeRequest('Guest - Vaciar carrito', 'DELETE', '{{baseUrl}}/api/v1/cart/', {
    headers: guestHeader(),
    description: 'Vacía carrito invitado.'
  }),
  makeRequest('Usuario - Merge carrito guest', 'POST', '{{baseUrl}}/api/v1/cart/merge', {
    body: '{\n  "guestToken": "{{guestToken}}"\n}',
    description: 'Fusiona carrito guest tras login. JWT requerido.'
  })
]);

// --- 03b Addresses ---
ensureFolder('03b - Direcciones (cliente)', 'Requiere **Login Cliente**.');
const addresses = findFolder('03b - Direcciones (cliente)');
upsertByName(addresses, [
  {
    name: 'Listar direcciones',
    event: [saveAddressIdListTest],
    request: {
      method: 'GET',
      url: '{{baseUrl}}/api/v1/addresses',
      description: 'Tests guardan addressId del primer ítem.'
    }
  },
  makeRequest('Crear dirección', 'POST', '{{baseUrl}}/api/v1/addresses', {
    body: '{\n  "label": "Casa",\n  "street": "Av. Reforma 123",\n  "city": "Ciudad de México",\n  "state": "CDMX",\n  "postalCode": "06600",\n  "country": "MX",\n  "phone": "5551234567",\n  "isDefault": true\n}',
    event: [saveAddressIdTest],
    description: 'Crea dirección para checkout con addressId.'
  }),
  makeRequest('Actualizar dirección', 'PUT', '{{baseUrl}}/api/v1/addresses/{{addressId}}', {
    body: '{\n  "label": "Casa",\n  "street": "Av. Reforma 456",\n  "city": "Ciudad de México",\n  "state": "CDMX",\n  "postalCode": "06600",\n  "country": "MX",\n  "phone": "5551234567",\n  "isDefault": true\n}'
  }),
  makeRequest('Marcar default', 'PATCH', '{{baseUrl}}/api/v1/addresses/{{addressId}}/default', {
    description: 'Marca predeterminada.'
  }),
  makeRequest('Eliminar dirección', 'DELETE', '{{baseUrl}}/api/v1/addresses/{{addressId}}')
]);

// --- 04 Checkout ---
const checkout = findFolder('04 - Checkout y Pedidos (cliente)');
upsertByName(checkout, [
  makeRequest('Checkout (con addressId)', 'POST', '{{baseUrl}}/api/v1/checkout', {
    body: '{\n  "addressId": "{{addressId}}",\n  "shippingCost": 99.00\n}',
    event: [saveOrderIdTest],
    description: 'Usa dirección guardada. Crear dirección antes.'
  }),
  makeRequest('Pago mock (checkout)', 'POST', '{{baseUrl}}/api/v1/checkout/{{orderId}}/pay', {
    description: 'Alias POST /orders/{id}/pay'
  }),
  makeRequest('Reintentar pago', 'POST', '{{baseUrl}}/api/v1/orders/{{orderId}}/retry-payment', {
    description: 'PaymentFailed o PendingPayment.'
  })
]);

// --- 05 Admin ---
const adminGen = findFolder('05 - Admin - General');
upsertByName(adminGen, [
  makeRequest('Dashboard stats', 'GET', '{{baseUrl}}/api/v1/admin/dashboard/stats', {
    description: 'Métricas: pedidos, productos, usuarios.'
  })
]);

// --- 09 Covers ---
ensureFolder('09 - Admin - Portadas (Covers)', 'Requiere **Login Admin**.');
const coversFolder = findFolder('09 - Admin - Portadas (Covers)');
upsertByName(coversFolder, [
  {
    name: 'Listar portadas',
    event: [{
      listen: 'test',
      script: {
        type: 'text/javascript',
        exec: [
          'if (pm.response.code === 200) {',
          '  const list = pm.response.json();',
          '  if (Array.isArray(list) && list.length) {',
          "    pm.collectionVariables.set('coverId', list[0].id);",
          "    try { pm.environment.set('coverId', list[0].id); } catch(e) {}",
          '  }',
          '}'
        ]
      }
    }],
    request: {
      method: 'GET',
      url: '{{baseUrl}}/api/v1/admin/covers',
      description: 'CRUD portadas del home.'
    }
  },
  makeRequest('Crear portada', 'POST', '{{baseUrl}}/api/v1/admin/covers', {
    body: '{\n  "title": "Promo verano",\n  "imageUrl": "https://placehold.co/1200x400",\n  "linkUrl": "/catalog/products",\n  "sortOrder": 2,\n  "isActive": true\n}',
    event: [{
      listen: 'test',
      script: {
        type: 'text/javascript',
        exec: ["if (pm.response.code === 200) { pm.collectionVariables.set('coverId', pm.response.json().id); }"]
      }
    }]
  }),
  makeRequest('Actualizar portada', 'PUT', '{{baseUrl}}/api/v1/admin/covers/{{coverId}}', {
    body: '{\n  "title": "Promo actualizada",\n  "imageUrl": "https://placehold.co/1200x400",\n  "linkUrl": "/catalog/products",\n  "sortOrder": 1,\n  "isActive": true\n}'
  }),
  makeRequest('Reordenar portadas', 'PATCH', '{{baseUrl}}/api/v1/admin/covers/reorder', {
    body: '{\n  "ids": ["{{coverId}}"]\n}'
  }),
  makeRequest('Eliminar portada', 'DELETE', '{{baseUrl}}/api/v1/admin/covers/{{coverId}}')
]);

// --- 08 Shipments ---
const adminShip = findFolder('08 - Admin - Pedidos y Envíos');
upsertByName(adminShip, [
  makeRequest('Listar envíos', 'GET', '{{baseUrl}}/api/v1/admin/shipments?page=1&pageSize=20', {
    description: 'Listado paginado.'
  }),
  makeRequest('Ticket PDF por pedido', 'GET', '{{baseUrl}}/api/v1/admin/orders/{{orderId}}/ticket', {
    description: 'PDF por orderId.'
  }),
  makeRequest('Marcar envío en tránsito', 'PATCH', '{{baseUrl}}/api/v1/admin/shipments/{{shipmentId}}/in-transit'),
  makeRequest('Marcar envío entregado', 'PATCH', '{{baseUrl}}/api/v1/admin/shipments/{{shipmentId}}/delivered'),
  makeRequest('Eliminar conductor', 'DELETE', '{{baseUrl}}/api/v1/admin/drivers/{{driverId}}'),
  makeRequest('Listo despacho (PATCH)', 'PATCH', '{{baseUrl}}/api/v1/admin/orders/{{orderId}}/ready-to-dispatch', {
    description: 'Paid → ReadyToDispatch'
  })
]);

// --- 10 Options ---
ensureFolder('10 - Admin - Opciones producto', 'Necesita {{productId}} del listado admin.');
const optsFolder = findFolder('10 - Admin - Opciones producto');
upsertByName(optsFolder, [
  makeRequest('Listar opciones', 'GET', '{{baseUrl}}/api/v1/admin/products/{{productId}}/options'),
  makeRequest('Crear opción', 'POST', '{{baseUrl}}/api/v1/admin/products/{{productId}}/options', {
    body: '{\n  "name": "Color",\n  "sortOrder": 1\n}',
    event: [{
      listen: 'test',
      script: {
        type: 'text/javascript',
        exec: ["if (pm.response.code === 200) pm.collectionVariables.set('optionId', pm.response.json().id);"]
      }
    }]
  }),
  makeRequest('Agregar valor opción', 'POST', '{{baseUrl}}/api/v1/admin/products/{{productId}}/options/{{optionId}}/values', {
    body: '{\n  "value": "Negro",\n  "sortOrder": 1\n}'
  })
]);

// Inventario GET by variant
const inv = findFolder('07 - Admin - Inventario');
upsertByName(inv, [
  makeRequest('Stock por variante', 'GET', '{{baseUrl}}/api/v1/admin/inventory/{{variantId}}', {
    description: 'Detalle inventario de una variante.'
  })
]);

// Reorder folders
const order = [
  '00 - Setup',
  '01 - Auth',
  '02 - Catálogo (público)',
  '03 - Carrito',
  '03b - Direcciones (cliente)',
  '04 - Checkout y Pedidos (cliente)',
  '05 - Admin - General',
  '06 - Admin - Catálogo CRUD',
  '07 - Admin - Inventario',
  '08 - Admin - Pedidos y Envíos',
  '09 - Admin - Portadas (Covers)',
  '10 - Admin - Opciones producto',
  'Flujo Cliente (ejecutar en orden)',
  'Flujo Admin (ejecutar en orden)'
];
const sorted = [];
for (const n of order) {
  const f = col.item.find((i) => i.name === n);
  if (f) sorted.push(f);
}
for (const f of col.item) {
  if (!sorted.includes(f)) sorted.push(f);
}
col.item = sorted;

col.info.description =
  'Colección Ecommerce API — paridad ampliada con spec Laravel.\n\n' +
  '**Inicio:** 00 - Setup → Login Admin o Cliente (guarda accessToken).\n\n' +
  '**Tienda:** catálogo home/covers/slugs, carrito guest+merge, direcciones, checkout (addressId o inline).\n\n' +
  '**Admin:** dashboard/stats, covers, envíos, opciones por producto.\n\n' +
  'Seed: admin@ecommerce.local / Admin123! · cliente@ecommerce.local / Cliente123! · slug audifonos-pro-x';

fs.writeFileSync(collectionPath, JSON.stringify(col, null, 2) + '\n');
console.log('OK:', collectionPath);
