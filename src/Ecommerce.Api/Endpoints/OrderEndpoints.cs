// Pedidos del cliente: listado, detalle, tracking, pago y cancelación.
using Ecommerce.Api.Extensions;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Features.Orders;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        var orders = group.MapGroup("/orders").WithTags("Orders").RequireAuthorization();

        orders.MapGet("/", async (int page, int pageSize, string? status, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new ListMyOrdersQuery(userId.Value, page, pageSize, status), ct)).ToHttpResult();
        });

        orders.MapGet("/{orderId:guid}", async (Guid orderId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new GetMyOrderQuery(userId.Value, orderId), ct)).ToHttpResult();
        });

        orders.MapGet("/{orderId:guid}/tracking", async (Guid orderId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new GetOrderTrackingQuery(userId.Value, orderId), ct)).ToHttpResult();
        });

        orders.MapPost("/{orderId:guid}/cancel", async (Guid orderId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new CancelOrderCommand(userId.Value, orderId), ct)).ToHttpResult();
        });

        orders.MapPost("/{orderId:guid}/pay", async (Guid orderId, PayOrderRequest? req, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new PayOrderCommand(userId.Value, orderId, req), ct)).ToHttpResult();
        });

        orders.MapPost("/{orderId:guid}/retry-payment", async (Guid orderId, PayOrderRequest? req, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new PayOrderCommand(userId.Value, orderId, req), ct)).ToHttpResult();
        });

        return group;
    }
}
