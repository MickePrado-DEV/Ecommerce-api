using FluentResults;

namespace Ecommerce.Application.Common;

public static class CatalogErrors
{
    public const string NotFoundCode = "Catalog.NotFound";

    public static Error ProductNotFound(string slug) =>
        new Error($"Producto '{slug}' no encontrado").WithMetadata("Code", NotFoundCode);

    public static Error FamilyNotFound(string slug) =>
        new Error($"Familia '{slug}' no encontrada").WithMetadata("Code", NotFoundCode);

    public static Error CategoryNotFound(string slug) =>
        new Error($"Categoría '{slug}' no encontrada").WithMetadata("Code", NotFoundCode);

    public static Error SubcategoryNotFound(string slug) =>
        new Error($"Subcategoría '{slug}' no encontrada").WithMetadata("Code", NotFoundCode);
}
