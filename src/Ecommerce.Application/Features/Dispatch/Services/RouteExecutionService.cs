using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Emums;
using FluentResults;

namespace Ecommerce.Application.Features.Dispatch.Services;

public class RouteExecutionService(IDispatchRepository dispatch)
{
    public async Task<Result<DeliveryRouteDetailDto>> StartAsync(Guid routeId, CancellationToken ct = default)
    {
        var route = await dispatch.GetRouteDetailAsync(routeId, ct);
        if (route is null)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.NotFound("Route", routeId));
        if (route.Status != DeliveryRouteStatus.Assigned)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("La ruta debe estar asignada para iniciar."));
        if (route.DriverId is null)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("Asigna un conductor antes de iniciar."));

        route.Status = DeliveryRouteStatus.InTransit;
        route.StartedAt = DateTime.UtcNow;
        var orders = route.Stops.Select(s => s.Order).ToList();
        foreach (var order in orders)
        {
            order.DispatchStatus = DispatchStatus.InTransit;
            if (order.Status is OrderStatus.Paid or OrderStatus.ReadyToDispatch)
                order.Status = OrderStatus.Dispatched;
        }

        await dispatch.StartRouteAsync(route, orders, ct);
        var updated = await dispatch.GetRouteDetailAsync(routeId, ct);
        return Result.Ok(RouteMapping.MapRouteDetail(updated!));
    }

    public async Task<Result<DeliveryRouteDetailDto>> FinishAsync(Guid routeId, CancellationToken ct = default)
    {
        var route = await dispatch.GetRouteDetailAsync(routeId, ct);
        if (route is null)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.NotFound("Route", routeId));
        if (route.Status != DeliveryRouteStatus.InTransit)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("La ruta debe estar en tránsito."));

        if (route.Stops.Any(s => s.Status == DeliveryRouteStopStatus.Pending))
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("Todas las paradas deben estar entregadas o fallidas."));

        route.Status = DeliveryRouteStatus.Done;
        route.FinishedAt = DateTime.UtcNow;
        var orders = route.Stops.Select(s => s.Order).ToList();
        foreach (var order in orders)
        {
            var stop = route.Stops.First(s => s.OrderId == order.Id);
            order.DispatchStatus = stop.Status == DeliveryRouteStopStatus.Delivered
                ? DispatchStatus.Delivered
                : DispatchStatus.Failed;
            if (stop.Status == DeliveryRouteStopStatus.Delivered)
            {
                order.DispatchDeliveredAt = stop.DeliveredAt;
                order.Status = OrderStatus.Delivered;
            }
        }

        await dispatch.FinishRouteAsync(route, orders, ct);
        var updated = await dispatch.GetRouteDetailAsync(routeId, ct);
        return Result.Ok(RouteMapping.MapRouteDetail(updated!));
    }

    public async Task<Result<DeliveryRouteDetailDto>> MarkStopDeliveredAsync(Guid stopId, CancellationToken ct = default)
    {
        var route = await FindRouteByStopAsync(stopId, ct);
        if (route is null)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.NotFound("Stop", stopId));

        var stop = route.Stops.First(s => s.Id == stopId);
        if (stop.Status != DeliveryRouteStopStatus.Pending)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("La parada ya fue procesada."));

        stop.Status = DeliveryRouteStopStatus.Delivered;
        stop.DeliveredAt = DateTime.UtcNow;
        stop.Order.DispatchStatus = DispatchStatus.Delivered;
        stop.Order.DispatchDeliveredAt = stop.DeliveredAt;
        stop.Order.Status = OrderStatus.Delivered;

        await dispatch.UpdateStopDeliveredAsync(stop, stop.Order, ct);
        var updated = await dispatch.GetRouteDetailAsync(route.Id, ct);
        return Result.Ok(RouteMapping.MapRouteDetail(updated!));
    }

    public async Task<Result<DeliveryRouteDetailDto>> MarkStopFailedAsync(
        Guid stopId,
        string? reason,
        CancellationToken ct = default)
    {
        var route = await FindRouteByStopAsync(stopId, ct);
        if (route is null)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.NotFound("Stop", stopId));

        var stop = route.Stops.First(s => s.Id == stopId);
        if (stop.Status != DeliveryRouteStopStatus.Pending)
            return Result.Fail<DeliveryRouteDetailDto>(AdminErrors.InvalidState("La parada ya fue procesada."));

        stop.Status = DeliveryRouteStopStatus.Failed;
        stop.FailedAt = DateTime.UtcNow;
        stop.FailureReason = reason;
        stop.Order.DispatchStatus = DispatchStatus.Failed;

        await dispatch.UpdateStopFailedAsync(stop, stop.Order, reason, ct);
        var updated = await dispatch.GetRouteDetailAsync(route.Id, ct);
        return Result.Ok(RouteMapping.MapRouteDetail(updated!));
    }

    private Task<Domain.Entities.DeliveryRoute?> FindRouteByStopAsync(Guid stopId, CancellationToken ct) =>
        dispatch.GetRouteByStopIdAsync(stopId, ct);
}
