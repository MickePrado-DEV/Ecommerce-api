using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using FluentResults;
namespace Ecommerce.Application.Features.Dispatch.Services;

public class RoutePlannerService(IDispatchRepository dispatch, IShipmentRepository shipments)
{
    public async Task<Result<DeliveryRouteDetailDto>> CreateRouteFromBatchAsync(
        Guid batchId,
        CreateRouteFromBatchRequest request,
        CancellationToken ct = default)
    {
        var batch = await dispatch.GetBatchDetailAsync(batchId, ct);
        if (batch is null)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.NotFound("Batch", batchId));
        if (batch.Status == DispatchBatchStatus.Converted)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("El lote ya tiene ruta creada."));

        var settings = await dispatch.GetOrCreateSettingsAsync(ct);
        var maxStops = request.MaxStops ?? settings.DefaultMaxStopsPerRoute;

        var orders = batch.BatchOrders.Select(bo => bo.Order).ToList();
        if (orders.Count > maxStops)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState($"Máximo {maxStops} paradas por ruta."));

        if (!Enum.TryParse<DeliveryRouteOriginType>(request.OriginType, true, out var originType))
            originType = settings.DefaultRouteOriginType;

        var (originLat, originLng) = await ResolveOriginAsync(originType, batch, request, ct);
        if (originLat is null || originLng is null)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("No se pudo determinar el origen de la ruta."));

        var stops = PlanStops(orders, (double)originLat.Value, (double)originLng.Value);
        var totalDistance = ComputeRouteDistance((double)originLat.Value, (double)originLng.Value, stops);

        DeliveryRoute? route = null;
        await dispatch.ExecuteInTransactionAsync(async () =>
        {
            route = new DeliveryRoute
            {
                Id = Guid.NewGuid(),
                Code = await dispatch.NextRouteCodeAsync(ct),
                Status = DeliveryRouteStatus.Draft,
                BatchId = batch.Id,
                DriverId = request.DriverId,
                OriginType = originType,
                OriginLat = originLat,
                OriginLng = originLng,
                TotalStops = stops.Count,
                TotalDistanceKm = (decimal)totalDistance,
            };

            var stopEntities = stops.Select(s => new DeliveryRouteStop
            {
                Id = Guid.NewGuid(),
                RouteId = route.Id,
                OrderId = s.Order.Id,
                StopIndex = s.Index,
                Lat = (decimal)s.Lat,
                Lng = (decimal)s.Lng,
                AddressText = s.AddressText,
                Status = DeliveryRouteStopStatus.Pending,
            }).ToList();

            foreach (var order in orders)
            {
                order.DispatchStatus = DispatchStatus.Routed;
                order.RoutedAt = DateTime.UtcNow;
            }

            batch.Status = DispatchBatchStatus.Converted;
            await dispatch.AddRouteAsync(route, stopEntities, orders, batch, ct);
        }, ct);

        var detail = await dispatch.GetRouteDetailAsync(route!.Id, ct);
        return Result.Ok(RouteMapping.MapRouteDetail(detail!));
    }

    private async Task<(decimal? Lat, decimal? Lng)> ResolveOriginAsync(
        DeliveryRouteOriginType originType,
        DispatchBatch batch,
        CreateRouteFromBatchRequest request,
        CancellationToken ct)
    {
        return originType switch
        {
            DeliveryRouteOriginType.Custom when request.OriginLat.HasValue && request.OriginLng.HasValue =>
                ((decimal)request.OriginLat.Value, (decimal)request.OriginLng.Value),
            DeliveryRouteOriginType.DriverLocation when request.DriverId.HasValue =>
                await GetDriverOriginAsync(request.DriverId.Value, batch, ct),
            _ => (batch.CenterLat, batch.CenterLng),
        };
    }

    private async Task<(decimal? Lat, decimal? Lng)> GetDriverOriginAsync(
        Guid driverId,
        DispatchBatch batch,
        CancellationToken ct)
    {
        var driver = await shipments.GetDriverWithUserAsync(driverId, ct);
        if (driver?.StartLatitude is { } la && driver.StartLongitude is { } lo)
            return (la, lo);
        return (batch.CenterLat, batch.CenterLng);
    }

    private static List<PlannedStop> PlanStops(List<Order> orders, double originLat, double originLng)
    {
        var remaining = orders.ToList();
        var result = new List<PlannedStop>();
        var currentLat = originLat;
        var currentLng = originLng;
        var index = 1;

        while (remaining.Count > 0)
        {
            Order? next = null;
            var bestDist = double.MaxValue;
            foreach (var o in remaining)
            {
                if (!DispatchBatchService.TryGetCoords(o, out var lat, out var lng)) continue;
                var d = GeoMath.HaversineKm(currentLat, currentLng, lat, lng);
                if (d < bestDist)
                {
                    bestDist = d;
                    next = o;
                }
            }

            if (next is null) break;
            DispatchBatchService.TryGetCoords(next, out var nLat, out var nLng);
            result.Add(new PlannedStop(index++, next, nLat, nLng,
                next.Address?.AddressText ?? DispatchBatchService.FormatAddress(next.Address) ?? next.OrderNumber));
            remaining.Remove(next);
            currentLat = nLat;
            currentLng = nLng;
        }

        return result;
    }

    private static double ComputeRouteDistance(double originLat, double originLng, List<PlannedStop> stops)
    {
        if (stops.Count == 0) return 0;
        var total = 0.0;
        var lat = originLat;
        var lng = originLng;
        foreach (var s in stops)
        {
            total += GeoMath.HaversineKm(lat, lng, s.Lat, s.Lng);
            lat = s.Lat;
            lng = s.Lng;
        }

        return total;
    }

    private sealed record PlannedStop(int Index, Order Order, double Lat, double Lng, string AddressText);
}
