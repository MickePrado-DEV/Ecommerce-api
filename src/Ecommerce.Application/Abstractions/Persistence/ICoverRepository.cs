using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface ICoverRepository
{
    Task<List<Cover>> ListActiveAsync(CancellationToken ct = default);
    Task<List<Cover>> ListAllAsync(CancellationToken ct = default);
    Task<(List<Cover> Items, int Total)> ListPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Cover?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Cover> SaveAsync(Cover cover, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReorderPrincipalAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default);
    Task<int> CountEffectiveActiveAsync(Guid? excludeId, CancellationToken ct = default);
    Task<int?> GetNextPrincipalOrderAsync(CancellationToken ct = default);
    Task DeactivateExpiredAsync(CancellationToken ct = default);
}
