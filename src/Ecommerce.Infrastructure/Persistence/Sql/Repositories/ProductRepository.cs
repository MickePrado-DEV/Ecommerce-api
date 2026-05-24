using Ecommerce.Application.Abstractions.Persistence;
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

        public async Task<(List<Product> Items, int Total)> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default)
        {
            var q = db.Products.AsNoTracking().Where(p => p.IsActive);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => p.Name.Contains(search));

            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (items, total);
        }
    }
}
