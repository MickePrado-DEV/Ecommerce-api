using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IAdminRoleRepository
{
    Task<List<Role>> ListAsync(CancellationToken ct = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Permission>> ListPermissionsAsync(CancellationToken ct = default);
    Task UpdatePermissionsAsync(Guid roleId, IReadOnlyList<string> permissionCodes, CancellationToken ct = default);
}
