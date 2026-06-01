using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Ecommerce.Infrastructure.Persistence.Sql.Common;
using System.Linq.Expressions;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class OrderRepository(EcommerceDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Orders
            .Include(o => o.Items)
            .Include(o => o.Address)
            .Include(o => o.Payment)
            .Include(o => o.Shipment).ThenInclude(s => s!.Driver)
            .Include(o => o.Shipment).ThenInclude(s => s!.Ticket)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Orders
            .Include(o => o.Items)
            .Include(o => o.Address)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

    public Task<List<Order>> ListByUserAsync(Guid userId, CancellationToken ct = default) =>
        db.Orders.AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .ToListAsync(ct);

    public async Task<(List<Order> Items, int Total)> ListAdminAsync(
        int page, int pageSize, OrderStatus? status, string? sortBy, string sortDirection, CancellationToken ct = default)
    {
        var q = db.Orders.AsNoTracking().AsQueryable();
        if (status.HasValue) q = q.Where(o => o.Status == status);
        var total = await q.CountAsync(ct);
        var sortFields = new Dictionary<string, Expression<Func<Order, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["orderNumber"] = o => o.OrderNumber,
            ["createdAt"] = o => o.CreatedAt,
            ["total"] = o => o.Total,
            ["status"] = o => o.Status,
        };
        var items = await q
            .OrderByField(sortBy, sortDirection, sortFields, o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Include(o => o.Items)
            .Include(o => o.Shipment)
            .ToListAsync(ct);
        return (items, total);
    }

    public Task<Order> AddAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Add(order);
        return Task.FromResult(order);
    }

    public async Task UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken ct = default)
    {
        var order = await db.Orders.FindAsync([orderId], ct)
            ?? throw new InvalidOperationException("Order not found");
        order.Status = status;
        await db.SaveChangesAsync(ct);
    }

    public string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
}
