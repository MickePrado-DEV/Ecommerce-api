namespace Ecommerce.Application.DTOs.Cart;

public record CartDto(Guid Id, Guid? GuestToken, IReadOnlyList<CartItemDto> Items, decimal Subtotal);
public record CartItemDto(Guid Id, Guid VariantId, string ProductName, string Sku, int Quantity, decimal UnitPrice, decimal LineTotal);
public record UpdateCartItemRequest(int Quantity);
