namespace Ecommerce.Domain.Exceptions;

public class InsufficientStockException(Guid variantId) : Exception($"Stock insuficiente para la variante {variantId}")
{
    public Guid VariantId { get; } = variantId;
}
