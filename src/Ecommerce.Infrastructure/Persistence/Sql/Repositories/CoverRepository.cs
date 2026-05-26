using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Covers;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class CoverRepository(EcommerceDbContext db) : ICoverRepository
{
    public Task<List<Cover>> ListActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return db.Covers.AsNoTracking()
            .Where(c => c.IsActive && (!c.EndsAt.HasValue || c.EndsAt >= now) && (!c.StartsAt.HasValue || c.StartsAt <= now))
            .Where(c => c.SortOrder >= 1 && c.SortOrder <= CoverRules.MaxPrincipalActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);
    }

    public Task<List<Cover>> ListAllAsync(CancellationToken ct = default) =>
        db.Covers.AsNoTracking().OrderByDescending(c => c.UpdatedAt).ToListAsync(ct);

    public async Task<(List<Cover> Items, int Total)> ListPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var q = db.Covers.AsNoTracking().OrderByDescending(c => c.UpdatedAt);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<Cover?> GetAsync(Guid id, CancellationToken ct = default) =>
        db.Covers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Cover> SaveAsync(Cover cover, CancellationToken ct = default)
    {
        if (cover.Id == Guid.Empty)
        {
            cover.Id = Guid.NewGuid();
            db.Covers.Add(cover);
        }
        else
        {
            var tracked = await db.Covers.FindAsync([cover.Id], ct);
            if (tracked is null)
            {
                db.Covers.Add(cover);
            }
            else
            {
                tracked.Title = cover.Title;
                tracked.ImageUrl = cover.ImageUrl;
                tracked.LinkUrl = cover.LinkUrl;
                tracked.SortOrder = cover.SortOrder;
                tracked.IsActive = cover.IsActive;
                tracked.StartsAt = cover.StartsAt;
                tracked.EndsAt = cover.EndsAt;
                cover = tracked;
            }
        }

        await db.SaveChangesAsync(ct);
        return cover;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var cover = await db.Covers.FindAsync([id], ct);
        if (cover is not null) db.Covers.Remove(cover);
        await db.SaveChangesAsync(ct);
    }

    public async Task ReorderPrincipalAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count > CoverRules.MaxPrincipalActive)
            throw new InvalidOperationException($"Máximo {CoverRules.MaxPrincipalActive} portadas principales.");

        var now = DateTime.UtcNow;
        var all = await db.Covers.ToListAsync(ct);
        var idSet = ids.ToHashSet();

        for (var i = 0; i < ids.Count; i++)
        {
            var cover = all.FirstOrDefault(c => c.Id == ids[i])
                ?? throw new InvalidOperationException($"Portada {ids[i]} no encontrada.");
            if (!CoverRules.IsEffectivelyActive(cover, now))
                throw new InvalidOperationException($"La portada «{cover.Title}» no está activa o vigente.");
            cover.SortOrder = i + 1;
        }

        foreach (var cover in all.Where(c => c.SortOrder > 0 && !idSet.Contains(c.Id)))
            cover.SortOrder = 0;

        await db.SaveChangesAsync(ct);
    }

    public async Task<int> CountEffectiveActiveAsync(Guid? excludeId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var q = db.Covers.AsNoTracking()
            .Where(c => c.IsActive && (!c.EndsAt.HasValue || c.EndsAt >= now) && (!c.StartsAt.HasValue || c.StartsAt <= now));
        if (excludeId.HasValue)
            q = q.Where(c => c.Id != excludeId.Value);
        return await q.CountAsync(ct);
    }

    public async Task<int?> GetNextPrincipalOrderAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var used = await db.Covers.AsNoTracking()
            .Where(c => c.IsActive && (!c.EndsAt.HasValue || c.EndsAt >= now) && (!c.StartsAt.HasValue || c.StartsAt <= now))
            .Where(c => c.SortOrder >= 1 && c.SortOrder <= CoverRules.MaxPrincipalActive)
            .Select(c => c.SortOrder)
            .ToListAsync(ct);

        for (var i = 1; i <= CoverRules.MaxPrincipalActive; i++)
        {
            if (!used.Contains(i))
                return i;
        }

        return null;
    }

    public async Task DeactivateExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var expired = await db.Covers
            .Where(c => c.IsActive && c.EndsAt != null && c.EndsAt < now)
            .ToListAsync(ct);

        if (expired.Count == 0) return;

        foreach (var cover in expired)
        {
            cover.IsActive = false;
            cover.SortOrder = 0;
        }

        await db.SaveChangesAsync(ct);
    }
}
