using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Services;

public static class CouponCalculator
{
    public static decimal ComputeDiscount(Coupon coupon, decimal subtotal)
    {
        if (subtotal <= 0) return 0;
        return coupon.DiscountType switch
        {
            CouponDiscountType.Percent => Math.Round(subtotal * (coupon.Value / 100m), 2),
            CouponDiscountType.FixedAmount => Math.Min(coupon.Value, subtotal),
            _ => 0
        };
    }

    public static bool IsValidFor(Coupon coupon, decimal subtotal, DateTime utcNow)
    {
        if (!coupon.IsActive) return false;
        if (coupon.ValidFrom.HasValue && utcNow < coupon.ValidFrom.Value) return false;
        if (coupon.ValidUntil.HasValue && utcNow > coupon.ValidUntil.Value) return false;
        if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value) return false;
        if (coupon.MinSubtotal.HasValue && subtotal < coupon.MinSubtotal.Value) return false;
        return true;
    }
}
