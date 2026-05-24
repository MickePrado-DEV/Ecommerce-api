using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class DashboardRepository(EcommerceDbContext db) : IDashboardRepository
{
    public async Task<(int Orders, int PendingPayment, int Paid, int ReadyToDispatch, int Products, int Users)> GetStatsAsync(CancellationToken ct = default)
    {
        var orders = await db.Orders.CountAsync(ct);
        var pending = await db.Orders.CountAsync(o => o.Status == OrderStatus.PendingPayment, ct);
        var paid = await db.Orders.CountAsync(o => o.Status == OrderStatus.Paid, ct);
        var ready = await db.Orders.CountAsync(o => o.Status == OrderStatus.ReadyToDispatch, ct);
        var products = await db.Products.CountAsync(p => p.IsActive, ct);
        var users = await db.Users.CountAsync(ct);
        return (orders, pending, paid, ready, products, users);
    }
}
