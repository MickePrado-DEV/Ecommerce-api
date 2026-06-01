using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Sql.Common;
using System.Linq.Expressions;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

internal static class InventoryListQueries
{
    public static async Task<(List<Inventory> Items, int Total)> ListPagedAsync(
        EcommerceDbContext db,
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        string sortDirection,
        CancellationToken ct = default)
    {
        var q = db.Inventories.AsNoTracking().Include(i => i.Variant).ThenInclude(v => v.Product).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(i =>
                i.Variant.Sku.ToLower().Contains(term) ||
                i.Variant.Product.Name.ToLower().Contains(term));
        }

        var total = await q.CountAsync(ct);
        var sortFields = new Dictionary<string, Expression<Func<Inventory, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["sku"] = i => i.Variant.Sku,
            ["productName"] = i => i.Variant.Product.Name,
            ["quantityOnHand"] = i => i.QuantityOnHand,
            ["available"] = i => i.QuantityOnHand - i.QuantityReserved,
        };
        var items = await q
            .OrderByField(sortBy, sortDirection, sortFields, i => i.Variant.Sku)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
