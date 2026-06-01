using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using FluentResults;

namespace Ecommerce.Application.Features.Dispatch.Services;

public class DispatchBatchService(IDispatchRepository dispatch)
{
    public async Task<Result<CreateBatchesResultDto>> AutoCreateBatchesAsync(
        AutoCreateBatchesRequest request,
        CancellationToken ct = default)
    {
        var settings = await dispatch.GetOrCreateSettingsAsync(ct);
        var radiusKm = request.RadiusKm ?? (double)settings.DefaultClusterRadiusKm;
        var maxStops = request.MaxStops ?? settings.DefaultMaxStopsPerBatch;

        var orders = await dispatch.GetReadyOrdersForBatchingAsync(request.From, request.To, ct);
        var inBatch = await dispatch.GetOrderIdsInBatchesAsync(ct);
        var candidates = orders
            .Where(o => !inBatch.Contains(o.Id) && TryGetCoords(o, out _, out _))
            .ToList();

        if (candidates.Count == 0)
            return Result.Ok(new CreateBatchesResultDto(0, []));

        var codes = new List<string>();
        var unassigned = new HashSet<Guid>(candidates.Select(o => o.Id));
        var candidateMap = candidates.ToDictionary(o => o.Id);

        await dispatch.ExecuteInTransactionAsync(async () =>
        {
            while (unassigned.Count > 0)
            {
                var seedId = unassigned.First();
                var seed = candidateMap[seedId];
                TryGetCoords(seed, out var centerLat, out var centerLng);

                var cluster = new List<(Order Order, double DistKm)>();
                foreach (var id in unassigned.ToList())
                {
                    var o = candidateMap[id];
                    TryGetCoords(o, out var lat, out var lng);
                    var dist = GeoMath.HaversineKm(centerLat, centerLng, lat, lng);
                    if (dist <= radiusKm)
                        cluster.Add((o, dist));
                }

                cluster = cluster.OrderBy(c => c.DistKm).Take(maxStops).ToList();
                if (cluster.Count == 0)
                {
                    unassigned.Remove(seedId);
                    continue;
                }

                var points = cluster.Select(c =>
                {
                    TryGetCoords(c.Order, out var la, out var lo);
                    return (la, lo);
                }).ToList();
                var (avgLat, avgLng) = GeoMath.Centroid(points);

                var batch = new DispatchBatch
                {
                    Id = Guid.NewGuid(),
                    Code = await dispatch.NextBatchCodeAsync(ct),
                    Status = DispatchBatchStatus.Open,
                    CenterLat = (decimal)avgLat,
                    CenterLng = (decimal)avgLng,
                    RadiusKm = (decimal)radiusKm,
                    MaxStops = maxStops,
                };

                var pivots = new List<DispatchBatchOrder>();
                var toUpdate = new List<Order>();
                foreach (var (order, distKm) in cluster)
                {
                    pivots.Add(new DispatchBatchOrder
                    {
                        Id = Guid.NewGuid(),
                        BatchId = batch.Id,
                        OrderId = order.Id,
                        DistanceKm = (decimal)distKm,
                    });
                    order.DispatchStatus = DispatchStatus.Batched;
                    order.BatchedAt = DateTime.UtcNow;
                    toUpdate.Add(order);
                    unassigned.Remove(order.Id);
                }

                await dispatch.AddBatchAsync(batch, pivots, toUpdate, ct);
                codes.Add(batch.Code);
            }
        }, ct);

        return Result.Ok(new CreateBatchesResultDto(codes.Count, codes));
    }

