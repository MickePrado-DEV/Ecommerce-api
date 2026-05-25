using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Features.Orders;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class OrderReadRepository(EcommerceDbContext db) : IOrderReadRepository
{
    public async Task<IReadOnlyList<OrderSummaryDto>> ListSummariesByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await db.Orders.AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto(o.Id, o.OrderNumber, o.Status.ToString(), o.Total, o.CreatedAt))
            .ToListAsync(ct);
        return list;
    }

    public async Task<OrderDetailDto?> GetDetailForUserAsync(Guid orderId, Guid userId, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Address)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
        return order is null ? null : OrderMapping.ToDetail(order);
    }
}
