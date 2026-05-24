namespace Ecommerce.Application.DTOs.Cart;

public record AddCartItemRequest(Guid VariantId, int Quantity);