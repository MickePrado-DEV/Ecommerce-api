using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface ICartRepository
{
    Task<Cart> GetOrCreateAsync(Guid? userId, Guid? guestToken, CancellationToken ct = default);
    Task<Cart?> GetWithItemsAsync(Guid cartId, CancellationToken ct = default);
    Task<Variant?> GetVariantAsync(Guid variantId, CancellationToken ct = default);
    Task ClearAsync(Guid cartId, CancellationToken ct = default);
    Task MergeGuestIntoUserAsync(Guid userId, Guid guestToken, CancellationToken ct = default);
    Task DeleteCartAsync(Guid cartId, CancellationToken ct = default);
}
