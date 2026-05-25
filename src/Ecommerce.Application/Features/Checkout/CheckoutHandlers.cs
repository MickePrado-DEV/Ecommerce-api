// Checkout: CreateOrderCommand crea pedido, reserva stock y vacía carrito en transacción.
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Checkout;
using Ecommerce.Domain.Cart;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.Orders;
using Ecommerce.Domain.Services;
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
    decimal ShippingCost,
    string? CouponCode) : IRequest<Result<CheckoutResultDto>>;

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
        request.ShippingCost,
        request.CouponCode);
}

public class CreateOrderCommandHandler(
    ICartRepository carts,
    IOrderRepository orders,
    IInventoryRepository inventory,
    IAddressReadRepository addresses,
    ICouponRepository coupons,
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

        var couponResult = await ResolveCouponAsync(request.CouponCode, subtotal, ct);
        if (couponResult.IsFailed)
            return Result.Fail<CheckoutResultDto>(couponResult.Errors);

        var (discount, couponCode, couponId) = couponResult.Value;
        var total = subtotal - discount + request.ShippingCost;

        var order = new Order
        {
            OrderNumber = orders.GenerateOrderNumber(),
            UserId = request.UserId,
            Status = OrderStatus.PendingPayment,
            Subtotal = subtotal,
            DiscountAmount = discount,
            CouponCode = couponCode,
            ShippingCost = request.ShippingCost,
            Total = total,
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
            Payment = new Payment { Amount = total, Status = PaymentStatus.Pending }
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
            if (couponId.HasValue)
                await coupons.IncrementUsedAsync(couponId.Value, ct);
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

        return Result.Ok(new CheckoutResultDto(
            order.Id,
            order.OrderNumber,
            order.Subtotal,
            order.DiscountAmount,
            order.CouponCode,
            order.Total,
            order.Status.ToString()));
    }

    private async Task<Result<(decimal Discount, string? Code, Guid? CouponId)>> ResolveCouponAsync(
        string? couponCode, decimal subtotal, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return Result.Ok<(decimal, string?, Guid?)>((0, null, null));

        var normalized = couponCode.Trim().ToUpperInvariant();
        var coupon = await coupons.GetByCodeAsync(normalized, ct);
        if (coupon is null)
            return Result.Fail<(decimal, string?, Guid?)>(CouponErrors.NotFound(normalized));

        if (!CouponCalculator.IsValidFor(coupon, subtotal, DateTime.UtcNow))
            return Result.Fail<(decimal, string?, Guid?)>(CouponErrors.Invalid(normalized, "expirado, inactivo o subtotal insuficiente"));

        var discount = CouponCalculator.ComputeDiscount(coupon, subtotal);
        return Result.Ok<(decimal, string?, Guid?)>((discount, coupon.Code, coupon.Id));
    }

    private async Task<Result<OrderAddress>> BuildOrderAddressAsync(CreateOrderCommand request, CancellationToken ct)
    {
        if (request.AddressId.HasValue)
        {
            var address = await addresses.GetByIdAsync(request.AddressId.Value, request.UserId, ct);
            if (address is null)
                return Result.Fail<OrderAddress>(OrderErrors.AddressNotFound(request.AddressId.Value));

            var streetLine = string.Join(" ", new[]
            {
                address.Street,
                address.ExternalNumber,
                string.IsNullOrWhiteSpace(address.InternalNumber) ? null : $"Int. {address.InternalNumber}"
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            return Result.Ok(new OrderAddress
            {
                FullName = address.ContactName ?? address.Label,
                Street = streetLine,
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
