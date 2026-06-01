using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class ProductOptionRepository(EcommerceDbContext db) : IProductOptionRepository
{
    public Task<List<ProductOption>> ListAllAsync(CancellationToken ct = default) =>
        db.ProductOptions.AsNoTracking()
            .Include(o => o.Values)
            .OrderBy(o => o.SortOrder).ThenBy(o => o.Name)
            .ToListAsync(ct);

    public Task<ProductOption?> GetByIdAsync(Guid optionId, CancellationToken ct = default) =>
        db.ProductOptions.Include(o => o.Values)
            .FirstOrDefaultAsync(o => o.Id == optionId, ct);

    public async Task<ProductOption> SaveOptionAsync(ProductOption option, CancellationToken ct = default)
    {
        if (option.Id == Guid.Empty) db.ProductOptions.Add(option);
        else db.ProductOptions.Update(option);
        await db.SaveChangesAsync(ct);
        return option;
    }

    public async Task DeleteOptionAsync(Guid optionId, CancellationToken ct = default)
    {
        var option = await db.ProductOptions.FirstOrDefaultAsync(o => o.Id == optionId, ct);
        if (option is not null) db.ProductOptions.Remove(option);
        await db.SaveChangesAsync(ct);
    }

    public async Task<OptionValue> SaveValueAsync(OptionValue value, CancellationToken ct = default)
    {
        if (value.Id == Guid.Empty) db.OptionValues.Add(value);
        else db.OptionValues.Update(value);
        await db.SaveChangesAsync(ct);
        return value;
    }

    public async Task DeleteValueAsync(Guid valueId, Guid optionId, CancellationToken ct = default)
    {
        var value = await db.OptionValues.FirstOrDefaultAsync(v => v.Id == valueId && v.ProductOptionId == optionId, ct);
        if (value is not null) db.OptionValues.Remove(value);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> HasAssignmentsAsync(Guid optionId, CancellationToken ct = default) =>
        db.ProductOptionAssignments.AnyAsync(a => a.ProductOptionId == optionId, ct);

    public Task<List<ProductOptionAssignment>> ListAssignmentsAsync(Guid productId, CancellationToken ct = default) =>
        db.ProductOptionAssignments.AsNoTracking()
            .Include(a => a.ProductOption).ThenInclude(o => o.Values)
            .Where(a => a.ProductId == productId)
            .OrderBy(a => a.ProductOption.SortOrder).ThenBy(a => a.ProductOption.Name)
            .ToListAsync(ct);

    public async Task AttachOptionAsync(Guid productId, Guid optionId, IReadOnlyList<Guid> valueIds, CancellationToken ct = default)
    {
        var option = await db.ProductOptions.Include(o => o.Values)
            .FirstOrDefaultAsync(o => o.Id == optionId, ct)
            ?? throw new InvalidOperationException("Option not found");

        var selected = option.Values
            .Where(v => valueIds.Contains(v.Id))
            .OrderBy(v => v.SortOrder)
            .Select(v => new OptionFeatureSnapshot(v.Id, v.Value, v.Description))
            .ToList();

        if (selected.Count == 0)
            throw new InvalidOperationException("At least one value is required");

        var existing = await db.ProductOptionAssignments
            .FirstOrDefaultAsync(a => a.ProductId == productId && a.ProductOptionId == optionId, ct);

        if (existing is not null)
        {
            existing.FeaturesJson = OptionFeatureJson.Serialize(selected);
            db.ProductOptionAssignments.Update(existing);
        }
        else
        {
            db.ProductOptionAssignments.Add(new ProductOptionAssignment
            {
                ProductId = productId,
                ProductOptionId = optionId,
                FeaturesJson = OptionFeatureJson.Serialize(selected),
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task DetachOptionAsync(Guid productId, Guid optionId, CancellationToken ct = default)
    {
        var assignment = await db.ProductOptionAssignments
            .FirstOrDefaultAsync(a => a.ProductId == productId && a.ProductOptionId == optionId, ct);
        if (assignment is not null) db.ProductOptionAssignments.Remove(assignment);
        await db.SaveChangesAsync(ct);
    }

    public async Task<GenerateVariantsResultDto> GenerateVariantsAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct)
            ?? throw new InvalidOperationException("Product not found");

        var assignments = await ListAssignmentsAsync(productId, ct);
        await DeleteProductVariantsAsync(productId, ct);

        if (assignments.Count == 0)
            return new GenerateVariantsResultDto(0, 0);

        var groups = assignments
            .Select(a => OptionFeatureJson.Deserialize(a.FeaturesJson))
            .Where(g => g.Count > 0)
            .ToList();

        if (groups.Count == 0 || groups.Any(g => g.Count == 0))
            return new GenerateVariantsResultDto(0, 0);

        var expected = groups.Aggregate(1, (acc, g) => acc * g.Count);
        var usedSkus = new HashSet<string>(
            await db.Variants.Select(v => v.Sku).ToListAsync(ct),
            StringComparer.Ordinal);

        var created = 0;
        foreach (var combo in Cartesian(groups))
        {
            var variant = new Variant
            {
                ProductId = product.Id,
                Sku = BuildSku(product, combo, usedSkus),
                Price = product.BasePrice,
                IsActive = true,
            };
            db.Variants.Add(variant);
            await db.SaveChangesAsync(ct);

            foreach (var feature in combo)
            {
                db.VariantOptionValues.Add(new VariantOptionValue
                {
                    VariantId = variant.Id,
                    OptionValueId = feature.Id,
                });
            }

            db.Inventories.Add(new Inventory
            {
                VariantId = variant.Id,
                QuantityOnHand = 0,
            });

            created++;
        }

        await db.SaveChangesAsync(ct);
        return new GenerateVariantsResultDto(created, expected);
    }

    public Task<List<Variant>> ListVariantsAsync(Guid productId, CancellationToken ct = default) =>
        db.Variants.AsNoTracking()
            .Include(v => v.Inventory)
            .Include(v => v.OptionValues).ThenInclude(ov => ov.OptionValue).ThenInclude(val => val.ProductOption)
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.Sku)
            .ToListAsync(ct);

    private async Task DeleteProductVariantsAsync(Guid productId, CancellationToken ct)
    {
        var variantIds = await db.Variants.Where(v => v.ProductId == productId).Select(v => v.Id).ToListAsync(ct);
        if (variantIds.Count == 0) return;

        await db.VariantOptionValues.Where(v => variantIds.Contains(v.VariantId)).ExecuteDeleteAsync(ct);
        await db.StockMovements.Where(m => variantIds.Contains(m.VariantId)).ExecuteDeleteAsync(ct);
        await db.Inventories.Where(i => variantIds.Contains(i.VariantId)).ExecuteDeleteAsync(ct);
        await db.Variants.Where(v => v.ProductId == productId).ExecuteDeleteAsync(ct);
    }

    private static string BuildSku(Product product, IReadOnlyList<OptionFeatureSnapshot> combo, HashSet<string> usedSkus)
    {
        var parts = combo
            .Select(f => new string(f.Value.Where(char.IsLetterOrDigit).Take(4).ToArray()))
            .Where(p => p.Length > 0)
            .ToList();
        var baseSku = parts.Count > 0
            ? $"{product.Slug[..Math.Min(12, product.Slug.Length)]}-{string.Join('-', parts)}".ToUpperInvariant()
            : $"VAR-{product.Id.ToString("N")[..8]}";

        var sku = baseSku;
        var suffix = 1;
        while (!usedSkus.Add(sku))
            sku = $"{baseSku}-{suffix++}";

        return sku;
    }

    private static IEnumerable<List<OptionFeatureSnapshot>> Cartesian(IReadOnlyList<List<OptionFeatureSnapshot>> groups)
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
                var row = new List<OptionFeatureSnapshot>(tail.Count + 1) { head };
                row.AddRange(tail);
                yield return row;
            }
        }
    }
}
