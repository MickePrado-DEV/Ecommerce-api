using System.Linq.Expressions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Infrastructure.Persistence.Sql.Common;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class AdminCatalogRepository(EcommerceDbContext db) : IAdminCatalogRepository
{
    public Task<Family?> GetFamilyAsync(Guid id, CancellationToken ct = default) =>
        db.Families.FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<List<Family>> ListFamiliesAsync(CancellationToken ct = default) =>
        db.Families.OrderBy(f => f.SortOrder).ToListAsync(ct);

    public async Task<(List<Family> Items, int Total)> ListFamiliesPagedAsync(
        AdminTableQueryParams query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        var q = db.Families.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(f => f.Name.Contains(s) || f.Slug.Contains(s));
        }

        if (query.NameInitials is { Length: > 0 } initials)
        {
            var letters = initials
                .Select(x => x.Trim().ToUpperInvariant())
                .Where(x => x.Length > 0)
                .Select(x => x[0])
                .ToList();
            q = q.Where(f => letters.Any(ch => f.Name.ToUpper().StartsWith(ch.ToString())));
        }

        if (query.IdBuckets is { Length: > 0 } buckets)
        {
            var ranges = ParseIdBuckets(buckets);
            if (ranges.Count > 0)
                q = ApplySortOrderRanges(q, ranges);
        }

        q = (query.SortKey?.ToLowerInvariant()) switch
        {
            "name" => desc ? q.OrderByDescending(f => f.Name) : q.OrderBy(f => f.Name),
            "id" => desc ? q.OrderByDescending(f => f.Id) : q.OrderBy(f => f.Id),
            _ => desc ? q.OrderByDescending(f => f.SortOrder) : q.OrderBy(f => f.SortOrder),
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(List<CategoryAdminRowDto> Items, int Total)> ListCategoriesPagedAsync(
        AdminTableQueryParams query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        var q = db.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(c => c.Name.Contains(s) || c.Slug.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(query.FamilyName))
        {
            var f = query.FamilyName.Trim();
            q = q.Where(c => c.Family.Name.Contains(f));
        }

        if (query.FamilyIds is { Length: > 0 } familyIds)
            q = q.Where(c => familyIds.Contains(c.FamilyId));

        q = (query.SortKey?.ToLowerInvariant()) switch
        {
            "name" => desc ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
            "familyname" => desc ? q.OrderByDescending(c => c.Family.Name) : q.OrderBy(c => c.Family.Name),
            "id" => desc ? q.OrderByDescending(c => c.Id) : q.OrderBy(c => c.Id),
            _ => desc ? q.OrderByDescending(c => c.SortOrder) : q.OrderBy(c => c.SortOrder),
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryAdminRowDto(
                c.Id,
                c.FamilyId,
                c.Name,
                c.Slug,
                c.SortOrder,
                c.IsActive,
                c.Family.Name))
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<(List<SubcategoryAdminRowDto> Items, int Total)> ListSubcategoriesPagedAsync(
        AdminTableQueryParams query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        var q = db.Subcategories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Name.Contains(s) || x.Slug.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(query.CategoryName))
        {
            var c = query.CategoryName.Trim();
            q = q.Where(x => x.Category.Name.Contains(c));
        }

        if (!string.IsNullOrWhiteSpace(query.FamilyName))
        {
            var f = query.FamilyName.Trim();
            q = q.Where(x => x.Category.Family.Name.Contains(f));
        }

        if (query.FamilyIds is { Length: > 0 } familyIds)
            q = q.Where(x => familyIds.Contains(x.Category.FamilyId));

        if (query.CategoryIds is { Length: > 0 } categoryIds)
            q = q.Where(x => categoryIds.Contains(x.CategoryId));

        q = (query.SortKey?.ToLowerInvariant()) switch
        {
            "name" => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
            "categoryname" => desc ? q.OrderByDescending(x => x.Category.Name) : q.OrderBy(x => x.Category.Name),
            "familyname" => desc ? q.OrderByDescending(x => x.Category.Family.Name) : q.OrderBy(x => x.Category.Family.Name),
            "id" => desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id),
            _ => desc ? q.OrderByDescending(x => x.SortOrder) : q.OrderBy(x => x.SortOrder),
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SubcategoryAdminRowDto(
                s.Id,
                s.CategoryId,
                s.Category.FamilyId,
                s.Name,
                s.Slug,
                s.SortOrder,
                s.IsActive,
                s.Category.Name,
                s.Category.Family.Name))
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<Family> SaveFamilyAsync(Family entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) db.Families.Add(entity);
        else db.Families.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteFamilyAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Families.FindAsync([id], ct) ?? throw new NotFoundException("Family", id);
        db.Families.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<Category?> GetCategoryAsync(Guid id, CancellationToken ct = default) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Category> SaveCategoryAsync(Category entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) db.Categories.Add(entity);
        else db.Categories.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Categories.FindAsync([id], ct) ?? throw new NotFoundException("Category", id);
        db.Categories.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<Subcategory?> GetSubcategoryAsync(Guid id, CancellationToken ct = default) =>
        db.Subcategories.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Subcategory> SaveSubcategoryAsync(Subcategory entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) db.Subcategories.Add(entity);
        else db.Subcategories.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteSubcategoryAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Subcategories.FindAsync([id], ct) ?? throw new NotFoundException("Subcategory", id);
        db.Subcategories.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default) =>
        db.Products.Include(p => p.Variants).Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(List<Product> Items, int Total)> ListProductsAsync(
        int page, int pageSize, string? search, string? sortBy, string sortDirection, CancellationToken ct = default)
    {
        var q = db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(term) || p.Slug.ToLower().Contains(term));
        }

        var total = await q.CountAsync(ct);
        var sortFields = new Dictionary<string, Expression<Func<Product, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = p => p.Name,
            ["slug"] = p => p.Slug,
            ["basePrice"] = p => p.BasePrice,
            ["createdAt"] = p => p.CreatedAt,
        };
        var items = await q
            .OrderByField(sortBy, sortDirection, sortFields, p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Product> SaveProductAsync(Product entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) db.Products.Add(entity);
        else db.Products.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Products.FindAsync([id], ct) ?? throw new NotFoundException("Product", id);
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public Task<Variant?> GetVariantAsync(Guid id, CancellationToken ct = default) =>
        db.Variants.Include(v => v.Inventory).FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Variant> SaveVariantAsync(Variant entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty)
        {
            db.Variants.Add(entity);
            await db.SaveChangesAsync(ct);
            db.Inventories.Add(new Inventory { VariantId = entity.Id, QuantityOnHand = 0 });
        }
        else db.Variants.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    private static IQueryable<Family> ApplySortOrderRanges(
        IQueryable<Family> query,
        List<(int Min, int Max)> ranges)
    {
        var param = Expression.Parameter(typeof(Family), "f");
        var sortOrder = Expression.Property(param, nameof(Family.SortOrder));
        Expression? body = null;
        foreach (var (min, max) in ranges)
        {
            var ge = Expression.GreaterThanOrEqual(sortOrder, Expression.Constant(min));
            var le = Expression.LessThanOrEqual(sortOrder, Expression.Constant(max));
            var range = Expression.AndAlso(ge, le);
            body = body is null ? range : Expression.OrElse(body, range);
        }

        var predicate = Expression.Lambda<Func<Family, bool>>(body!, param);
        return query.Where(predicate);
    }

    private static List<(int Min, int Max)> ParseIdBuckets(string[] buckets)
    {
        var list = new List<(int Min, int Max)>();
        foreach (var raw in buckets)
        {
            var parts = raw.Split('-', StringSplitOptions.TrimEntries);
            if (parts.Length == 2
                && int.TryParse(parts[0], out var min)
                && int.TryParse(parts[1], out var max))
            {
                list.Add((min, max));
            }
        }

        return list;
    }

    public async Task DeleteVariantAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Variants.FindAsync([id], ct) ?? throw new NotFoundException("Variant", id);
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }
}
