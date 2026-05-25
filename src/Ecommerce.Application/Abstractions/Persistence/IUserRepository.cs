using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task SaveRefreshTokenAsync(Guid userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default);
    Task<RefreshToken?> GetValidRefreshTokenAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeRefreshTokensAsync(Guid userId, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task<Guid?> GetRoleIdByCodeAsync(string roleCode, CancellationToken ct = default);
    Task AssignRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
}
