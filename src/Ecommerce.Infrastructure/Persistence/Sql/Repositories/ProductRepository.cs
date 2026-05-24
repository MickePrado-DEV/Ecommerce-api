using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories
{
    public class ProductRepository(EcommerceDbContext db) : IProductRepository
    {
        public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
            db.Products
                .Include(p => p.Variants).ThenInclude(v => v.Inventory)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, ct);

        public async Task<(List<Product> Items, int Total)> ListAsync(CatalogProductQuery query, CancellationToken ct = default)
        {
            var q = db.Products.AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.IsActive);

            if (query.SubcategoryId.HasValue)
                q = q.Where(p => p.SubcategoryId == query.SubcategoryId);
            else if (query.CategoryId.HasValue)
                q = q.Where(p => p.Subcategory.CategoryId == query.CategoryId);
            else if (query.FamilyId.HasValue)
                q = q.Where(p => p.Subcategory.Category.FamilyId == query.FamilyId);

            if (!string.IsNullOrWhiteSpace(query.Search))
                q = q.Where(p => p.Name.Contains(query.Search) || p.Description!.Contains(query.Search));

            q = ApplySort(q, query.Sort);

            var total = await q.CountAsync(ct);
            var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
            return (items, total);
        }

        public Task<List<Product>> ListLatestAsync(int take, CancellationToken ct = default) =>
            db.Products.AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .ToListAsync(ct);

        private static IQueryable<Product> ApplySort(IQueryable<Product> q, string? sort) => sort?.ToLowerInvariant() switch
        {
            "price:desc" or "2" => q.OrderByDescending(p => p.BasePrice),
            "price:asc" or "3" => q.OrderBy(p => p.BasePrice),
            "recent" or "1" => q.OrderByDescending(p => p.CreatedAt),
            _ => q.OrderBy(p => p.Name)
        };
    }
}
