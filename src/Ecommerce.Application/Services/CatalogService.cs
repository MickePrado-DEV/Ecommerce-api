using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Application.Mapping;

namespace Ecommerce.Application.Services;

public class CatalogService(ICatalogRepository catalog, IProductRepository products, ICoverRepository covers) : ICatalogService
{
    public async Task<IReadOnlyList<FamilyDto>> GetFamiliesAsync(CancellationToken ct = default)
    {
        var families = await catalog.GetFamiliesTreeAsync(ct);
        return families.Select(f => f.ToDto()).ToList();
    }

    public async Task<CatalogHomeDto> GetHomeAsync(int take, CancellationToken ct = default)
    {
        var coverList = await GetCoversAsync(ct);
        var latest = await GetLatestProductsAsync(take, ct);
        return new CatalogHomeDto(coverList, latest);
    }

    public async Task<IReadOnlyList<CoverDto>> GetCoversAsync(CancellationToken ct = default)
    {
        var items = await covers.ListActiveAsync(ct);
        return items.Select(c => new CoverDto(c.Id, c.Title, c.ImageUrl, c.LinkUrl, c.SortOrder)).ToList();
    }

    public async Task<IReadOnlyList<ProductListItemDto>> GetLatestProductsAsync(int take, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);
        var items = await products.ListLatestAsync(take, ct);
        return items.Select(p => p.ToListItem()).ToList();
    }

    public async Task<FamilyDetailDto?> GetFamilyBySlugAsync(string slug, CancellationToken ct = default)
    {
        var family = await catalog.GetFamilyBySlugAsync(slug, ct);
        return family is null ? null : new FamilyDetailDto(family.Id, family.Name, family.Slug, family.Categories.Select(c => c.ToDto()).ToList());
    }

    public async Task<CategoryDetailDto?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default)
    {
        var category = await catalog.GetCategoryBySlugAsync(slug, ct);
        return category is null ? null : new CategoryDetailDto(
            category.Id, category.FamilyId, category.Name, category.Slug,
            category.Subcategories.Select(s => new SubcategoryDto(s.Id, s.Name, s.Slug)).ToList());
    }

    public async Task<SubcategoryDetailDto?> GetSubcategoryBySlugAsync(string slug, CancellationToken ct = default)
    {
        var sub = await catalog.GetSubcategoryBySlugAsync(slug, ct);
        return sub is null ? null : new SubcategoryDetailDto(sub.Id, sub.CategoryId, sub.Name, sub.Slug);
    }

    public async Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await products.GetBySlugAsync(slug, ct);
        return product?.ToDetail();
    }

    public async Task<PagedResult<ProductListItemDto>> ListProductsAsync(CatalogProductQuery query, CancellationToken ct = default)
    {
        var q = query with
        {
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };
        var result = await products.ListAsync(q, ct);
        return result.ToPaged(q.Page, q.PageSize);
    }
}
