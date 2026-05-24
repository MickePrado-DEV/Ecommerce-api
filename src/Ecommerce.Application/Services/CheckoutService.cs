using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Checkout;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class CheckoutService(
    ICartRepository carts,
    IOrderRepository orders,
    IInventoryRepository inventory,
    IUnitOfWork uow) : ICheckoutService
{
    public async Task<CheckoutResultDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken ct = default)
    {
        var cart = await carts.GetOrCreateAsync(userId, null, ct);
        if (cart.Items.Count == 0)
            throw new InvalidOperationException("El carrito está vacío");

        var subtotal = cart.Items.Sum(i =>
        {
            var price = i.Variant.Price ?? i.Variant.Product.BasePrice;
            return price * i.Quantity;
        });

        var order = new Order
        {
            OrderNumber = orders.GenerateOrderNumber(),
            UserId = userId,
            Status = OrderStatus.PendingPayment,
            Subtotal = subtotal,
            ShippingCost = request.ShippingCost,
            Total = subtotal + request.ShippingCost,
            Address = new OrderAddress
            {
                FullName = request.FullName,
                Street = request.Street,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                Phone = request.Phone
            },
            Items = cart.Items.Select(i => new OrderItem
            {
                VariantId = i.VariantId,
                ProductName = i.Variant.Product.Name,
                Sku = i.Variant.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.Variant.Price ?? i.Variant.Product.BasePrice,
                LineTotal = (i.Variant.Price ?? i.Variant.Product.BasePrice) * i.Quantity
            }).ToList(),
            Payment = new Payment { Amount = subtotal + request.ShippingCost, Status = PaymentStatus.Pending }
        };

        await uow.BeginTransactionAsync(ct);
        try
        {
            await orders.AddAsync(order, ct);
            await uow.SaveChangesAsync(ct);
            await inventory.ReserveAsync(order.Id,
                order.Items.Select(i => (i.VariantId, i.Quantity)),
                DateTime.UtcNow.AddMinutes(30), ct);
            await carts.ClearAsync(cart.Id, ct);
            await uow.CommitAsync(ct);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }

        return new CheckoutResultDto(order.Id, order.OrderNumber, order.Total, order.Status.ToString());
    }
}
