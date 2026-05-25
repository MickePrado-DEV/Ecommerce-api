using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Wishlist;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class WishlistRepository(EcommerceDbContext db) : IWishlistRepository
{
    public async Task<IReadOnlyList<WishlistItemDto>> ListByUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.WishlistItems.AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WishlistItemDto(
                w.ProductId,
                w.Product.Name,
                w.Product.Slug,
                w.Product.BasePrice,
                w.Product.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()
                    ?? w.Product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                w.CreatedAt))
            .ToListAsync(ct);

    public Task<WishlistItem?> GetAsync(Guid userId, Guid productId, CancellationToken ct = default) =>
        db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, ct);

    public async Task AddAsync(WishlistItem item, CancellationToken ct = default)
    {
        db.WishlistItems.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(WishlistItem item, CancellationToken ct = default)
    {
        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct = default) =>
        db.Products.AnyAsync(p => p.Id == productId && p.IsActive, ct);
}
