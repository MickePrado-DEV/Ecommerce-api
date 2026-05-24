using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class AdminCoverService(ICoverRepository repo) : IAdminCoverService
{
    public async Task<IReadOnlyList<CoverAdminDto>> ListAsync(CancellationToken ct = default)
    {
        var items = await repo.ListAllAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<CoverAdminDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var cover = await repo.GetAsync(id, ct);
        return cover is null ? null : Map(cover);
    }

    public async Task<CoverAdminDto> SaveAsync(SaveCoverRequest request, CancellationToken ct = default)
    {
        var entity = new Cover
        {
            Id = request.Id ?? Guid.Empty,
            Title = request.Title,
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };
        var saved = await repo.SaveAsync(entity, ct);
        return Map(saved);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default) => repo.DeleteAsync(id, ct);

    public Task ReorderAsync(ReorderCoversRequest request, CancellationToken ct = default) =>
        repo.ReorderAsync(request.Ids, ct);

    private static CoverAdminDto Map(Cover c) => new(c.Id, c.Title, c.ImageUrl, c.LinkUrl, c.SortOrder, c.IsActive);
}
