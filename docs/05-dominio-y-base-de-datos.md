# Dominio y base de datos

## Jerarquía del catálogo

```
Family (familia)
  └── Category (categoría)
        └── Subcategory (subcategoría)
              └── Product (producto)
                    ├── ProductImage
                    ├── ProductOption → OptionValue
                    └── Variant (SKU, precio opcional)
                          └── Inventory (stock)
```

## Entidades principales

### Usuarios y seguridad

| Tabla | Entidad | Descripción |
|-------|---------|-------------|
| `users` | `User` | Cuenta, password hash, datos personales |
| `roles` | `Role` | Rol (`admin`, `customer`) |
| `permissions` | `Permission` | Código de permiso |
| `user_roles` | `UserRole` | N:M usuario–rol |
| `role_permissions` | `RolePermission` | N:M rol–permiso |
| `refresh_tokens` | `RefreshToken` | Tokens de renovación |
| `addresses` | `Address` | Direcciones guardadas del usuario |

### Tienda

| Tabla | Entidad | Descripción |
|-------|---------|-------------|
| `carts` | `Cart` | `UserId` o `GuestToken` |
| `cart_items` | `CartItem` | Variante + cantidad |
| `orders` | `Order` | Pedido con totales y estado |
| `order_items` | `OrderItem` | Snapshot de línea (nombre, SKU, precios) |
| `order_addresses` | `OrderAddress` | Dirección de envío del pedido |
| `payments` | `Payment` | Monto y estado de pago |
| `stock_reservations` | `StockReservation` | Reserva temporal por pedido |
| `stock_movements` | `StockMovement` | Auditoría de movimientos |

### Inventario

`Inventory` usa `VariantId` como PK:

| Campo | Significado |
|-------|-------------|
| `QuantityOnHand` | Unidades físicas disponibles |
| `QuantityReserved` | Unidades reservadas (checkout pendiente de pago) |

**Disponible para venta** = `QuantityOnHand - QuantityReserved`

### Logística

| Tabla | Entidad | Descripción |
|-------|---------|-------------|
| `drivers` | `Driver` | Repartidor |
| `shipments` | `Shipment` | Envío ligado a pedido |
| `dispatch_tickets` | `DispatchTicket` | Número de ticket para PDF |

## Estados (enums)

### OrderStatus

| Valor | Significado |
|-------|-------------|
| `PendingPayment` | Creado en checkout, espera pago |
| `PaymentFailed` | Pago rechazado (mock no lo usa por defecto) |
| `Paid` | Pagado, stock descontado |
| `ReadyToDispatch` | Listo en almacén |
| `Dispatched` | Enviado |
| `Delivered` | Entregado |
| `Cancelled` | Cancelado |

### PaymentStatus

`Pending` → `Approved` / `Declined` / `Refunded`

### ShipmentStatus

`Pending` → `InTransit` → `Delivered`

### StockMovementType

`In`, `Reservation`, `Sale`, `Return`, `Adjustment`

## EF Core

- **DbContext:** `Infrastructure/Persistence/Sql/EcommerceDbContext.cs`
- **Convención:** nombres de tabla en snake_case plural
- **Enums:** guardados como `string` en SQL
- **Decimales:** `decimal(18,2)` en precios y totales
- **Inicialización:** `EnsureCreated` + `DbSeeder` (no migraciones EF en este proyecto)

## Proveedores de BD

| Provider | Config `Persistence:Provider` | Uso |
|----------|------------------------------|-----|
| SqlServer | `SqlServer` | Desarrollo con LocalDB (perfil por defecto) |
| Sqlite | `Sqlite` | Alternativa sin SQL Server |
| MySql | — | No disponible hasta Pomelo EF10 en NuGet |

## Errores y reglas de dominio (FluentResults)

Además de entidades y excepciones, el dominio expone **errores tipados** para handlers:

| Carpeta | Uso |
|---------|-----|
| `Domain/Auth/AuthErrors.cs` | Credenciales, email duplicado |
| `Domain/Orders/OrderErrors.cs` | Pedido no encontrado, no pagable, stock |
| `Domain/Cart/CartErrors.cs` | Carrito / ítems |
| `Domain/Addresses/AddressErrors.cs` | Direcciones |
| `Domain/Admin/AdminErrors.cs` | Recursos admin, estados inválidos |
| `Domain/Addresses/AddressRules.cs` | Longitudes máximas de campos |

Los handlers devuelven `Result.Fail(...)` con metadata `Code` para mapear a HTTP en la Api.

## Repositorios

| Interfaz | Implementación | Responsabilidad |
|----------|----------------|-----------------|
| `IUserRepository` | `UserRepository` | Auth, permisos, refresh tokens |
| `ICatalogReadRepository` | `CatalogReadRepository` | Lecturas públicas con proyecciones EF |
| `IAddressReadRepository` / `IAddressWriteRepository` | `Address*Repository` | Direcciones (lectura/escritura) |
| `ICartRepository` | `CartRepository` | Carrito guest/usuario |
| `IOrderRepository` / `IOrderReadRepository` | `Order*Repository` | Pedidos (escritura / lectura DTO) |
| `IInventoryRepository` | `InventoryRepository` | Reservas, commit, ajustes |
| `IAdminCatalogRepository` | `AdminCatalogRepository` | CRUD admin catálogo |
| `IShipmentRepository` | `ShipmentRepository` | Envíos y conductores |
| `ICoverRepository` | `CoverRepository` | Portadas admin |
| `IDashboardRepository` | `DashboardRepository` | Estadísticas dashboard |
| `IProductOptionRepository` | `ProductOptionRepository` | Opciones por producto |
| `IUnitOfWork` | `UnitOfWork` | Transacciones y `SaveChanges` |
