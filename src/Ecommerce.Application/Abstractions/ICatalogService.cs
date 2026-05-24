using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;

namespace Ecommerce.Application.Abstractions;

public interface ICatalogService
{
    Task<IReadOnlyList<FamilyDto>> GetFamiliesAsync(CancellationToken ct = default);
    Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> ListProductsAsync(int page, int pageSize, string? search, CancellationToken ct = default);
}
