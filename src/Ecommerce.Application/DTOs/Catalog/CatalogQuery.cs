namespace Ecommerce.Application.DTOs.Catalog;

public record CatalogProductQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? FamilyId = null,
    Guid? CategoryId = null,
    Guid? SubcategoryId = null,
    string? Search = null,
    string? Sort = null,
    IReadOnlyList<Guid>? OptionValueIds = null);

public record CatalogHomeDto(
    IReadOnlyList<CoverDto> Covers,
    IReadOnlyList<ProductListItemDto> LatestProducts);

public record CoverDto(Guid Id, string Title, string ImageUrl, string? LinkUrl, int SortOrder);

public record FamilyDetailDto(Guid Id, string Name, string Slug, IReadOnlyList<CategoryDto> Categories);

public record CategoryDetailDto(Guid Id, Guid FamilyId, string Name, string Slug, IReadOnlyList<SubcategoryDto> Subcategories);

public record SubcategoryDetailDto(Guid Id, Guid CategoryId, string Name, string Slug);
