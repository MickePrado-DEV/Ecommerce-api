using Ecommerce.Api.Filters;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.DTOs.Cart;

namespace Ecommerce.Api.Endpoints;

public static class CartEndpoints
{
    public static RouteGroupBuilder MapCartEndpoints(this RouteGroupBuilder group)
    {
        var cart = group.MapGroup("/cart").WithTags("Cart");

        cart.MapGet("/", async (ICartService svc, HttpContext ctx, CancellationToken ct) =>
            Results.Ok(await svc.GetAsync(ctx.GetUserId(), ctx.GetGuestToken(), ct)));

        cart.MapPost("/items", async (AddCartItemRequest req, ICartService svc, HttpContext ctx, CancellationToken ct) =>
            Results.Ok(await svc.AddItemAsync(ctx.GetUserId(), ctx.GetGuestToken(), req, ct)))
            .WithValidation<AddCartItemRequest>();

        cart.MapPut("/items/{itemId:guid}", async (Guid itemId, UpdateCartItemRequest req, ICartService svc, HttpContext ctx, CancellationToken ct) =>
            Results.Ok(await svc.UpdateItemAsync(ctx.GetUserId(), ctx.GetGuestToken(), itemId, req, ct)))
            .WithValidation<UpdateCartItemRequest>();

        cart.MapPatch("/items/{itemId:guid}", async (Guid itemId, UpdateCartItemRequest req, ICartService svc, HttpContext ctx, CancellationToken ct) =>
            Results.Ok(await svc.UpdateItemAsync(ctx.GetUserId(), ctx.GetGuestToken(), itemId, req, ct)))
            .WithValidation<UpdateCartItemRequest>();

        cart.MapDelete("/items/{itemId:guid}", async (Guid itemId, ICartService svc, HttpContext ctx, CancellationToken ct) =>
            Results.Ok(await svc.RemoveItemAsync(ctx.GetUserId(), ctx.GetGuestToken(), itemId, ct)));

        cart.MapDelete("/", async (ICartService svc, HttpContext ctx, CancellationToken ct) =>
        {
            await svc.ClearAsync(ctx.GetUserId(), ctx.GetGuestToken(), ct);
            return Results.NoContent();
        });

        cart.MapPost("/merge", async (MergeCartRequest req, ICartService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await svc.MergeAsync(userId.Value, req, ct));
        }).RequireAuthorization().WithValidation<MergeCartRequest>();

        return group;
    }
}
