using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user, IReadOnlyList<string> permissions);
        (string Token, string Hash, DateTime ExpiresAt) GenerateRefreshToken();
    }
}
