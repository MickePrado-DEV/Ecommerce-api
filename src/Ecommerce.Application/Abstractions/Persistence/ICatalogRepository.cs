using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface ICatalogRepository
{
    Task<List<Family>> GetFamiliesTreeAsync(CancellationToken ct = default);
}
