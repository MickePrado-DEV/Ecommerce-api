namespace Ecommerce.Domain.Entities;

/// <summary>Vincula una variante con los valores de opción elegidos (ej. Color=Negro).</summary>
public class VariantOptionValue
{
    public Guid VariantId { get; set; }
    public Guid OptionValueId { get; set; }
    public Variant Variant { get; set; } = null!;
    public OptionValue OptionValue { get; set; } = null!;
}
