namespace Ecommerce.Domain.Entities;

/// <summary>
/// Vincula un producto con una opción global y el subconjunto de valores habilitados (como option_product en Laravel).
/// </summary>
public class ProductOptionAssignment
{
    public Guid ProductId { get; set; }
    public Guid ProductOptionId { get; set; }
    /// <summary>JSON: [{ "id", "value", "description" }, ...]</summary>
    public string FeaturesJson { get; set; } = "[]";

    public Product Product { get; set; } = null!;
    public ProductOption ProductOption { get; set; } = null!;
}
