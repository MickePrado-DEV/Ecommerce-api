// Checkout: crea pedido desde carrito (reserva stock). Requiere usuario autenticado.
using Ecommerce.Api.Extensions;
using Ecommerce.Application.DTOs.Checkout;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Orders;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class CheckoutEndpoints
{
    public static RouteGroupBuilder MapCheckoutEndpoints(this RouteGroupBuilder group)
    {
        var checkout = group.MapGroup("/checkout").WithTags("Checkout").RequireAuthorization();

        // Crea Order en PendingPayment, reserva stock y vacía carrito (transacción)
        checkout.MapPost("/", async (CheckoutRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            var cmd = CreateOrderCommandMapping.FromRequest(userId.Value, req);
            return (await sender.Send(cmd, ct)).ToHttpResult();
        });

        // Pago simulado desde checkout (misma lógica que POST /orders/{id}/pay)
        checkout.MapPost("/{orderId:guid}/pay", async (Guid orderId, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new PayOrderCommand(userId.Value, orderId), ct)).ToHttpResult();
        });

        return group;
    }
}
