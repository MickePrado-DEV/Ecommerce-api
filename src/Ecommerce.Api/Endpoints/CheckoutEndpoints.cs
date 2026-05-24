using Ecommerce.Api.Filters;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.DTOs.Checkout;

namespace Ecommerce.Api.Endpoints;

public static class CheckoutEndpoints
{
    public static RouteGroupBuilder MapCheckoutEndpoints(this RouteGroupBuilder group)
    {
        var checkout = group.MapGroup("/checkout").WithTags("Checkout").RequireAuthorization();

        checkout.MapPost("/", async (CheckoutRequest req, ICheckoutService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await svc.CheckoutAsync(userId.Value, req, ct));
        }).WithValidation<CheckoutRequest>();

        return group;
    }
}
