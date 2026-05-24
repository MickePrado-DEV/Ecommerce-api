using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class AuthService(IUserRepository users, IJwtTokenService jwt) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await users.GetByEmailWithRolesAsync(email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return await BuildLoginResponseAsync(user, ct);
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await users.EmailExistsAsync(request.Email, ct))
            throw new InvalidOperationException("El email ya está registrado");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        await users.CreateAsync(user, ct);
        user = (await users.GetByEmailWithRolesAsync(request.Email, ct))!;
        return await BuildLoginResponseAsync(user, ct);
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = HashToken(refreshToken);
        var stored = await users.GetValidRefreshTokenAsync(hash, ct);
        if (stored is null) return null;

        await users.RevokeRefreshTokensAsync(stored.UserId, ct);
        return await BuildLoginResponseAsync(stored.User, ct);
    }

    public Task LogoutAsync(Guid userId, CancellationToken ct = default) =>
        users.RevokeRefreshTokensAsync(userId, ct);

    public async Task<UserDto?> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.GetByIdWithRolesAsync(userId, ct);
        return user is null ? null : ToUserDto(user);
    }

    private async Task<LoginResponse> BuildLoginResponseAsync(User user, CancellationToken ct)
    {
        var permissions = await users.GetPermissionsAsync(user.Id, ct);
        var access = jwt.GenerateAccessToken(user, permissions);
        var refresh = jwt.GenerateRefreshToken();
        await users.SaveRefreshTokenAsync(user.Id, refresh.Hash, refresh.ExpiresAt, ct);
        return new LoginResponse(access, refresh.Token, ToUserDto(user), permissions);
    }

    private static UserDto ToUserDto(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Roles);

    private static string HashToken(string token) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(token)));
}
