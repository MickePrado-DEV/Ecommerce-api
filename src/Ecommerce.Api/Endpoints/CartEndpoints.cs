// Carrito: usuario (JWT) o invitado (header X-Guest-Token). Merge requiere login.
using Ecommerce.Api.Extensions;
using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Application.Features.Cart;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class CartEndpoints
{
    public static RouteGroupBuilder MapCartEndpoints(this RouteGroupBuilder group)
    {
        var cart = group.MapGroup("/cart").WithTags("Cart");

        cart.MapGet("/", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
            (await sender.Send(new GetCartQuery(ctx.GetUserId(), ctx.GetGuestToken()), ct)).ToHttpResult());

        cart.MapPost("/items", async (AddCartItemRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
            (await sender.Send(new AddCartItemCommand(ctx.GetUserId(), ctx.GetGuestToken(), req.VariantId, req.Quantity), ct)).ToHttpResult());

        cart.MapPut("/items/{itemId:guid}", async (Guid itemId, UpdateCartItemRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
            (await sender.Send(new UpdateCartItemCommand(ctx.GetUserId(), ctx.GetGuestToken(), itemId, req.Quantity), ct)).ToHttpResult());

        cart.MapPatch("/items/{itemId:guid}", async (Guid itemId, UpdateCartItemRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
            (await sender.Send(new UpdateCartItemCommand(ctx.GetUserId(), ctx.GetGuestToken(), itemId, req.Quantity), ct)).ToHttpResult());

        cart.MapDelete("/items/{itemId:guid}", async (Guid itemId, ISender sender, HttpContext ctx, CancellationToken ct) =>
            (await sender.Send(new RemoveCartItemCommand(ctx.GetUserId(), ctx.GetGuestToken(), itemId), ct)).ToHttpResult());

        cart.MapDelete("/", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
            (await sender.Send(new ClearCartCommand(ctx.GetUserId(), ctx.GetGuestToken()), ct)).ToHttpResult());

        // Tras login: fusiona carrito invitado al carrito del usuario
        cart.MapPost("/merge", async (MergeCartRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new MergeCartCommand(userId.Value, req.GuestToken), ct)).ToHttpResult();
        }).RequireAuthorization();

        return group;
    }
}
