using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class ProductOption : BaseEntity
{
    public string Name { get; set; } = null!;
    /// <summary>1 = texto/etiquetas, 2 = color (hex).</summary>
    public int OptionType { get; set; } = 1;
    public int SortOrder { get; set; }
    public ICollection<OptionValue> Values { get; set; } = [];
    public ICollection<ProductOptionAssignment> ProductAssignments { get; set; } = [];
}
