using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Features.Orders;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class OrderReadRepository(EcommerceDbContext db) : IOrderReadRepository
{
    public async Task<(IReadOnlyList<OrderSummaryDto> Items, int Total)> ListSummariesByUserAsync(
        Guid userId, int page, int pageSize, string? status, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = db.Orders.AsNoTracking().Where(o => o.UserId == userId);
        if (Enum.TryParse<OrderStatus>(status, true, out var st))
            q = q.Where(o => o.Status == st);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryDto(o.Id, o.OrderNumber, o.Status.ToString(), o.Total, o.CreatedAt))
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<OrderDetailDto?> GetDetailForUserAsync(Guid orderId, Guid userId, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Address)
            .Include(o => o.Payment)
            .Include(o => o.Shipment!).ThenInclude(s => s.Driver)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
        return order is null ? null : OrderMapping.ToDetail(order);
    }

    public async Task<OrderTrackingDto?> GetTrackingForUserAsync(Guid orderId, Guid userId, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Shipment!).ThenInclude(s => s.Driver)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
        if (order is null) return null;

        OrderShipmentInfoDto? shipment = order.Shipment is null ? null : new(
            order.Shipment.Id,
            order.Shipment.Status.ToString(),
            order.Shipment.TrackingNumber,
            order.Shipment.Driver?.Name,
            order.Shipment.ShippedAt);

        return new OrderTrackingDto(order.Id, order.OrderNumber, order.Status.ToString(), shipment);
    }
}
