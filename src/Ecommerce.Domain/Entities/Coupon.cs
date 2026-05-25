using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = null!;
    public CouponDiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public decimal? MinSubtotal { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
}