    public async Task<Result<DispatchBatchDetailDto>> CreateManualBatchAsync(
        ManualCreateBatchRequest request,
        CancellationToken ct = default)
    {
        if (request.OrderIds.Count == 0)
            return Result.Fail<DispatchBatchDetailDto>(AdminErrors.InvalidState("Selecciona al menos un pedido."));

        var settings = await dispatch.GetOrCreateSettingsAsync(ct);
        var radiusKm = request.RadiusKm ?? (double)settings.DefaultClusterRadiusKm;
        var maxStops = request.MaxStops ?? settings.DefaultMaxStopsPerBatch;
        if (request.OrderIds.Count > maxStops)
            return Result.Fail<DispatchBatchDetailDto>(AdminErrors.InvalidState($"Máximo {maxStops} pedidos por lote."));

        var orders = await dispatch.GetReadyOrdersForBatchingAsync(null, null, ct);
        var selected = orders.Where(o => request.OrderIds.Contains(o.Id)).ToList();
        if (selected.Count != request.OrderIds.Count)
            return Result.Fail<DispatchBatchDetailDto>(AdminErrors.InvalidState("Algunos pedidos no están en cola ready o no existen."));

        foreach (var o in selected)
        {
            if (!TryGetCoords(o, out _, out _))
                return Result.Fail<DispatchBatchDetailDto>(AdminErrors.InvalidState($"El pedido {o.OrderNumber} no tiene coordenadas."));
            if (await dispatch.IsOrderInActiveBatchAsync(o.Id, ct))
                return Result.Fail<DispatchBatchDetailDto>(AdminErrors.InvalidState($"El pedido {o.OrderNumber} ya está en un lote."));
        }

        TryGetCoords(selected[0], out var centerLat, out var centerLng);
        var points = selected.Select(o =>
        {
            TryGetCoords(o, out var la, out var lo);
            return (la, lo);
        }).ToList();
        var centroid = GeoMath.Centroid(points);

        DispatchBatch? batch = null;
        await dispatch.ExecuteInTransactionAsync(async () =>
        {
            batch = new DispatchBatch
            {
                Id = Guid.NewGuid(),
                Code = await dispatch.NextBatchCodeAsync(ct),
                Status = DispatchBatchStatus.Open,
                CenterLat = (decimal)centroid.Lat,
                CenterLng = (decimal)centroid.Lng,
                RadiusKm = (decimal)radiusKm,
                MaxStops = maxStops,
            };

            var pivots = new List<DispatchBatchOrder>();
            var toUpdate = new List<Order>();
            foreach (var order in selected)
            {
                TryGetCoords(order, out var la, out var lo);
                var dist = GeoMath.HaversineKm(centroid.Lat, centroid.Lng, la, lo);
                pivots.Add(new DispatchBatchOrder
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.Id,
                    OrderId = order.Id,
                    DistanceKm = (decimal)dist,
                });
                order.DispatchStatus = DispatchStatus.Batched;
                order.BatchedAt = DateTime.UtcNow;
                toUpdate.Add(order);
            }

            await dispatch.AddBatchAsync(batch, pivots, toUpdate, ct);
        }, ct);

        var detail = await dispatch.GetBatchDetailAsync(batch!.Id, ct);
        return Result.Ok(MapBatchDetail(detail!));
    }

    internal static bool TryGetCoords(Order order, out double lat, out double lng)
    {
        if (order.Address?.Latitude is { } la && order.Address.Longitude is { } lo)
        {
            lat = (double)la;
            lng = (double)lo;
            return true;
        }

        lat = lng = 0;
        return false;
    }

    internal static DispatchBatchDetailDto MapBatchDetail(DispatchBatch batch) => new(
        batch.Id,
        batch.Code,
        batch.Status.ToString(),
        (double)batch.CenterLat,
        (double)batch.CenterLng,
        (double)batch.RadiusKm,
        batch.MaxStops,
        batch.CreatedAt,
        batch.BatchOrders.Select(bo => new DispatchBatchOrderItemDto(
            bo.OrderId,
            bo.Order.OrderNumber,
            bo.DistanceKm.HasValue ? (double)bo.DistanceKm.Value : null,
            bo.Order.Address?.AddressText ?? FormatAddress(bo.Order.Address))).ToList());

    internal static string? FormatAddress(OrderAddress? a)
    {
        if (a is null) return null;
        return $"{a.Street}, {a.City}, {a.State} {a.PostalCode}";
    }
}
