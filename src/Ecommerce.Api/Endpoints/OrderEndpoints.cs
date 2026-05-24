using Ecommerce.Application.Abstractions;

namespace Ecommerce.Api.Endpoints;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        var orders = group.MapGroup("/orders").WithTags("Orders").RequireAuthorization();

        orders.MapGet("/", async (IOrderService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await svc.ListMineAsync(userId.Value, ct));
        });

        orders.MapGet("/{orderId:guid}", async (Guid orderId, IOrderService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            var order = await svc.GetAsync(userId.Value, orderId, ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        });

        orders.MapPost("/{orderId:guid}/pay", async (Guid orderId, IOrderService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await svc.PayMockAsync(userId.Value, orderId, ct));
        });

        orders.MapPost("/{orderId:guid}/retry-payment", async (Guid orderId, IOrderService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await svc.PayMockAsync(userId.Value, orderId, ct));
        });

        return group;
    }
}
