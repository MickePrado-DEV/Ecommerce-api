using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class CatalogRepository(EcommerceDbContext db) : ICatalogRepository
{
    public Task<List<Family>> GetFamiliesTreeAsync(CancellationToken ct = default) =>
        db.Families.AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .Include(f => f.Categories.Where(c => c.IsActive)).ThenInclude(c => c.Subcategories.Where(s => s.IsActive))
            .ToListAsync(ct);

    public Task<Family?> GetFamilyBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Families.AsNoTracking()
            .Where(f => f.IsActive && f.Slug == slug)
            .Include(f => f.Categories.Where(c => c.IsActive)).ThenInclude(c => c.Subcategories.Where(s => s.IsActive))
            .FirstOrDefaultAsync(ct);

    public Task<Category?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Categories.AsNoTracking()
            .Where(c => c.IsActive && c.Slug == slug)
            .Include(c => c.Subcategories.Where(s => s.IsActive))
            .FirstOrDefaultAsync(ct);

    public Task<Subcategory?> GetSubcategoryBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Subcategories.AsNoTracking()
            .Where(s => s.IsActive && s.Slug == slug)
            .Include(s => s.Category)
            .FirstOrDefaultAsync(ct);
}
