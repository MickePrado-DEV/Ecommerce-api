using Ecommerce.Application.DTOs.Wishlist;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IWishlistRepository
{
    Task<IReadOnlyList<WishlistItemDto>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<WishlistItem?> GetAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task AddAsync(WishlistItem item, CancellationToken ct = default);
    Task RemoveAsync(WishlistItem item, CancellationToken ct = default);
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct = default);
}
