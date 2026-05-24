using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities;

public class StockMovement : BaseEntity
{
    public Guid VariantId { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public Variant Variant { get; set; } = null!;
}
