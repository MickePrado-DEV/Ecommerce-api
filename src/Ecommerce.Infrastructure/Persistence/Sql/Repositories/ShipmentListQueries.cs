using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Sql.Common;
using System.Linq.Expressions;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

internal static class ShipmentListQueries
{
    public static async Task<(List<Shipment> Items, int Total)> ListShipmentsAsync(
        EcommerceDbContext db,
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        string sortDirection,
        CancellationToken ct = default)
    {
        var q = db.Shipments.AsNoTracking().Include(s => s.Driver).Include(s => s.Order).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(s =>
                s.Order.OrderNumber.ToLower().Contains(term) ||
                (s.TrackingNumber != null && s.TrackingNumber.ToLower().Contains(term)) ||
                (s.Driver != null && s.Driver.Name.ToLower().Contains(term)));
        }

        var total = await q.CountAsync(ct);
        var sortFields = new Dictionary<string, Expression<Func<Shipment, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["orderNumber"] = s => s.Order.OrderNumber,
            ["status"] = s => s.Status,
            ["trackingNumber"] = s => s.TrackingNumber!,
            ["createdAt"] = s => s.CreatedAt,
        };
        var items = await q
            .OrderByField(sortBy, sortDirection, sortFields, s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public static async Task<(List<Driver> Items, int Total)> ListDriversPagedAsync(
        EcommerceDbContext db,
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        string sortDirection,
        CancellationToken ct = default)
    {
        var q = db.Drivers.AsNoTracking().Include(d => d.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(d =>
                d.Name.ToLower().Contains(term) ||
                d.Phone.ToLower().Contains(term) ||
                (d.VehiclePlate != null && d.VehiclePlate.ToLower().Contains(term)));
        }

        var total = await q.CountAsync(ct);
        var sortFields = new Dictionary<string, Expression<Func<Driver, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = d => d.Name,
            ["phone"] = d => d.Phone,
            ["vehiclePlate"] = d => d.VehiclePlate!,
            ["isActive"] = d => d.IsActive,
        };
        var items = await q
            .OrderByField(sortBy, sortDirection, sortFields, d => d.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
