namespace Ecommerce.Domain.Exceptions
{
    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(Guid variantId) : base($"Stock insuficiente para la variante {variantId}")
        {

        }
    }
}
