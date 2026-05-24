using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Family : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Category> Categories { get; set; } = [];
}
