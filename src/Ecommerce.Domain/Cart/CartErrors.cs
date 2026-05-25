using FluentResults;

namespace Ecommerce.Domain.Cart;

public static class CartErrors
{
    public const string NotFoundCode = "NotFound";
    public const string ValidationCode = "Validation";
    public const string EmptyCartCode = "Validation";

    public static Error VariantNotFound(Guid variantId) =>
        new Error($"Variante {variantId} no encontrada").WithMetadata("Code", NotFoundCode);

    public static Error CartItemNotFound(Guid itemId) =>
        new Error($"Ítem de carrito {itemId} no encontrado").WithMetadata("Code", NotFoundCode);

    public static Error InvalidQuantity() =>
        new Error("La cantidad debe ser mayor a cero").WithMetadata("Code", ValidationCode);

    public static Error EmptyCart() =>
        new Error("El carrito está vacío").WithMetadata("Code", EmptyCartCode);
}
