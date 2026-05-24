namespace Ecommerce.Application.DTOs.Catalog;

public record FamilyDto(Guid Id, string Name, string Slug, IReadOnlyList<CategoryDto> Categories);
public record CategoryDto(Guid Id, string Name, string Slug, IReadOnlyList<SubcategoryDto> Subcategories);
public record SubcategoryDto(Guid Id, string Name, string Slug);
public record ProductListItemDto(Guid Id, string Name, string Slug, decimal Price, string? PrimaryImage);
public record ProductDetailDto(Guid Id, string Name, string Slug, string? Description, decimal BasePrice, IReadOnlyList<ProductVariantDto> Variants, IReadOnlyList<string> Images);
public record ProductVariantDto(Guid Id, string Sku, decimal Price, int Available);
