// Rutas mobile repartidor: requiere JWT con rol driver.
using Ecommerce.Api.Extensions;
using Ecommerce.Application.Features.DriverPortal;
using Ecommerce.Domain.Authorization;
using Ecommerce.Domain.Emums;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class DriverEndpoints
{
    public static RouteGroupBuilder MapDriverEndpoints(this RouteGroupBuilder group)
    {
        var driver = group.MapGroup("/driver")
            .WithTags("Driver")
            .RequireAuthorization()
            .RequireAuthorization(policy => policy.RequireRole(RoleCodes.Driver));

        driver.MapGet("/me", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new GetDriverProfileQuery(userId.Value), ct)).ToHttpResult();
        });

        driver.MapGet("/shipments", async (int page, int pageSize, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new ListMyShipmentsQuery(userId.Value, page, pageSize), ct)).ToHttpResult();
        });

        driver.MapPatch("/shipments/{shipmentId:guid}/in-transit", async (Guid shipmentId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(
                new DriverUpdateShipmentStatusCommand(userId.Value, shipmentId, ShipmentStatus.InTransit), ct)).ToHttpResult();
        });

        driver.MapPatch("/shipments/{shipmentId:guid}/delivered", async (Guid shipmentId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(
                new DriverUpdateShipmentStatusCommand(userId.Value, shipmentId, ShipmentStatus.Delivered), ct)).ToHttpResult();
        });

        return group;
    }
}
