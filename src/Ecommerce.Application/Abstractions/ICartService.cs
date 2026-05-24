using Ecommerce.Application.DTOs.Cart;

namespace Ecommerce.Application.Abstractions;

public interface ICartService
{
    Task<CartDto> GetAsync(Guid? userId, Guid? guestToken, CancellationToken ct = default);
    Task<CartDto> AddItemAsync(Guid? userId, Guid? guestToken, AddCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateItemAsync(Guid? userId, Guid? guestToken, Guid itemId, UpdateCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(Guid? userId, Guid? guestToken, Guid itemId, CancellationToken ct = default);
    Task<CartDto> MergeAsync(Guid userId, MergeCartRequest request, CancellationToken ct = default);
    Task ClearAsync(Guid? userId, Guid? guestToken, CancellationToken ct = default);
}
