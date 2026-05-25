using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Application.Mapping;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class CatalogReadRepository(EcommerceDbContext db) : ICatalogReadRepository
{
    public async Task<IReadOnlyList<FamilyDto>> GetFamiliesTreeAsync(CancellationToken ct = default)
    {
        var list = await db.Families.AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .Select(f => new FamilyDto(
                f.Id,
                f.Name,
                f.Slug,
                f.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new CategoryDto(
                        c.Id,
                        c.Name,
                        c.Slug,
                        c.Subcategories
                            .Where(s => s.IsActive)
                            .OrderBy(s => s.SortOrder)
                            .Select(s => new SubcategoryDto(s.Id, s.Name, s.Slug))
                            .ToList()))
                    .ToList()))
            .ToListAsync(ct);
        return list;
    }

    public Task<FamilyDetailDto?> GetFamilyBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Families.AsNoTracking()
            .Where(f => f.IsActive && f.Slug == slug)
            .Select(f => new FamilyDetailDto(
                f.Id,
                f.Name,
                f.Slug,
                f.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new CategoryDto(
                        c.Id,
                        c.Name,
                        c.Slug,
                        c.Subcategories
                            .Where(s => s.IsActive)
                            .OrderBy(s => s.SortOrder)
                            .Select(s => new SubcategoryDto(s.Id, s.Name, s.Slug))
                            .ToList()))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

    public Task<CategoryDetailDto?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Categories.AsNoTracking()
            .Where(c => c.IsActive && c.Slug == slug)
            .Select(c => new CategoryDetailDto(
                c.Id,
                c.FamilyId,
                c.Name,
                c.Slug,
                c.Subcategories
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new SubcategoryDto(s.Id, s.Name, s.Slug))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

    public Task<SubcategoryDetailDto?> GetSubcategoryBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Subcategories.AsNoTracking()
            .Where(s => s.IsActive && s.Slug == slug)
            .Select(s => new SubcategoryDetailDto(s.Id, s.CategoryId, s.Name, s.Slug))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CoverDto>> GetActiveCoversAsync(CancellationToken ct = default)
    {
        var list = await db.Covers.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CoverDto(c.Id, c.Title, c.ImageUrl, c.LinkUrl, c.SortOrder))
            .ToListAsync(ct);
        return list;
    }

    public async Task<IReadOnlyList<ProductListItemDto>> GetLatestProductsAsync(int take, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);
        return await db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .Select(ProjectListItem())
            .ToListAsync(ct);
    }

    public async Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        // Include + map: proyección anidada no traduce bien en todos los proveedores (p. ej. SQLite en tests).
        var product = await db.Products.AsNoTracking()
            .Include(p => p.Variants).ThenInclude(v => v.Inventory)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, ct);
        return product?.ToDetail();
    }

    public async Task<PagedResult<ProductListItemDto>> ListProductsAsync(CatalogProductQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = db.Products.AsNoTracking().Where(p => p.IsActive);

        if (query.SubcategoryId.HasValue)
            q = q.Where(p => p.SubcategoryId == query.SubcategoryId);
        else if (query.CategoryId.HasValue)
            q = q.Where(p => p.Subcategory.CategoryId == query.CategoryId);
        else if (query.FamilyId.HasValue)
            q = q.Where(p => p.Subcategory.Category.FamilyId == query.FamilyId);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(p => p.Name.Contains(query.Search) || (p.Description != null && p.Description.Contains(query.Search)));

        q = ApplySort(q, query.Sort);

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProjectListItem())
            .ToListAsync(ct);

        return new PagedResult<ProductListItemDto>(items, page, pageSize, total);
    }

    private static System.Linq.Expressions.Expression<Func<Product, ProductListItemDto>> ProjectListItem() =>
        p => new ProductListItemDto(
            p.Id,
            p.Name,
            p.Slug,
            p.BasePrice,
            p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()
                ?? p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault());

    private static IQueryable<Product> ApplySort(IQueryable<Product> q, string? sort) => sort?.ToLowerInvariant() switch
    {
        "price:desc" or "2" => q.OrderByDescending(p => p.BasePrice),
        "price:asc" or "3" => q.OrderBy(p => p.BasePrice),
        "recent" or "1" => q.OrderByDescending(p => p.CreatedAt),
        _ => q.OrderBy(p => p.Name)
    };
}
