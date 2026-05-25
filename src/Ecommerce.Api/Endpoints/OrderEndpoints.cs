using Ecommerce.Api.Extensions;
using Ecommerce.Application.Features.Orders;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        var orders = group.MapGroup("/orders").WithTags("Orders").RequireAuthorization();

        orders.MapGet("/", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new ListMyOrdersQuery(userId.Value), ct)).ToHttpResult();
        });

        orders.MapGet("/{orderId:guid}", async (Guid orderId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new GetMyOrderQuery(userId.Value, orderId), ct)).ToHttpResult();
        });

        orders.MapPost("/{orderId:guid}/pay", async (Guid orderId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new PayOrderCommand(userId.Value, orderId), ct)).ToHttpResult();
        });

        orders.MapPost("/{orderId:guid}/retry-payment", async (Guid orderId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new PayOrderCommand(userId.Value, orderId), ct)).ToHttpResult();
        });

        return group;
    }
}
