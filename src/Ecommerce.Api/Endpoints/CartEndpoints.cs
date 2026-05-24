using Ecommerce.Application.DTOs.Cart;

namespace Ecommerce.Api.Endpoints
{
    public static class CartEndpoints
    {
        public static RouteGroupBuilder MapCartEndpoints(this RouteGroupBuilder group)
        {
            var cart = group.MapGroup("/cart").WithTags("Cart");

            cart.MapGet("/", async (ICartService svc, HttpContext ctx, CancellationToken ct) =>
                Results.Ok(await svc.GetAsync(ctx.GetUserId(), ctx.GetGuestToken(), ct)));

            cart.MapPost("/items", async (AddCartItemRequest req, ICartService svc, HttpContext ctx, CancellationToken ct) =>
                Results.Ok(await svc.AddItemAsync(ctx.GetUserId(), ctx.GetGuestToken(), req, ct)));

            return group;
        }
    }
}
