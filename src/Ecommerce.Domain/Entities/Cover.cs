using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Cover : BaseEntity
{
    public string Title { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? LinkUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
