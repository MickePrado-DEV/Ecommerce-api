using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class AdminOrderService(IOrderRepository orders, IShipmentRepository shipments, IPdfTicketGenerator pdf) : IAdminOrderService
{
    public async Task<PagedOrdersAdminDto> ListAsync(int page, int pageSize, OrderStatus? status, CancellationToken ct = default)
    {
        var result = await orders.ListAdminAsync(page, pageSize, status, ct);
        return new PagedOrdersAdminDto(
            result.Items.Select(o => new OrderSummaryDto(o.Id, o.OrderNumber, o.Status.ToString(), o.Total, o.CreatedAt)).ToList(),
            result.Total, page, pageSize);
    }

    public async Task<OrderDetailDto?> GetAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orders.GetByIdAsync(orderId, ct);
        return order is null ? null : OrderServiceMap.MapDetail(order);
    }

    public async Task MarkReadyToDispatchAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orders.GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException("Order", orderId);
        if (order.Status != OrderStatus.Paid)
            throw new InvalidOperationException("La orden debe estar pagada");
        await orders.UpdateStatusAsync(orderId, OrderStatus.ReadyToDispatch, ct);
    }

    public async Task<byte[]> GenerateTicketPdfByOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var shipment = await shipments.GetByOrderIdAsync(orderId, ct)
            ?? throw new NotFoundException("Shipment", orderId);
        return pdf.GenerateDispatchTicket(shipment);
    }
}

internal static class OrderServiceMap
{
    public static OrderDetailDto MapDetail(Domain.Entities.Order order) => new(
        order.Id, order.OrderNumber, order.Status.ToString(),
        order.Subtotal, order.ShippingCost, order.Total, order.CreatedAt,
        order.Items.Select(i => new OrderItemDto(i.ProductName, i.Sku, i.Quantity, i.UnitPrice, i.LineTotal)).ToList(),
        order.Address is null ? null : new OrderAddressDto(
            order.Address.FullName, order.Address.Street, order.Address.City,
            order.Address.State, order.Address.PostalCode, order.Address.Country, order.Address.Phone),
        order.Payment is null ? null : new PaymentInfoDto(
            order.Payment.Status.ToString(), order.Payment.Amount, order.Payment.PaidAt));
}
