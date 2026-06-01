using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid SubcategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; } = true;

        public Subcategory Subcategory { get; set; } = null!;
        public ICollection<Variant> Variants { get; set; } = [];
        public ICollection<ProductImage> Images { get; set; } = [];
        public ICollection<ProductReview> Reviews { get; set; } = [];
        public ICollection<ProductOptionAssignment> OptionAssignments { get; set; } = [];
    }
}
