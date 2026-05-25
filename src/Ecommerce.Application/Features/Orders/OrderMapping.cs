using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Orders;

public static class OrderMapping
{
    public static OrderDetailDto ToDetail(Order order) => new(
        order.Id, order.OrderNumber, order.Status.ToString(),
        order.Subtotal, order.ShippingCost, order.Total, order.CreatedAt,
        order.Items.Select(i => new OrderItemDto(i.ProductName, i.Sku, i.Quantity, i.UnitPrice, i.LineTotal)).ToList(),
        order.Address is null ? null : new OrderAddressDto(
            order.Address.FullName, order.Address.Street, order.Address.City,
            order.Address.State, order.Address.PostalCode, order.Address.Country, order.Address.Phone),
        order.Payment is null ? null : new PaymentInfoDto(
            order.Payment.Status.ToString(), order.Payment.Amount, order.Payment.PaidAt));
}
