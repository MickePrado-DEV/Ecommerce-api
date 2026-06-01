using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class OptionValue : BaseEntity
{
    public Guid ProductOptionId { get; set; }
    public string Value { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ProductOption ProductOption { get; set; } = null!;
}
