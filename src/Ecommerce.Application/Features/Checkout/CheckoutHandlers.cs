using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Checkout;
using Ecommerce.Domain.Cart;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.Orders;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Checkout;

public record CreateOrderCommand(
    Guid UserId,
    Guid? AddressId,
    string? FullName,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? Phone,
    decimal ShippingCost) : IRequest<Result<CheckoutResultDto>>;

public static class CreateOrderCommandMapping
{
    public static CreateOrderCommand FromRequest(Guid userId, CheckoutRequest request) => new(
        userId,
        request.AddressId,
        request.FullName,
        request.Street,
        request.City,
        request.State,
        request.PostalCode,
        request.Country,
        request.Phone,
        request.ShippingCost);
}

public class CreateOrderCommandHandler(
    ICartRepository carts,
    IOrderRepository orders,
    IInventoryRepository inventory,
    IAddressReadRepository addresses,
    IUnitOfWork uow) : IRequestHandler<CreateOrderCommand, Result<CheckoutResultDto>>
{
    public async Task<Result<CheckoutResultDto>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var cart = await carts.GetOrCreateAsync(request.UserId, null, ct);
        if (cart.Items.Count == 0)
            return Result.Fail<CheckoutResultDto>(CartErrors.EmptyCart());

        var subtotal = cart.Items.Sum(i =>
        {
            var price = i.Variant.Price ?? i.Variant.Product.BasePrice;
            return price * i.Quantity;
        });

        var orderAddressResult = await BuildOrderAddressAsync(request, ct);
        if (orderAddressResult.IsFailed)
            return Result.Fail<CheckoutResultDto>(orderAddressResult.Errors);

        var order = new Order
        {
            OrderNumber = orders.GenerateOrderNumber(),
            UserId = request.UserId,
            Status = OrderStatus.PendingPayment,
            Subtotal = subtotal,
            ShippingCost = request.ShippingCost,
            Total = subtotal + request.ShippingCost,
            Address = orderAddressResult.Value,
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
        catch (InsufficientStockException ex)
        {
            await uow.RollbackAsync(ct);
            return Result.Fail<CheckoutResultDto>(OrderErrors.InsufficientStock(ex.VariantId));
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }

        return Result.Ok(new CheckoutResultDto(order.Id, order.OrderNumber, order.Total, order.Status.ToString()));
    }

    private async Task<Result<OrderAddress>> BuildOrderAddressAsync(CreateOrderCommand request, CancellationToken ct)
    {
        if (request.AddressId.HasValue)
        {
            var address = await addresses.GetByIdAsync(request.AddressId.Value, request.UserId, ct);
            if (address is null)
                return Result.Fail<OrderAddress>(OrderErrors.AddressNotFound(request.AddressId.Value));

            return Result.Ok(new OrderAddress
            {
                FullName = address.Label,
                Street = address.Street,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                Phone = address.Phone
            });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result.Fail<OrderAddress>(OrderErrors.MissingShippingAddress());

        return Result.Ok(new OrderAddress
        {
            FullName = request.FullName!,
            Street = request.Street!,
            City = request.City!,
            State = request.State!,
            PostalCode = request.PostalCode!,
            Country = request.Country!,
            Phone = request.Phone!
        });
    }

}
