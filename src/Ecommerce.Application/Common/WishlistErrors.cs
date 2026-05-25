using FluentResults;

namespace Ecommerce.Application.Common;

public static class WishlistErrors
{
    public static Error ProductNotFound(Guid productId) =>
        new Error($"Producto {productId} no encontrado").WithMetadata("Code", "Wishlist.ProductNotFound");

    public static Error AlreadyInWishlist(Guid productId) =>
        new Error($"El producto {productId} ya está en favoritos").WithMetadata("Code", "Wishlist.AlreadyExists");

    public static Error NotInWishlist(Guid productId) =>
        new Error($"El producto {productId} no está en favoritos").WithMetadata("Code", "Wishlist.NotFound");
}
