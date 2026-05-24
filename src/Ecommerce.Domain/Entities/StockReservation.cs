using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class StockReservation : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Order Order { get; set; } = null!;
}
