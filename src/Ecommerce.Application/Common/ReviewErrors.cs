using FluentResults;

namespace Ecommerce.Application.Common;

public static class ReviewErrors
{
    public static Error ProductNotFound(string slug) =>
        new Error($"Producto '{slug}' no encontrado").WithMetadata("Code", "Review.ProductNotFound");

    public static Error AlreadyReviewed() =>
        new Error("Ya has publicado una reseña para este producto").WithMetadata("Code", "Review.AlreadyExists");
}
