using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class CoverRepository(EcommerceDbContext db) : ICoverRepository
{
    public Task<List<Cover>> ListActiveAsync(CancellationToken ct = default) =>
        db.Covers.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(ct);

    public Task<List<Cover>> ListAllAsync(CancellationToken ct = default) =>
        db.Covers.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync(ct);

    public Task<Cover?> GetAsync(Guid id, CancellationToken ct = default) =>
        db.Covers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Cover> SaveAsync(Cover cover, CancellationToken ct = default)
    {
        if (cover.Id == Guid.Empty) db.Covers.Add(cover);
        else db.Covers.Update(cover);
        await db.SaveChangesAsync(ct);
        return cover;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var cover = await db.Covers.FindAsync([id], ct);
        if (cover is not null) db.Covers.Remove(cover);
        await db.SaveChangesAsync(ct);
    }

    public async Task ReorderAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        var covers = await db.Covers.Where(c => ids.Contains(c.Id)).ToListAsync(ct);
        for (var i = 0; i < ids.Count; i++)
        {
            var cover = covers.FirstOrDefault(c => c.Id == ids[i]);
            if (cover is not null) cover.SortOrder = i + 1;
        }
        await db.SaveChangesAsync(ct);
    }
}
