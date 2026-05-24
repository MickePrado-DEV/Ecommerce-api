using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities
{
    public class Variant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = null!;
        public decimal? Price { get; set; }
        public bool IsActive { get; set; } = true;

        public Product Product { get; set; } = null!;
        public Inventory? Inventory { get; set; }
    }
}
