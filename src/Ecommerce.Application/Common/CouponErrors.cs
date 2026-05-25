using FluentResults;

namespace Ecommerce.Application.Common;

public static class CouponErrors
{
    public static Error NotFound(string code) =>
        new Error($"Cupón '{code}' no encontrado").WithMetadata("Code", "Coupon.NotFound");

    public static Error Invalid(string code, string reason) =>
        new Error($"Cupón '{code}' no válido: {reason}").WithMetadata("Code", "Coupon.Invalid");
}
