using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Reviews;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class ProductReviewRepository(EcommerceDbContext db) : IProductReviewRepository
{
    public async Task<IReadOnlyList<ProductReviewDto>> ListApprovedByProductIdAsync(Guid productId, CancellationToken ct = default) =>
        await db.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ProductReviewDto(
                r.Id,
                r.User.FirstName + " " + r.User.LastName,
                r.Rating,
                r.Title,
                r.Comment,
                r.CreatedAt))
            .ToListAsync(ct);

    public async Task<ProductReviewSummaryDto?> GetSummaryByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        var approved = db.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved);
        var count = await approved.CountAsync(ct);
        if (count == 0) return new ProductReviewSummaryDto(0, 0);
        var avg = await approved.AverageAsync(r => (double)r.Rating, ct);
        return new ProductReviewSummaryDto(Math.Round(avg, 1), count);
    }

    public Task<Product?> GetActiveProductBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, ct);

    public Task<bool> UserHasReviewedAsync(Guid userId, Guid productId, CancellationToken ct = default) =>
        db.ProductReviews.AnyAsync(r => r.UserId == userId && r.ProductId == productId, ct);

    public Task<bool> UserHasDeliveredProductAsync(Guid userId, Guid productId, CancellationToken ct = default) =>
        db.Orders.AsNoTracking()
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
            .AnyAsync(o => o.Items.Any(i =>
                db.Variants.Any(v => v.Id == i.VariantId && v.ProductId == productId)), ct);

    public async Task AddAsync(ProductReview review, CancellationToken ct = default)
    {
        db.ProductReviews.Add(review);
        await db.SaveChangesAsync(ct);
    }
}
