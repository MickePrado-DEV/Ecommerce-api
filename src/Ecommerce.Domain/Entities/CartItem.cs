using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities
{

    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public Cart Cart { get; set; } = null!;
        public Variant Variant { get; set; } = null!;
    }
}
