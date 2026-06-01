using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = null!;
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public DispatchStatus DispatchStatus { get; set; } = DispatchStatus.Pending;
    public DateTime? ReadyAt { get; set; }
    public DateTime? BatchedAt { get; set; }
    public DateTime? RoutedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? DispatchDeliveredAt { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public OrderAddress? Address { get; set; }
    public Payment? Payment { get; set; }
    public ICollection<StockReservation> StockReservations { get; set; } = [];
    public Shipment? Shipment { get; set; }
}
