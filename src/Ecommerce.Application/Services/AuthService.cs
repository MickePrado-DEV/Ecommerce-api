using Ecommerce.Application.Abstractions;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services
{
    public class AuthService(IUserRepository users, IJwtTokenService jwt) : IAuthService
    {
        public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken ct = default)
        {
            var user = await users.GetByEmailWithRolesAsync(email, ct)
                ?? throw new NotFoundException("User", email);

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            var permissions = await users.GetPermissionsAsync(user.Id, ct);
            var access = jwt.GenerateAccessToken(user, permissions);
            var refresh = jwt.GenerateRefreshToken();
            await users.SaveRefreshTokenAsync(user.Id, refresh.Hash, refresh.ExpiresAt, ct);

            return new LoginResponse(
                access,
                refresh.Token,
                new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Roles),
                permissions);
        }

        public Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task LogoutAsync(Guid userId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<UserDto?> GetMeAsync(Guid userId, CancellationToken ct = default)
            => throw new NotImplementedException();
    }
}
