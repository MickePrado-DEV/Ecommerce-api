using Ecommerce.Application.DTOs.Reviews;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IProductReviewRepository
{
    Task<IReadOnlyList<ProductReviewDto>> ListApprovedByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<ProductReviewSummaryDto?> GetSummaryByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<Product?> GetActiveProductBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> UserHasReviewedAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task<bool> UserHasDeliveredProductAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task AddAsync(ProductReview review, CancellationToken ct = default);
}
