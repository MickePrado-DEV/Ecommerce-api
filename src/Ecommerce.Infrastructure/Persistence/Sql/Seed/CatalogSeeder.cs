using System.Text.Json;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Seed;

/// <summary>Taxonomía (FamilySeeder) y productos demo (EcommerceProductSeeder), sin imágenes ni covers.</summary>
internal static class CatalogSeeder
{
    private const string TaxonomyMarkerFamily = "Tecnología";

    private static readonly string[] Brands =
    [
        "Nova", "Prime", "Urban", "Atlas", "Lumen", "Kairo", "Vértice", "Nórdic",
        "Selva", "Andes", "Pacífico", "Cóndor", "Aurora", "Brisa", "Forge", "Pulse",
    ];

    private static readonly Dictionary<string, (decimal Min, decimal Max)> PriceRanges = new(StringComparer.Ordinal)
    {
        ["Tecnología"] = (199.90m, 8999.90m),
        ["Electrohogar"] = (299.90m, 5999.90m),
        ["Moda Hombre"] = (99.90m, 899.90m),
        ["Moda Mujer"] = (99.90m, 1299.90m),
        ["Belleza"] = (39.90m, 799.90m),
        ["Supermercado"] = (9.90m, 499.90m),
        ["Deportes"] = (49.90m, 1999.90m),
    };

