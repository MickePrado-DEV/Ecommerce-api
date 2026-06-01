/**
 * Añade/actualiza endpoints en Ecommerce-API.postman_collection.json
 * Ejecutar: node postman/scripts/sync-collection-v2.js
 */
const fs = require('fs');
const path = require('path');

const collectionPath = path.join(__dirname, '..', 'Ecommerce-API.postman_collection.json');
const col = JSON.parse(fs.readFileSync(collectionPath, 'utf8'));

const extraVars = [
  { key: 'addressId', value: '66666666-6666-6666-6666-666666666601' },
  { key: 'coverId', value: '' },
  { key: 'optionId', value: '' },
  { key: 'globalOptionId', value: '' },
  { key: 'roleId', value: '' },
  { key: 'batchId', value: '' },
  { key: 'routeId', value: '' },
  { key: 'stopId', value: '' },
  { key: 'couponCode', value: 'WELCOME10' },
  { key: 'familySlug', value: 'electronica' },
  { key: 'categorySlug', value: 'audio' },
  { key: 'subcategorySlug', value: 'audifonos' },
  { key: 'productSlug', value: 'audifonos-pro-x' }
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

const savePagedFirstId = (path, varName) => ({
  listen: 'test',
  script: {
    type: 'text/javascript',
    exec: [
      'if (pm.response.code !== 200) return;',
      'const j = pm.response.json();',
      `const item = j.items?.[0] || j[0];`,
      `if (item?.${path}) {`,
      `  pm.collectionVariables.set('${varName}', item.${path});`,
      `  try { pm.environment.set('${varName}', item.${path}); } catch(e) {}`,
      '}'
    ]
  }
});

const saveDriversOptionsTest = {
  listen: 'test',
  script: {
    type: 'text/javascript',
    exec: [
      'if (pm.response.code !== 200) return;',
      'const j = pm.response.json();',
      'const list = Array.isArray(j) ? j : (j.items || []);',
      'if (list.length) {',
      "  pm.collectionVariables.set('driverId', list[0].id);",
      "  try { pm.environment.set('driverId', list[0].id); } catch(e) {}",
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
  makeRequest('Listar pedidos (paginado)', 'GET', '{{baseUrl}}/api/v1/admin/orders?page=1&pageSize=20&sortBy=createdAt&sortDirection=desc', {
    description: 'Respuesta paginada estándar.',
    event: [savePagedFirstId('id', 'orderId')]
  }),
  makeRequest('Listar envíos (paginado)', 'GET', '{{baseUrl}}/api/v1/admin/shipments?page=1&pageSize=20&sortBy=createdAt&sortDirection=desc', {
    description: 'Listado paginado con sort.',
    event: [savePagedFirstId('id', 'shipmentId')]
  }),
  {
    name: 'Listar conductores (select)',
    event: [saveDriversOptionsTest],
    request: {
      method: 'GET',
      url: '{{baseUrl}}/api/v1/admin/drivers/options',
      description: 'Lista completa para selects (crear envío).'
    }
  },
  makeRequest('Listar conductores (paginado)', 'GET', '{{baseUrl}}/api/v1/admin/drivers?page=1&pageSize=20&sortBy=name&sortDirection=asc'),
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

// --- 10 Global Options ---
ensureFolder('10 - Admin - Opciones globales', 'Catálogo global Talla/Color/Sexo + asignaciones por producto.');
const globalOpts = findFolder('10 - Admin - Opciones globales');
upsertByName(globalOpts, [
  makeRequest('Listar opciones globales', 'GET', '{{baseUrl}}/api/v1/admin/options'),
  makeRequest('Listar asignaciones producto', 'GET', '{{baseUrl}}/api/v1/admin/products/{{productId}}/option-assignments'),
  makeRequest('Listar variantes producto', 'GET', '{{baseUrl}}/api/v1/admin/products/{{productId}}/variants'),
  makeRequest('Generar variantes', 'POST', '{{baseUrl}}/api/v1/admin/products/{{productId}}/variants/generate')
]);

// --- 11 Users & Roles ---
ensureFolder('11 - Admin - Usuarios y Roles', 'Requiere Login Admin.');
const usersFolder = findFolder('11 - Admin - Usuarios y Roles');
upsertByName(usersFolder, [
  makeRequest('Listar usuarios', 'GET', '{{baseUrl}}/api/v1/admin/users?page=1&pageSize=20&sortBy=createdAt&sortDirection=desc'),
  makeRequest('Obtener usuario', 'GET', '{{baseUrl}}/api/v1/admin/users/{{userId}}'),
  makeRequest('Listar roles', 'GET', '{{baseUrl}}/api/v1/admin/roles', {
    event: [savePagedFirstId('id', 'roleId')]
  }),
  makeRequest('Listar permisos', 'GET', '{{baseUrl}}/api/v1/admin/permissions'),
  makeRequest('Permisos de rol', 'GET', '{{baseUrl}}/api/v1/admin/roles/{{roleId}}')
]);

// --- 12 Dispatch ---
ensureFolder('12 - Admin - Despacho (lotes/rutas)', 'Pedidos ReadyToDispatch + geo en direcciones.');
const dispatchFolder = findFolder('12 - Admin - Despacho (lotes/rutas)');
upsertByName(dispatchFolder, [
  makeRequest('Config despacho', 'GET', '{{baseUrl}}/api/v1/admin/dispatch/settings'),
  makeRequest('Cola despacho', 'GET', '{{baseUrl}}/api/v1/admin/dispatch/queue?page=1&pageSize=20'),
  makeRequest('Listar lotes', 'GET', '{{baseUrl}}/api/v1/admin/dispatch/batches'),
  makeRequest('Listar rutas', 'GET', '{{baseUrl}}/api/v1/admin/dispatch/routes')
]);

// Inventario paginado
const inv = findFolder('07 - Admin - Inventario');
upsertByName(inv, [
  makeRequest('Listar inventario (paginado)', 'GET', '{{baseUrl}}/api/v1/admin/inventory?page=1&pageSize=20&sortBy=sku&sortDirection=asc'),
  makeRequest('Stock por variante', 'GET', '{{baseUrl}}/api/v1/admin/inventory/{{variantId}}', {
    description: 'Detalle inventario de una variante.'
  }),
  makeRequest('Ajustar stock', 'PUT', '{{baseUrl}}/api/v1/admin/inventory/{{variantId}}', {
    body: '{\n  "quantityOnHand": 100\n}'
  })
]);

// Catálogo admin paginado
const adminCatalog = findFolder('06 - Admin - Catálogo CRUD');
upsertByName(adminCatalog, [
  makeRequest('Listar productos (paginado)', 'GET', '{{baseUrl}}/api/v1/admin/catalog/products?page=1&pageSize=20&search=&sortBy=name&sortDirection=asc', {
    event: [savePagedFirstId('id', 'productId')]
  })
]);

// Fix Flujo Admin drivers step
const flujoAdmin = findFolder('Flujo Admin (ejecutar en orden)');
if (flujoAdmin) {
  const driversStep = flujoAdmin.item.find((i) => i.name === '4. Listar conductores');
  if (driversStep) {
    driversStep.event = [saveDriversOptionsTest];
    driversStep.request.method = 'GET';
    driversStep.request.url = '{{baseUrl}}/api/v1/admin/drivers/options';
  }
  const ordersStep = flujoAdmin.item.find((i) => i.name === '2. Listar pedidos pagados');
  if (ordersStep) {
    ordersStep.request.url = '{{baseUrl}}/api/v1/admin/orders?page=1&pageSize=10&status=Paid&sortBy=createdAt&sortDirection=desc';
    ordersStep.event = [savePagedFirstId('id', 'orderId')];
  }
}

// Flujo Completo E2E
ensureFolder('Flujo Completo E2E', 'Ejecutar carpeta completa con Runner. Seed: scriptsSql/seed.sqlserver.sql');
const e2e = findFolder('Flujo Completo E2E');
upsertByName(e2e, [
  makeRequest('0. Ready BD', 'GET', '{{baseUrl}}/ready', { auth: 'noauth' }),
  makeRequest('1. Login Cliente', 'POST', '{{baseUrl}}/api/v1/auth/login', {
    auth: 'noauth',
    body: '{\n  "email": "cliente@ecommerce.local",\n  "password": "Cliente123!"\n}',
    event: [{
      listen: 'test',
      script: { type: 'text/javascript', exec: [
        "if (pm.response.code===200){const j=pm.response.json();['accessToken','refreshToken','customerToken'].forEach(k=>pm.collectionVariables.set(k,j.accessToken));}"
      ]}
    }]
  }),
  makeRequest('2. Detalle producto', 'GET', '{{baseUrl}}/api/v1/catalog/products/{{productSlug}}', {
    auth: 'noauth',
    event: [{ listen: 'test', script: { type: 'text/javascript', exec: [
      "if(pm.response.code===200&&pm.response.json().variants?.length){pm.collectionVariables.set('variantId',pm.response.json().variants[0].id);}"
    ]}}]
  }),
  makeRequest('3. Agregar carrito', 'POST', '{{baseUrl}}/api/v1/cart/items', {
    body: '{\n  "variantId": "{{variantId}}",\n  "quantity": 1\n}'
  }),
  makeRequest('4. Checkout addressId', 'POST', '{{baseUrl}}/api/v1/checkout', {
    body: '{\n  "addressId": "{{addressId}}",\n  "shippingCost": 99.00\n}',
    event: [saveOrderIdTest]
  }),
  makeRequest('5. Pagar pedido', 'POST', '{{baseUrl}}/api/v1/orders/{{orderId}}/pay'),
  makeRequest('6. Login Admin', 'POST', '{{baseUrl}}/api/v1/auth/login', {
    auth: 'noauth',
    body: '{\n  "email": "admin@ecommerce.local",\n  "password": "Admin123!"\n}',
    event: [{ listen: 'test', script: { type: 'text/javascript', exec: [
      "if(pm.response.code===200){pm.collectionVariables.set('accessToken',pm.response.json().accessToken);}"
    ]}}]
  }),
  makeRequest('7. Listo despacho', 'POST', '{{baseUrl}}/api/v1/admin/orders/{{orderId}}/ready'),
  {
    name: '8. Conductores options',
    event: [saveDriversOptionsTest],
    request: { method: 'GET', url: '{{baseUrl}}/api/v1/admin/drivers/options' }
  },
  makeRequest('9. Crear envío', 'POST', '{{baseUrl}}/api/v1/admin/shipments', {
    body: '{\n  "orderId": "{{orderId}}",\n  "driverId": "{{driverId}}",\n  "trackingNumber": "E2E-001"\n}',
    event: [{ listen: 'test', script: { type: 'text/javascript', exec: [
      "if(pm.response.code===200) pm.collectionVariables.set('shipmentId', pm.response.json().id);"
    ]}}]
  }),
  makeRequest('10. Ticket PDF envío', 'GET', '{{baseUrl}}/api/v1/admin/shipments/{{shipmentId}}/ticket.pdf')
]);

// Remove legacy folder name if duplicated
col.item = col.item.filter((i) => i.name !== '10 - Admin - Opciones producto');

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
  '10 - Admin - Opciones globales',
  '11 - Admin - Usuarios y Roles',
  '12 - Admin - Despacho (lotes/rutas)',
  'Flujo Cliente (ejecutar en orden)',
  'Flujo Admin (ejecutar en orden)',
  'Flujo Completo E2E'
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
  'Colección Ecommerce API — flujo tienda + admin + despacho.\n\n' +
  '**Setup:** `scriptsSql/run-all.ps1` → seed E2E → `00 - Setup` → Login.\n\n' +
  '**Runner E2E:** carpeta **Flujo Completo E2E** (compra → pago → despacho → PDF).\n\n' +
  'Usuarios seed:\n' +
  '- admin@ecommerce.local / Admin123!\n' +
  '- cliente@ecommerce.local / Cliente123!\n' +
  '- repartidor@ecommerce.local / Repartidor123!\n\n' +
  'Variables seed: productSlug=audifonos-pro-x, addressId fijo, cupón WELCOME10.';

fs.writeFileSync(collectionPath, JSON.stringify(col, null, 2) + '\n');
console.log('OK:', collectionPath);
