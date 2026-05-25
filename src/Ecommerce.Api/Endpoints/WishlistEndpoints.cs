using Ecommerce.Api.Extensions;
using Ecommerce.Application.Features.Wishlist;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class WishlistEndpoints
{
    public static RouteGroupBuilder MapWishlistEndpoints(this RouteGroupBuilder group)
    {
        var wishlist = group.MapGroup("/wishlist").WithTags("Wishlist").RequireAuthorization();

        wishlist.MapGet("/", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new GetWishlistQuery(userId.Value), ct)).ToHttpResult();
        });

        wishlist.MapPost("/{productId:guid}", async (Guid productId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new AddToWishlistCommand(userId.Value, productId), ct)).ToHttpResult();
        });

        wishlist.MapDelete("/{productId:guid}", async (Guid productId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new RemoveFromWishlistCommand(userId.Value, productId), ct)).ToHttpResult();
        });

        return group;
    }
}