    public static int ResolveProductCount()
    {
        var raw = Environment.GetEnvironmentVariable("SEED_PRODUCT_COUNT");
        if (!int.TryParse(raw, out var count) || count < 1)
            count = 50;

        var allowBelowMin = string.Equals(
            Environment.GetEnvironmentVariable("SEED_ALLOW_BELOW_MIN"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (!allowBelowMin && count < 1500)
            count = 50;

        return Math.Clamp(count, 1, 5000);
    }

    private static bool ShouldResetCatalog() =>
        string.Equals(
            Environment.GetEnvironmentVariable("SEED_RESET_CATALOG"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    public static async Task EnsureTaxonomyAsync(EcommerceDbContext db, CancellationToken ct = default)
    {
        if (ShouldResetCatalog())
            await PurgeCatalogAsync(db, ct);

        if (await db.Families.AnyAsync(f => f.Name == TaxonomyMarkerFamily, ct))
            return;

        var jsonPath = ResolveFamiliesJsonPath();
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"No se encontró el catálogo de familias: {jsonPath}");

        await using var stream = File.OpenRead(jsonPath);
        var families = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, string[]>>>(
            stream, cancellationToken: ct)
            ?? throw new InvalidOperationException("catalog-families.json vacío o inválido.");

        var familySlugs = new HashSet<string>(StringComparer.Ordinal);
        var familyOrder = 1;

        foreach (var (familyName, categories) in families)
        {
            var family = new Family
            {
                Name = familyName,
                Slug = SlugHelper.UniqueSlug(SlugHelper.ToSlug(familyName), familySlugs),
                SortOrder = familyOrder++,
                IsActive = true,
            };
            db.Families.Add(family);

            var categoryOrder = 1;
            foreach (var (categoryName, subcategories) in categories)
            {
                var category = new Category
                {
                    Family = family,
                    Name = categoryName,
                    Slug = SlugHelper.ToSlug($"{familyName}-{categoryName}"),
                    SortOrder = categoryOrder++,
                    IsActive = true,
                };
                db.Categories.Add(category);

                var subOrder = 1;
                foreach (var subName in subcategories)
                {
                    db.Subcategories.Add(new Subcategory
                    {
                        Category = category,
                        Name = subName,
                        Slug = SlugHelper.ToSlug($"{familyName}-{categoryName}-{subName}"),
                        SortOrder = subOrder++,
                        IsActive = true,
                    });
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureProductsAsync(EcommerceDbContext db, CancellationToken ct = default)
    {
        if (!await db.Families.AnyAsync(f => f.Name == TaxonomyMarkerFamily, ct))
            return;

        var target = ResolveProductCount();
        var current = await db.Products.CountAsync(ct);
        if (current >= target)
            return;

        var toCreate = target - current;
        var subcategories = await db.Subcategories
            .Include(s => s.Category).ThenInclude(c => c.Family)
            .OrderBy(s => s.Category.Family.SortOrder)
            .ThenBy(s => s.Category.SortOrder)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(ct);

        if (subcategories.Count == 0)
            return;

        var distribution = BuildDistribution(subcategories, toCreate);
        var subById = subcategories.ToDictionary(s => s.Id);
        var productSlugs = await db.Products.Select(p => p.Slug).ToListAsync(ct);
        var usedSlugs = new HashSet<string>(productSlugs, StringComparer.Ordinal);
        var usedSkus = await db.Variants.Select(v => v.Sku).ToListAsync(ct);
        var skuSet = new HashSet<string>(usedSkus, StringComparer.Ordinal);
        var random = Random.Shared;

        foreach (var (subcategoryId, count) in distribution)
        {
            if (count <= 0 || !subById.TryGetValue(subcategoryId, out var sub))
                continue;

            var familyName = sub.Category.Family.Name;
            var ctx = new SubcategorySeedContext(sub.Id, sub.Name, familyName, sub.Category.Name);

            for (var i = 0; i < count; i++)
                await CreateProductAsync(db, ctx, usedSlugs, skuSet, random, ct);
        }
    }

    private static async Task CreateProductAsync(
        EcommerceDbContext db,
        SubcategorySeedContext ctx,
        HashSet<string> usedSlugs,
        HashSet<string> usedSkus,
        Random random,
        CancellationToken ct)
    {
        var brand = Brands[random.Next(Brands.Length)];
        var model = $"{(char)random.Next('A', 'Z' + 1)}{(char)random.Next('A', 'Z' + 1)}-{random.Next(100, 1000)}";
        var name = $"{brand} {ctx.Name} {model}".Trim();
        var slug = SlugHelper.UniqueSlug($"{SlugHelper.ToSlug(name)}-{random.Next(100000, 999999)}", usedSlugs);
        var basePrice = RandomPrice(ctx.FamilyName, random);
        var description = BuildDescription(ctx);

        var product = new Product
        {
            SubcategoryId = ctx.Id,
            Name = name,
            Slug = slug,
            Description = description,
            BasePrice = basePrice,
            IsActive = true,
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        var templates = CatalogOptionTemplates.ResolveForFamily(ctx.FamilyName, random);
        if (templates.Count == 0)
        {
            await AddSimpleVariantAsync(db, product, basePrice, ctx.Id, usedSkus, random, ct);
            return;
        }

        var optionGroups = new List<List<OptionValue>>();
        var optionOrder = 1;
        foreach (var template in templates)
        {
            var productOption = new ProductOption
            {
                ProductId = product.Id,
                Name = template.Name,
                SortOrder = optionOrder++,
            };
            db.ProductOptions.Add(productOption);
            await db.SaveChangesAsync(ct);

            var values = new List<OptionValue>();
            var valueOrder = 1;
            foreach (var item in template.Values)
            {
                var display = template.Name == "Color" && item.Description is not null
                    ? item.Description
                    : item.Description ?? item.Value;
                var optionValue = new OptionValue
                {
                    ProductOptionId = productOption.Id,
                    Value = display,
                    SortOrder = valueOrder++,
                };
                db.OptionValues.Add(optionValue);
                values.Add(optionValue);
            }

            await db.SaveChangesAsync(ct);
            optionGroups.Add(values);
        }

        var comboIndex = 0;
        foreach (var combo in Cartesian(optionGroups))
        {
            comboIndex++;
            var price = basePrice + (comboIndex > 1 ? random.Next(0, 3) * 5m : 0m);
            var variant = new Variant
            {
                ProductId = product.Id,
                Sku = UniqueSku(ctx.Id, usedSkus),
                Price = price,
                IsActive = true,
            };
            db.Variants.Add(variant);
            await db.SaveChangesAsync(ct);

            foreach (var optionValue in combo)
            {
                db.VariantOptionValues.Add(new VariantOptionValue
                {
                    VariantId = variant.Id,
                    OptionValueId = optionValue.Id,
                });
            }

            db.Inventories.Add(new Inventory
            {
                VariantId = variant.Id,
                QuantityOnHand = random.Next(10, 401),
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task AddSimpleVariantAsync(
        EcommerceDbContext db,
        Product product,
        decimal price,
        Guid subcategoryId,
        HashSet<string> usedSkus,
        Random random,
        CancellationToken ct)
    {
        var variant = new Variant
        {
            ProductId = product.Id,
            Sku = UniqueSku(subcategoryId, usedSkus),
            Price = price,
            IsActive = true,
        };
        db.Variants.Add(variant);
        await db.SaveChangesAsync(ct);
        db.Inventories.Add(new Inventory
        {
            VariantId = variant.Id,
            QuantityOnHand = random.Next(10, 401),
        });
        await db.SaveChangesAsync(ct);
    }

    private static string BuildDescription(SubcategorySeedContext ctx) =>
        string.Join(
            "\n\n",
            $"{ctx.Name} de la línea {ctx.CategoryName} ({ctx.FamilyName}).",
            "Producto de demostración generado por el seed del catálogo.",
            "Características: calidad verificada, uso cotidiano, diseño actual.",
            "Garantía y envío según políticas de la tienda.");

    private static decimal RandomPrice(string familyName, Random random)
    {
        if (!PriceRanges.TryGetValue(familyName, out var range))
            range = (29.90m, 1499.90m);

        var value = (decimal)random.NextDouble() * (range.Max - range.Min) + range.Min;
        return Math.Round(value, 2);
    }

    private static Dictionary<Guid, int> BuildDistribution(List<Subcategory> subcategories, int total)
    {
        var distribution = new Dictionary<Guid, int>();
        var count = subcategories.Count;
        var baseAmount = total / Math.Max(1, count);
        var remainder = total % Math.Max(1, count);

        for (var i = 0; i < subcategories.Count; i++)
            distribution[subcategories[i].Id] = baseAmount + (i < remainder ? 1 : 0);

        return distribution;
    }

    private static string UniqueSku(Guid subcategoryId, HashSet<string> usedSkus)
    {
        string sku;
        do
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            sku = $"PRD-{subcategoryId.ToString("N")[..8]}-{suffix}";
        } while (!usedSkus.Add(sku));

        return sku;
    }

    private static IEnumerable<List<T>> Cartesian<T>(IReadOnlyList<List<T>> groups)
    {
        if (groups.Count == 0)
        {
            yield return [];
            yield break;
        }

        foreach (var head in groups[0])
        {
            if (groups.Count == 1)
            {
                yield return [head];
                continue;
            }

            foreach (var tail in Cartesian(groups.Skip(1).ToList()))
            {
                var row = new List<T>(tail.Count + 1) { head };
                row.AddRange(tail);
                yield return row;
            }
        }
    }

    private static async Task PurgeCatalogAsync(EcommerceDbContext db, CancellationToken ct)
    {
        await db.VariantOptionValues.ExecuteDeleteAsync(ct);
        await db.StockMovements.ExecuteDeleteAsync(ct);
        await db.Inventories.ExecuteDeleteAsync(ct);
        await db.Variants.ExecuteDeleteAsync(ct);
        await db.OptionValues.ExecuteDeleteAsync(ct);
        await db.ProductOptions.ExecuteDeleteAsync(ct);
        await db.ProductImages.ExecuteDeleteAsync(ct);
        await db.ProductReviews.ExecuteDeleteAsync(ct);
        await db.WishlistItems.ExecuteDeleteAsync(ct);
        await db.CartItems.ExecuteDeleteAsync(ct);
        await db.Products.ExecuteDeleteAsync(ct);
        await db.Subcategories.ExecuteDeleteAsync(ct);
        await db.Categories.ExecuteDeleteAsync(ct);
        await db.Families.ExecuteDeleteAsync(ct);
        await db.Covers.ExecuteDeleteAsync(ct);
    }

    private static string ResolveFamiliesJsonPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Seed", "catalog-families.json"),
            Path.Combine(AppContext.BaseDirectory, "catalog-families.json"),
            Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "Persistence", "Sql", "Seed", "catalog-families.json")),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        return candidates[0];
    }
}
