namespace Ecommerce.Application.DTOs.Wishlist;

public record WishlistItemDto(Guid ProductId, string Name, string Slug, decimal Price, string? PrimaryImage, DateTime AddedAt);
