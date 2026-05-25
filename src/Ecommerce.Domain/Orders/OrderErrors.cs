using FluentResults;

namespace Ecommerce.Domain.Orders;

public static class OrderErrors
{
    public const string NotFoundCode = "NotFound";
    public const string InvalidStateCode = "Validation";
    public const string InsufficientStockCode = "InsufficientStock";

    public static Error NotFound(Guid orderId) =>
        new Error($"Pedido {orderId} no encontrado").WithMetadata("Code", NotFoundCode);

    public static Error NotPayable() =>
        new Error("La orden no admite pago").WithMetadata("Code", InvalidStateCode);

    public static Error InsufficientStock(Guid variantId) =>
        new Error($"Stock insuficiente para la variante {variantId}").WithMetadata("Code", InsufficientStockCode);

    public static Error MissingShippingAddress() =>
        new Error("Debe indicar addressId o los datos de envío").WithMetadata("Code", InvalidStateCode);

    public static Error AddressNotFound(Guid addressId) =>
        new Error($"Dirección {addressId} no encontrada").WithMetadata("Code", NotFoundCode);
}
