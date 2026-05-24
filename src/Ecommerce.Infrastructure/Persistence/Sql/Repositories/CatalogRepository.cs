using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class CatalogRepository(EcommerceDbContext db) : ICatalogRepository
{
    public Task<List<Family>> GetFamiliesTreeAsync(CancellationToken ct = default) =>
        db.Families.AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .Include(f => f.Categories).ThenInclude(c => c.Subcategories)
            .ToListAsync(ct);
}
