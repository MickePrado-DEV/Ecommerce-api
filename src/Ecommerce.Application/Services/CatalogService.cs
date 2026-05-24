using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Application.Mapping;

namespace Ecommerce.Application.Services;

public class CatalogService(ICatalogRepository catalog, IProductRepository products) : ICatalogService
{
    public async Task<IReadOnlyList<FamilyDto>> GetFamiliesAsync(CancellationToken ct = default)
    {
        var families = await catalog.GetFamiliesTreeAsync(ct);
        return families.Select(f => f.ToDto()).ToList();
    }

    public async Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await products.GetBySlugAsync(slug, ct);
        return product?.ToDetail();
    }

    public async Task<PagedResult<ProductListItemDto>> ListProductsAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await products.ListAsync(page, pageSize, search, ct);
        return result.ToPaged(page, pageSize);
    }
}
