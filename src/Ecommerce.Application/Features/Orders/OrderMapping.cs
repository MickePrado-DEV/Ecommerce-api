using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Orders;

public static class OrderMapping
{
    public static OrderItemDto MapItem(
        OrderItem item,
        Guid? productId = null,
        string? productSlug = null) =>
        new(item.ProductName, item.Sku, item.Quantity, item.UnitPrice, item.LineTotal, productId, productSlug);

    public static OrderDetailDto ToDetail(
        Order order,
        IReadOnlyDictionary<Guid, (Guid ProductId, string Slug)>? variantProducts = null) => new(
        order.Id, order.OrderNumber, order.Status.ToString(),
        order.Subtotal, order.ShippingCost, order.Total, order.CreatedAt,
        order.Items.Select(i =>
        {
            if (variantProducts is not null && variantProducts.TryGetValue(i.VariantId, out var vp))
                return MapItem(i, vp.ProductId, vp.Slug);
            return MapItem(i);
        }).ToList(),
        order.Address is null ? null : new OrderAddressDto(
            order.Address.FullName, order.Address.Street, order.Address.City,
            order.Address.State, order.Address.PostalCode, order.Address.Country, order.Address.Phone),
        order.Payment is null ? null : new PaymentInfoDto(
            order.Payment.Status.ToString(), order.Payment.Amount, order.Payment.PaidAt),
        order.Shipment is null ? null : new OrderShipmentInfoDto(
            order.Shipment.Id,
            order.Shipment.Status.ToString(),
            order.Shipment.TrackingNumber,
            order.Shipment.Driver?.Name,
            order.Shipment.ShippedAt));
}
