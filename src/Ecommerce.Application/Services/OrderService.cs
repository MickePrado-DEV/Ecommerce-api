using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class OrderService(IOrderRepository orders, IInventoryRepository inventory, IUnitOfWork uow) : IOrderService
{
    public async Task<OrderDetailDto?> GetAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await orders.GetByIdForUserAsync(orderId, userId, ct);
        return order is null ? null : MapDetail(order);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> ListMineAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await orders.ListByUserAsync(userId, ct);
        return list.Select(o => new OrderSummaryDto(o.Id, o.OrderNumber, o.Status.ToString(), o.Total, o.CreatedAt)).ToList();
    }

    public async Task<PaymentResultDto> PayMockAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await orders.GetByIdForUserAsync(orderId, userId, ct)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status != OrderStatus.PendingPayment)
            throw new InvalidOperationException("La orden no está pendiente de pago");

        await uow.BeginTransactionAsync(ct);
        try
        {
            order.Payment!.Status = PaymentStatus.Approved;
            order.Payment.PaidAt = DateTime.UtcNow;
            order.Payment.ProviderReference = $"MOCK-{Guid.NewGuid():N}";
            order.Status = OrderStatus.Paid;
            await inventory.CommitReservationAsync(order.Id, ct);
            await uow.CommitAsync(ct);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }

        return new PaymentResultDto(order.Id, order.Status.ToString(), order.Payment.ProviderReference);
    }

    private static OrderDetailDto MapDetail(Domain.Entities.Order order) => new(
        order.Id, order.OrderNumber, order.Status.ToString(),
        order.Subtotal, order.ShippingCost, order.Total, order.CreatedAt,
        order.Items.Select(i => new OrderItemDto(i.ProductName, i.Sku, i.Quantity, i.UnitPrice, i.LineTotal)).ToList(),
        order.Address is null ? null : new OrderAddressDto(
            order.Address.FullName, order.Address.Street, order.Address.City,
            order.Address.State, order.Address.PostalCode, order.Address.Country, order.Address.Phone),
        order.Payment is null ? null : new PaymentInfoDto(
            order.Payment.Status.ToString(), order.Payment.Amount, order.Payment.PaidAt));
}
