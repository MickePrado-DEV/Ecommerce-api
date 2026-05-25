namespace Ecommerce.Infrastructure.Persistence.Sql.Seed;

/// <summary>Plantillas globales equivalentes a OptionSeeder (Laravel).</summary>
internal static class CatalogOptionTemplates
{
    public static readonly IReadOnlyList<OptionTemplate> All =
    [
        new("Talla",
        [
            new("s", "small"),
            new("m", "medium"),
            new("l", "large"),
            new("xl", "extra large"),
            new("xxl", "extra extra large"),
        ]),
        new("Color",
        [
            new("#000000", "black"),
            new("#FFFFFF", "white"),
            new("#00FF00", "green"),
            new("#FF0000", "red"),
            new("#0000FF", "blue"),
            new("#FFFF00", "yellow"),
            new("#FF00FF", "magenta"),
            new("#00FFFF", "cyan"),
            new("#808080", "gray"),
        ]),
        new("Sexo",
        [
            new("masculino", "Masculino"),
            new("femenino", "Femenino"),
        ]),
    ];

    private static readonly HashSet<string> FashionFamilies =
    [
        "Moda Hombre",
        "Moda Mujer",
        "Moda Infantil",
        "Deportes",
    ];

    public static IReadOnlyList<OptionTemplate> ResolveForFamily(string familyName, Random random)
    {
        var talla = All.First(t => t.Name == "Talla");
        var color = All.First(t => t.Name == "Color");
        var sexo = All.First(t => t.Name == "Sexo");

        if (FashionFamilies.Contains(familyName))
        {
            var list = new List<OptionTemplate> { talla, color };
            if (random.Next(100) < 25)
                list.Add(sexo);
            return list;
        }

        if (random.Next(100) >= 35)
            return [];

        if (random.Next(100) < 70)
            return [color];

        if (random.Next(100) < 40)
            return [talla];

        return [];
    }
}
