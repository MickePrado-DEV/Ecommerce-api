using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class DispatchRepository(EcommerceDbContext db) : IDispatchRepository
{
    private static readonly DispatchStatus[] LockedDispatchStatuses =
    [
        DispatchStatus.Batched,
        DispatchStatus.Routed,
        DispatchStatus.Assigned,
        DispatchStatus.InTransit,
        DispatchStatus.Delivered,
    ];

    public async Task<DispatchSettings> GetOrCreateSettingsAsync(CancellationToken ct = default)
    {
        var settings = await db.DispatchSettings.FirstOrDefaultAsync(ct);
        if (settings is not null) return settings;

        settings = new DispatchSettings { Id = Guid.Parse("00000000-0000-0000-0000-000000000001") };
        db.DispatchSettings.Add(settings);
        await db.SaveChangesAsync(ct);
        return settings;
    }

    public async Task UpdateSettingsAsync(DispatchSettings settings, CancellationToken ct = default)
    {
        db.DispatchSettings.Update(settings);
        await db.SaveChangesAsync(ct);
    }

    public async Task<(List<Order> Items, int Total)> ListReadyQueueAsync(DispatchQueueFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var q = db.Orders.AsNoTracking()
            .Include(o => o.Address)
            .Where(o => o.DispatchStatus == DispatchStatus.Ready);

        if (filter.From.HasValue)
            q = q.Where(o => o.CreatedAt >= filter.From.Value);
        if (filter.To.HasValue)
            q = q.Where(o => o.CreatedAt <= filter.To.Value);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public Task<List<Order>> GetReadyOrdersForBatchingAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var q = db.Orders
            .Include(o => o.Address)
            .Where(o => o.DispatchStatus == DispatchStatus.Ready);

        if (from.HasValue) q = q.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(o => o.CreatedAt <= to.Value);

        return q.OrderBy(o => o.CreatedAt).ToListAsync(ct);
    }

    public Task<bool> IsOrderInActiveBatchAsync(Guid orderId, CancellationToken ct = default) =>
        db.DispatchBatchOrders.AnyAsync(bo => bo.OrderId == orderId, ct);

    public async Task<HashSet<Guid>> GetOrderIdsInBatchesAsync(CancellationToken ct = default) =>
        (await db.DispatchBatchOrders.AsNoTracking().Select(bo => bo.OrderId).ToListAsync(ct)).ToHashSet();

    public async Task<string> NextBatchCodeAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"WB-{today}-";
        var count = await db.DispatchBatches.CountAsync(b => b.Code.StartsWith(prefix), ct);
        return $"{prefix}{count + 1:D3}";
    }

    public async Task<string> NextRouteCodeAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"RT-{today}-";
        var count = await db.DeliveryRoutes.CountAsync(r => r.Code.StartsWith(prefix), ct);
        return $"{prefix}{count + 1:D3}";
    }

    public async Task AddBatchAsync(
        DispatchBatch batch,
        IEnumerable<DispatchBatchOrder> pivots,
        IEnumerable<Order> ordersToUpdate,
        CancellationToken ct = default)
    {
        db.DispatchBatches.Add(batch);
        db.DispatchBatchOrders.AddRange(pivots);
        foreach (var order in ordersToUpdate)
            db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public Task<List<DispatchBatch>> ListBatchesAsync(CancellationToken ct = default) =>
        db.DispatchBatches.AsNoTracking()
            .Include(b => b.BatchOrders)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public Task<DispatchBatch?> GetBatchDetailAsync(Guid batchId, CancellationToken ct = default) =>
        db.DispatchBatches
            .Include(b => b.BatchOrders).ThenInclude(bo => bo.Order).ThenInclude(o => o.Address)
            .FirstOrDefaultAsync(b => b.Id == batchId, ct);

    public async Task AddRouteAsync(
        DeliveryRoute route,
        IEnumerable<DeliveryRouteStop> stops,
        IEnumerable<Order> ordersToUpdate,
        DispatchBatch? batchToUpdate,
        CancellationToken ct = default)
    {
        db.DeliveryRoutes.Add(route);
        db.DeliveryRouteStops.AddRange(stops);
        foreach (var order in ordersToUpdate)
            db.Orders.Update(order);
        if (batchToUpdate is not null)
            db.DispatchBatches.Update(batchToUpdate);
        await db.SaveChangesAsync(ct);
    }

    public Task<List<DeliveryRoute>> ListRoutesAsync(CancellationToken ct = default) =>
        db.DeliveryRoutes.AsNoTracking()
            .Include(r => r.Driver)
            .Include(r => r.Stops)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public Task<DeliveryRoute?> GetRouteDetailAsync(Guid routeId, CancellationToken ct = default) =>
        db.DeliveryRoutes
            .Include(r => r.Driver)
            .Include(r => r.Batch)
            .Include(r => r.Stops).ThenInclude(s => s.Order)
            .FirstOrDefaultAsync(r => r.Id == routeId, ct);

    public async Task<DeliveryRoute?> GetRouteByStopIdAsync(Guid stopId, CancellationToken ct = default)
    {
        var stop = await db.DeliveryRouteStops.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == stopId, ct);
        return stop is null ? null : await GetRouteDetailAsync(stop.RouteId, ct);
    }

    public async Task AssignRouteAsync(DeliveryRoute route, IEnumerable<Order> orders, CancellationToken ct = default)
    {
        db.DeliveryRoutes.Update(route);
        foreach (var order in orders)
            db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task StartRouteAsync(DeliveryRoute route, IEnumerable<Order> orders, CancellationToken ct = default)
    {
        db.DeliveryRoutes.Update(route);
        foreach (var order in orders)
            db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task FinishRouteAsync(DeliveryRoute route, IEnumerable<Order> orders, CancellationToken ct = default)
    {
        db.DeliveryRoutes.Update(route);
        foreach (var order in orders)
            db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateStopDeliveredAsync(DeliveryRouteStop stop, Order order, CancellationToken ct = default)
    {
        db.DeliveryRouteStops.Update(stop);
        db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateStopFailedAsync(DeliveryRouteStop stop, Order order, string? reason, CancellationToken ct = default)
    {
        db.DeliveryRouteStops.Update(stop);
        db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task<OrderDispatchInfoDto?> GetOrderDispatchInfoAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null) return null;

        var batchOrder = await db.DispatchBatchOrders.AsNoTracking()
            .Include(bo => bo.Batch)
            .Where(bo => bo.OrderId == orderId)
            .OrderByDescending(bo => bo.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var stop = await db.DeliveryRouteStops.AsNoTracking()
            .Include(s => s.Route).ThenInclude(r => r!.Driver)
            .Where(s => s.OrderId == orderId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var canMarkReady = order.Status == OrderStatus.Paid
            && !LockedDispatchStatuses.Contains(order.DispatchStatus);

        return new OrderDispatchInfoDto(
            order.DispatchStatus.ToString(),
            batchOrder?.Batch.Code,
            stop?.Route.Code,
            stop?.Route.Driver?.Name,
            canMarkReady);
    }

    public async Task MarkOrderDispatchReadyAsync(Order order, CancellationToken ct = default)
    {
        order.Status = OrderStatus.ReadyToDispatch;
        order.DispatchStatus = DispatchStatus.Ready;
        order.ReadyAt = DateTime.UtcNow;
        db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await action();
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
