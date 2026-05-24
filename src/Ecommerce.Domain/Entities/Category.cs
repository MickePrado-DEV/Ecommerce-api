using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Category : BaseEntity
{
    public Guid FamilyId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Family Family { get; set; } = null!;
    public ICollection<Subcategory> Subcategories { get; set; } = [];
}
