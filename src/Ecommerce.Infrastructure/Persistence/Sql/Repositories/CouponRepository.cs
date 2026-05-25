using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class CouponRepository(EcommerceDbContext db) : ICouponRepository
{
    public Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        db.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);

    public async Task IncrementUsedAsync(Guid couponId, CancellationToken ct = default)
    {
        var coupon = await db.Coupons.FirstAsync(c => c.Id == couponId, ct);
        coupon.UsedCount++;
        await db.SaveChangesAsync(ct);
    }
}
