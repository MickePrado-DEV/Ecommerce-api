using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Emums;
using FluentResults;

namespace Ecommerce.Application.Features.Dispatch.Services;

public class RouteAssignmentService(IDispatchRepository dispatch, IShipmentRepository shipments)
{
    public async Task<Result<DeliveryRouteDetailDto>> AssignAsync(
        Guid routeId,
        Guid driverId,
        CancellationToken ct = default)
    {
        var route = await dispatch.GetRouteDetailAsync(routeId, ct);
        if (route is null)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.NotFound("Route", routeId));
        if (route.Status is not DeliveryRouteStatus.Draft and not DeliveryRouteStatus.Assigned)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("La ruta no admite asignación en este estado."));

        var driver = await shipments.GetDriverWithUserAsync(driverId, ct);
        if (driver is null || !driver.IsActive)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.NotFound("Driver", driverId));

        if (driver.Capacity is { } cap && route.Stops.Count > cap)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState($"El conductor admite máximo {cap} paradas."));

        route.DriverId = driverId;
        route.Status = DeliveryRouteStatus.Assigned;
        var orders = route.Stops.Select(s => s.Order).ToList();
        foreach (var order in orders)
        {
            order.DispatchStatus = DispatchStatus.Assigned;
            order.AssignedAt = DateTime.UtcNow;
        }

        await dispatch.AssignRouteAsync(route, orders, ct);
        var updated = await dispatch.GetRouteDetailAsync(routeId, ct);
        return Result.Ok(RouteMapping.MapRouteDetail(updated!));
    }
}
