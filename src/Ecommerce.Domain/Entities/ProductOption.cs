using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class ProductOption : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    /// <summary>1 = texto, 2 = color (hex).</summary>
    public int OptionType { get; set; } = 1;
    public int SortOrder { get; set; }
    public Product Product { get; set; } = null!;
    public ICollection<OptionValue> Values { get; set; } = [];
}
