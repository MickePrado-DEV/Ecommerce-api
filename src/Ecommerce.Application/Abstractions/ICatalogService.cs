using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;

namespace Ecommerce.Application.Abstractions;

public interface ICatalogService
{
    Task<IReadOnlyList<FamilyDto>> GetFamiliesAsync(CancellationToken ct = default);
    Task<CatalogHomeDto> GetHomeAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<CoverDto>> GetCoversAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductListItemDto>> GetLatestProductsAsync(int take, CancellationToken ct = default);
    Task<FamilyDetailDto?> GetFamilyBySlugAsync(string slug, CancellationToken ct = default);
    Task<CategoryDetailDto?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default);
    Task<SubcategoryDetailDto?> GetSubcategoryBySlugAsync(string slug, CancellationToken ct = default);
    Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> ListProductsAsync(CatalogProductQuery query, CancellationToken ct = default);
}
