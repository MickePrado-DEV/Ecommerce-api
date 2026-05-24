using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class ProductOptionRepository(EcommerceDbContext db) : IProductOptionRepository
{
    public Task<List<ProductOption>> ListByProductAsync(Guid productId, CancellationToken ct = default) =>
        db.ProductOptions.AsNoTracking().Include(o => o.Values)
            .Where(o => o.ProductId == productId).OrderBy(o => o.SortOrder).ToListAsync(ct);

    public Task<ProductOption?> GetAsync(Guid optionId, Guid productId, CancellationToken ct = default) =>
        db.ProductOptions.Include(o => o.Values)
            .FirstOrDefaultAsync(o => o.Id == optionId && o.ProductId == productId, ct);

    public async Task<ProductOption> SaveOptionAsync(ProductOption option, CancellationToken ct = default)
    {
        if (option.Id == Guid.Empty) db.ProductOptions.Add(option);
        else db.ProductOptions.Update(option);
        await db.SaveChangesAsync(ct);
        return option;
    }

    public async Task DeleteOptionAsync(Guid optionId, Guid productId, CancellationToken ct = default)
    {
        var option = await db.ProductOptions.FirstOrDefaultAsync(o => o.Id == optionId && o.ProductId == productId, ct);
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
}
