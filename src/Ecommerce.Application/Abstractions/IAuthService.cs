using Ecommerce.Application.DTOs.Auth;

namespace Ecommerce.Application.Abstractions
{

    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken ct = default);
        Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default);
        Task LogoutAsync(Guid userId, CancellationToken ct = default);
        Task<UserDto?> GetMeAsync(Guid userId, CancellationToken ct = default);
    }

}
