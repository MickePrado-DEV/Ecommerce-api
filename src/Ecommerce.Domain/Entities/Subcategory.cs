using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Subcategory : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Category Category { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = [];
}
