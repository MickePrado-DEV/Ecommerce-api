namespace Ecommerce.Domain.Entities
{
    public class Inventory
    {
        public Guid VariantId { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public Variant Variant { get; set; } = null!;
    }
}
