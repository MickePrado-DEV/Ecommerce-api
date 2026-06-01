using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Dispatch.Services;

internal static class RouteMapping
{
    public static DispatchBatchSummaryDto MapBatchSummary(DispatchBatch b) => new(
        b.Id,
        b.Code,
        b.Status.ToString(),
        b.BatchOrders.Count,
        (double)b.CenterLat,
        (double)b.CenterLng,
        (double)b.RadiusKm,
        b.MaxStops,
        b.CreatedAt);

    public static DeliveryRouteSummaryDto MapRouteSummary(DeliveryRoute r) => new(
        r.Id,
        r.Code,
        r.Status.ToString(),
        r.TotalStops,
        r.TotalDistanceKm.HasValue ? (double)r.TotalDistanceKm.Value : null,
        r.Driver?.Name,
        r.CreatedAt);

    public static DeliveryRouteDetailDto MapRouteDetail(DeliveryRoute r)
    {
        var stops = r.Stops.OrderBy(s => s.StopIndex).Select(s => new DeliveryRouteStopDto(
            s.Id,
            s.StopIndex,
            s.OrderId,
            s.Order.OrderNumber,
            s.AddressText,
            (double)s.Lat,
            (double)s.Lng,
            s.Status.ToString(),
            s.DeliveredAt,
            s.FailedAt)).ToList();

        return new DeliveryRouteDetailDto(
            r.Id,
            r.Code,
            r.Status.ToString(),
            r.OriginType.ToString(),
            r.OriginLat.HasValue ? (double)r.OriginLat.Value : null,
            r.OriginLng.HasValue ? (double)r.OriginLng.Value : null,
            r.TotalStops,
            r.TotalDistanceKm.HasValue ? (double)r.TotalDistanceKm.Value : null,
            r.DriverId,
            r.Driver?.Name,
            r.BatchId,
            r.Batch?.Code,
            r.CreatedAt,
            r.StartedAt,
            r.FinishedAt,
            stops,
            BuildGoogleMapsUrl(r, stops));
    }

    private static string? BuildGoogleMapsUrl(DeliveryRoute r, IReadOnlyList<DeliveryRouteStopDto> stops)
    {
        if (stops.Count == 0) return null;
        var points = new List<string>();
        if (r.OriginLat.HasValue && r.OriginLng.HasValue)
            points.Add($"{(double)r.OriginLat.Value},{(double)r.OriginLng.Value}");
        foreach (var s in stops)
            points.Add($"{s.Lat},{s.Lng}");
        if (points.Count < 2) return $"https://www.google.com/maps/dir/?api=1&destination={points[0]}";
        var origin = points[0];
        var destination = points[^1];
        var waypoints = points.Count > 2 ? string.Join("|", points.Skip(1).Take(points.Count - 2)) : null;
        var url = $"https://www.google.com/maps/dir/?api=1&origin={origin}&destination={destination}";
        if (!string.IsNullOrEmpty(waypoints))
            url += $"&waypoints={waypoints}";
        return url;
    }
}
