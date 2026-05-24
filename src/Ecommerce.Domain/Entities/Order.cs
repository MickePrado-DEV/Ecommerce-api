using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = null!;
        public Guid UserId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public ICollection<OrderItem> Items { get; set; } = [];
    }
}
