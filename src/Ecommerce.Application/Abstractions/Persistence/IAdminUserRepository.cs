using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IAdminUserRepository
{
    Task<(IReadOnlyList<User> Items, int Total)> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Guid id, bool isActive, IReadOnlyList<string> roleCodes, CancellationToken ct = default);
}
