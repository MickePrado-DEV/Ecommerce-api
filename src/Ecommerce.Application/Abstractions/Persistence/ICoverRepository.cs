using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface ICoverRepository
{
    Task<List<Cover>> ListActiveAsync(CancellationToken ct = default);
    Task<List<Cover>> ListAllAsync(CancellationToken ct = default);
    Task<Cover?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Cover> SaveAsync(Cover cover, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReorderAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default);
}
