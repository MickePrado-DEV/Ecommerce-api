using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface ICatalogReadRepository
{
    Task<IReadOnlyList<FamilyDto>> GetFamiliesTreeAsync(CancellationToken ct = default);
    Task<FamilyDetailDto?> GetFamilyBySlugAsync(string slug, CancellationToken ct = default);
    Task<CategoryDetailDto?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default);
    Task<SubcategoryDetailDto?> GetSubcategoryBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<CoverDto>> GetActiveCoversAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductListItemDto>> GetLatestProductsAsync(int take, CancellationToken ct = default);
    Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default);
    Task<ResolvedVariantDto?> ResolveVariantAsync(string slug, IReadOnlyList<Guid> optionValueIds, CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> ListProductsAsync(CatalogProductQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogOptionDto>> GetFilterOptionsAsync(
        Guid? familyId,
        Guid? categoryId,
        Guid? subCategoryId,
        CancellationToken ct = default);
}
